using System.Net;
using System.Text.RegularExpressions;
using Feedlot.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Feedlot.Infrastructure.Services;

/// <summary>
/// Servicio HTTP que se autentica en SUBAGAN y extrae datos de subastas.
///
/// Flujo:
///   1. GET /subagan/users/login     → extrae CSRF token del meta tag
///   2. POST /subagan/users/login    → obtiene session cookie
///   3. GET /subagan/filterLots?...  → pagina los lotes con precios vendidos
/// </summary>
public sealed class SubaganHttpService : ISubaganHttpService
{
    private readonly ILogger<SubaganHttpService> _logger;
    private readonly string _baseUrl;
    private readonly string _usuario;
    private readonly string _password;

    private HttpClient? _client;
    private bool _authenticated;

    public SubaganHttpService(IConfiguration configuration, ILogger<SubaganHttpService> logger)
    {
        _logger = logger;
        var s = configuration.GetSection("SubaganSettings");
        _baseUrl  = s["BaseUrl"]  ?? "https://www.subaganenvivo.co";
        _usuario  = s["Usuario"]  ?? throw new InvalidOperationException("SubaganSettings:Usuario no configurado.");
        _password = s["Password"] ?? throw new InvalidOperationException("SubaganSettings:Password no configurado.");
    }

    // ── Autenticación ─────────────────────────────────────────────────────────

    public async Task<bool> LoginAsync(CancellationToken ct = default)
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true,
            UseCookies = true
        };

        _client = new HttpClient(handler) { BaseAddress = new Uri(_baseUrl) };
        _client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        try
        {
            // 1. Obtener CSRF token desde la página de login
            var loginPage = await _client.GetStringAsync("/subagan/users/login", ct);
            var csrfToken = ExtractCsrfToken(loginPage);

            if (string.IsNullOrEmpty(csrfToken))
            {
                _logger.LogWarning("SUBAGAN — No se encontró CSRF token.");
                return false;
            }

            // 2. POST credenciales
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["login"]     = _usuario,
                ["password"]  = _password,
                ["csrfToken"] = csrfToken
            });

            var response = await _client.PostAsync("/subagan/users/login", form, ct);
            var finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? "";
            _authenticated = !finalUrl.Contains("/login");

            _logger.LogInformation("SUBAGAN — Login {Result}. URL final: {Url}",
                _authenticated ? "exitoso" : "fallido", finalUrl);

            return _authenticated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SUBAGAN — Error al autenticar.");
            return false;
        }
    }

    // ── Lotes ─────────────────────────────────────────────────────────────────

    public async Task<List<SubaganLoteData>> ObtenerLotesAsync(int eventId, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        var todos = new List<SubaganLoteData>();
        int page = 1;
        const int pageSize = 200;

        while (true)
        {
            var url = BuildFilterLotsUrl(eventId, page, pageSize);
            var html = await _client!.GetStringAsync(url, ct);
            var lotes = ParseLotes(html);

            todos.AddRange(lotes);
            _logger.LogInformation("SUBAGAN evento {Id} — página {P}: {N} lotes", eventId, page, lotes.Count);

            if (lotes.Count < pageSize) break;
            page++;
        }

        _logger.LogInformation("SUBAGAN evento {Id} — total: {Total} lotes", eventId, todos.Count);
        return todos;
    }

    // ── Parsers ───────────────────────────────────────────────────────────────

    private static List<SubaganLoteData> ParseLotes(string html)
    {
        var lotes = new List<SubaganLoteData>();
        var blocks = Regex.Split(html, @"(?=<strong>Lote #\d+</strong>)");

        foreach (var block in blocks)
        {
            if (!block.Contains("Vendido a $")) continue;
            try
            {
                var lote = ParseLoteBlock(block);
                if (lote != null) lotes.Add(lote);
            }
            catch { /* saltar bloques malformados */ }
        }

        return lotes;
    }

    private static SubaganLoteData? ParseLoteBlock(string block)
    {
        var numM = Regex.Match(block, @"Lote #(\d+)");
        if (!numM.Success) return null;
        int numeroLote = int.Parse(numM.Groups[1].Value);

        var precioM = Regex.Match(block, @"Vendido a \$([0-9,\.]+)");
        if (!precioM.Success) return null;
        decimal precio = decimal.Parse(precioM.Groups[1].Value.Replace(",", "").Replace(".", ""));

        var tipoM = Regex.Match(block, @"<strong>Tipo:</strong>\s*([A-Z]+)\s*-\s*([^<]+)");
        if (!tipoM.Success) return null;
        string codigo     = tipoM.Groups[1].Value.Trim();
        string descripcion = tipoM.Groups[2].Value.Trim();

        var cantM     = Regex.Match(block, @"<strong>Cantidad:</strong>\s*(\d+)");
        int cantidad  = cantM.Success ? int.Parse(cantM.Groups[1].Value) : 1;

        var ptM       = Regex.Match(block, @"<strong>Peso Total:</strong>\s*([\d,\.]+)\s*Kg");
        decimal pt    = ptM.Success ? ParsePeso(ptM.Groups[1].Value) : 0;

        var ppM       = Regex.Match(block, @"<strong>Peso Pro:</strong>\s*([\d,\.]+)\s*Kg");
        decimal pp    = ppM.Success ? ParsePeso(ppM.Groups[1].Value) : 0;

        var procM     = Regex.Match(block, @"<strong>Procedencia:</strong>\s*([^<]+)");
        string proc   = procM.Success ? procM.Groups[1].Value.Trim() : "";

        var fechaM    = Regex.Match(block, @"<strong>Hora de pesaje:</strong>\s*(\d{1,2}-\w+-\d{4})");
        var fecha     = fechaM.Success ? ParseFecha(fechaM.Groups[1].Value) : DateOnly.FromDateTime(DateTime.Today);

        var obsM      = Regex.Match(block, @"<strong>Observaciones:</strong>\s*([^<]+)");
        string? obs   = obsM.Success ? obsM.Groups[1].Value.Trim() : null;
        if (obs == "---") obs = null;

        var lotIdM    = Regex.Match(block, @"downloadLot\?lotId=(\d+)");
        int loteId    = lotIdM.Success ? int.Parse(lotIdM.Groups[1].Value) : 0;

        return new SubaganLoteData(
            loteId, numeroLote, codigo, descripcion,
            cantidad, pt, pp, precio, proc, obs, fecha);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string BuildFilterLotsUrl(int eventId, int page, int pageSize)
        => $"/subagan/filterLots?eventId={eventId}&pageSelected={page}&sizePage={pageSize}" +
           $"&filterLot=&filterSex=&filterType=&filterState=" +
           $"&filterAverageWeight=1%3B1600&filterTotalAnimals=1%3B99" +
           $"&guideNumber=&myAutoBid=undefined";

    private static string ExtractCsrfToken(string html)
    {
        var m = Regex.Match(html, @"<meta name=""csrf-token"" content=""([^""]+)""");
        return m.Success ? m.Groups[1].Value : string.Empty;
    }

    private static decimal ParsePeso(string value)
        => decimal.Parse(value.Replace(",", "").Replace(".", ""));

    private static DateOnly ParseFecha(string fecha)
    {
        // "27-may.-2026"
        var meses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["ene"]=1,["feb"]=2,["mar"]=3,["abr"]=4,["may"]=5,["jun"]=6,
            ["jul"]=7,["ago"]=8,["sep"]=9,["oct"]=10,["nov"]=11,["dic"]=12
        };
        var p = fecha.Split('-');
        if (p.Length < 3) return DateOnly.FromDateTime(DateTime.Today);
        string mesKey = p[1].Replace(".", "").ToLower();
        return meses.TryGetValue(mesKey, out int mes)
            ? new DateOnly(int.Parse(p[2]), mes, int.Parse(p[0]))
            : DateOnly.FromDateTime(DateTime.Today);
    }

    private void EnsureAuthenticated()
    {
        if (!_authenticated || _client is null)
            throw new InvalidOperationException("Debe llamar LoginAsync() antes.");
    }
}
