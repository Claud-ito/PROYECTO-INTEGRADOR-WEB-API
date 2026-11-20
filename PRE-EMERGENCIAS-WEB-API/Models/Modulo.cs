namespace PRE_EMERGENCIAS_WEB_API.Models;

public class Modulo
{
    public int ModuloId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public ICollection<Piso> Pisos { get; set; } = new List<Piso>();
}
