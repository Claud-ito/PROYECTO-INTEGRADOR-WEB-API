using System.ComponentModel.DataAnnotations;

namespace PRE_EMERGENCIAS_WEB_API.Dtos.Pisos;

/// <summary>Datos para registrar un piso. El PisoId lo genera SQL Server.</summary>
public class CrearPisoDto
{
    /// <summary>Identificador del módulo al que pertenece el piso. Debe existir y estar activo.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ModuloId debe ser un número positivo.")]
    public int ModuloId { get; set; }

    /// <summary>Nombre del piso dentro del módulo, por ejemplo "Piso 1". Único en su módulo.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "La denominación es obligatoria.")]
    [StringLength(50, ErrorMessage = "La denominación admite como máximo 50 caracteres.")]
    public string Denominacion { get; set; } = string.Empty;
}
