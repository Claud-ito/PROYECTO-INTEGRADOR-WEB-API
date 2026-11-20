namespace PRE_EMERGENCIAS_WEB_API.Dtos.Modulos;

public sealed record ModuloRespuestaDto(
    int ModuloId,
    string Codigo,
    string Nombre,
    bool Activo);
