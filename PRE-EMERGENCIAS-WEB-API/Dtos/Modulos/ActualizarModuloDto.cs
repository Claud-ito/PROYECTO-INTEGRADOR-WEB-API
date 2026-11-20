using System.ComponentModel.DataAnnotations;

namespace PRE_EMERGENCIAS_WEB_API.Dtos.Modulos;

public sealed class ActualizarModuloDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código no puede superar 20 caracteres.")]
    public string Codigo { get; init; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Nombre { get; init; } = string.Empty;

    [Required(ErrorMessage = "El estado activo es obligatorio.")]
    public bool? Activo { get; init; }
}
