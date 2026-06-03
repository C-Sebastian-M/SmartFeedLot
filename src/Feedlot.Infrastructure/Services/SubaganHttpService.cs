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
                var snippet = loginPage.Length > 600 ? loginPage[..600] : loginPage;
                _logger.LogWarning(
                    "SUBAGAN — No se encontró CSRF token. Longitud de la respuesta: {Len}. Inicio del HTML: {Snippet}",
                    loginPage.Length, snippet);
                return false;
            }

            // 2. POST credenciales.
            // El formulario real (id="loginForm") postea a /subagan/users/authenticate
            // con enctype="multipart/form-data", y el token va en el campo "csrfToken".
            var form = new MultipartFormDataContent
            {
                { new StringContent(csrfToken), "csrfToken" },
                { new StringContent(_usuario),  "login" },
                { new StringContent(_password), "password" }
            };

            var response = await _client.PostAsync("/subagan/users/authenticate", form, ct);
            var finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? "";
            // Login OK si ya no estamos en login ni en la acción authenticate (redirige al panel).
            _authenticated = !finalUrl.Contains("/login") && !finalUrl.Contains("/authenticate");

            if (_authenticated)
            {
                _logger.LogInformation("SUBAGAN — Login exitoso. URL final: {Url}", finalUrl);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                var snippet = ExtractMensajeError(body);
                _logger.LogWarning(
                    "SUBAGAN — Login fallido. Status: {Status}. URL final: {Url}. Mensaje del sitio: {Msg}",
                    (int)response.StatusCode, finalUrl, snippet);
            }

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

    // ── Calendario de eventos ───────────────────────────────────────────────────

    public async Task<List<SubaganEventoCalendarioData>> ObtenerEventosCalendarioAsync(CancellationToken ct = default)
    {
        EnsureAuthenticated();

        var html = await _client!.GetStringAsync("/subagan/showCalendarEvents", ct);
        var eventos = ParseEventosCalendario(html);

        _logger.LogInformation("SUBAGAN — calendario: {N} eventos encontrados", eventos.Count);
        return eventos;
    }

    /// <summary>
    /// Extrae los eventos del array JS "eventColors" embebido en el HTML del calendario.
    /// Cada objeto tiene la forma: { id: '1208', title: '...', start: '2026-05-27', ... }
    /// </summary>
    private static List<SubaganEventoCalendarioData> ParseEventosCalendario(string html)
    {
        var eventos = new List<SubaganEventoCalendarioData>();

        // Captura cada bloque { id: '...', title: '...', start: '...' } tolerando
        // espacios, saltos de línea y campos adicionales entre ellos.
        var rx = new Regex(
            @"\{\s*id:\s*'(?<id>\d+)'\s*,\s*title:\s*'(?<title>(?:[^'\\]|\\.)*)'\s*,\s*start:\s*'(?<start>\d{4}-\d{2}-\d{2})'",
            RegexOptions.Singleline);

        foreach (Match m in rx.Matches(html))
        {
            if (!int.TryParse(m.Groups["id"].Value, out var eventId)) continue;
            if (!DateOnly.TryParse(m.Groups["start"].Value, out var fecha)) continue;

            // Desescapar comillas/barras del título JS y normalizar espacios.
            var titulo = m.Groups["title"].Value
                .Replace("\\'", "'")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Trim();
            titulo = Regex.Replace(titulo, @"\s+", " ");

            eventos.Add(new SubaganEventoCalendarioData(eventId, titulo, fecha));
        }

        return eventos;
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
        // SUBAGAN puede emitir el token de varias formas. Probamos en orden,
        // tolerando cualquier orden de atributos y comillas simples o dobles.

        // 1. <meta name="csrf-token" content="TOKEN">
        var m = Regex.Match(html,
            @"<meta\b[^>]*\bname\s*=\s*[""']csrf-token[""'][^>]*\bcontent\s*=\s*[""']([^""']+)[""']",
            RegexOptions.IgnoreCase);
        if (m.Success) return m.Groups[1].Value;

        // 2. <meta content="TOKEN" name="csrf-token">  (atributos en orden inverso)
        m = Regex.Match(html,
            @"<meta\b[^>]*\bcontent\s*=\s*[""']([^""']+)[""'][^>]*\bname\s*=\s*[""']csrf-token[""']",
            RegexOptions.IgnoreCase);
        if (m.Success) return m.Groups[1].Value;

        // 3. <input ... name="_csrfToken|csrfToken|_token" value="TOKEN"> (campo oculto del form)
        m = Regex.Match(html,
            @"<input\b[^>]*\bname\s*=\s*[""'](?:_csrfToken|csrfToken|_csrf|_token)[""'][^>]*\bvalue\s*=\s*[""']([^""']+)[""']",
            RegexOptions.IgnoreCase);
        if (m.Success) return m.Groups[1].Value;

        // 4. input con value antes que name
        m = Regex.Match(html,
            @"<input\b[^>]*\bvalue\s*=\s*[""']([^""']+)[""'][^>]*\bname\s*=\s*[""'](?:_csrfToken|csrfToken|_csrf|_token)[""']",
            RegexOptions.IgnoreCase);
        if (m.Success) return m.Groups[1].Value;

        return string.Empty;
    }

    /// <summary>
    /// Intenta extraer el mensaje de error/flash que muestra el sitio tras un login fallido,
    /// para distinguir entre credenciales inválidas, token rechazado, captcha, etc.
    /// </summary>
    private static string ExtractMensajeError(string html)
    {
        // CakePHP renderiza los flash messages en <div class="message ...">TEXTO</div>
        var m = Regex.Match(html,
            @"<div[^>]*class\s*=\s*[""'][^""']*\b(?:message|alert|error|flash)\b[^""']*[""'][^>]*>(.*?)</div>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (m.Success)
        {
            var texto = Regex.Replace(m.Groups[1].Value, "<[^>]+>", " ").Trim();
            texto = Regex.Replace(texto, @"\s+", " ");
            if (!string.IsNullOrWhiteSpace(texto))
                return texto.Length > 200 ? texto[..200] : texto;
        }
        return "(sin mensaje detectable; longitud respuesta: " + html.Length + ")";
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
