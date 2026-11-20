namespace PRE_EMERGENCIAS_WEB_API.Models;

public class Piso
{
    public int PisoId { get; set; }
    public int ModuloId { get; set; }
    public string Denominacion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public Modulo Modulo { get; set; } = null!;
}
