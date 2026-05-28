namespace Feedlot.Application.DTOs;

public sealed record ProveedorDto(
    Guid Id,
    string Nombre,
    string? Contacto,
    string? Telefono,
    string? Email);
