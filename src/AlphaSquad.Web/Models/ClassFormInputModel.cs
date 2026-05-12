using System.ComponentModel.DataAnnotations;

namespace AlphaSquad.Web.Models;

/// <summary>
/// Formulario de aulas usado no dashboard administrativo.
/// Mantem os campos em formato amigavel para o navegador e deixa a conversao para a API concentrada na camada de pagina.
/// </summary>
public sealed class ClassFormInputModel : IValidatableObject
{
    [Required(ErrorMessage = "Informe o nome da aula.")]
    [StringLength(120, ErrorMessage = "O nome da aula deve ter no maximo 120 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descricao deve ter no maximo 500 caracteres.")]
    public string? Description { get; set; }

    [StringLength(120, ErrorMessage = "O local deve ter no maximo 120 caracteres.")]
    public string? Location { get; set; }

    [Range(1, 1000, ErrorMessage = "A capacidade deve ser maior que zero.")]
    public int? Capacity { get; set; }

    [Required(ErrorMessage = "Informe a data e o horario de inicio.")]
    public DateTime? StartsAt { get; set; }

    [Required(ErrorMessage = "Informe a data e o horario de termino.")]
    public DateTime? EndsAt { get; set; }

    public Guid? InstructorUserId { get; set; }
    public bool IsSpecialClass { get; set; }
    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartsAt.HasValue && EndsAt.HasValue && EndsAt.Value <= StartsAt.Value)
        {
            yield return new ValidationResult(
                "O horario de termino deve ser maior que o horario de inicio.",
                [nameof(EndsAt)]);
        }
    }
}
