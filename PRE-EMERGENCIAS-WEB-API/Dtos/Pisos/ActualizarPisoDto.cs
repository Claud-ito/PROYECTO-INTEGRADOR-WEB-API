using System.ComponentModel.DataAnnotations;

namespace PRE_EMERGENCIAS_WEB_API.Dtos.Pisos;

/// <summary>
/// Datos completos para modificar un piso. El PisoId va en la ruta.
/// Activo=false registra la baja lógica del piso sin eliminarlo.
/// </summary>
public class ActualizarPisoDto
{
    /// <summary>Módulo al que pertenece el piso. Para trasladarlo, el módulo destino debe estar activo.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ModuloId debe ser un número positivo.")]
    public int ModuloId { get; set; }

    /// <summary>Nombre del piso dentro del módulo. Único en su módulo.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "La denominación es obligatoria.")]
    [StringLength(50, ErrorMessage = "La denominación admite como máximo 50 caracteres.")]
    public string Denominacion { get; set; } = string.Empty;

    /// <summary>Estado del piso. Obligatorio: false indica baja lógica.</summary>
    [Required(ErrorMessage = "El campo Activo es obligatorio.")]
    public bool? Activo { get; set; }
}
