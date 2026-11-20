namespace PRE_EMERGENCIAS_WEB_API.Dtos.Pisos;

/// <summary>Piso devuelto por la API junto con los datos básicos de su módulo.</summary>
public class PisoRespuestaDto
{
    public int PisoId { get; set; }
    public int ModuloId { get; set; }
    public string ModuloCodigo { get; set; } = string.Empty;
    public string ModuloNombre { get; set; } = string.Empty;
    public string Denominacion { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
