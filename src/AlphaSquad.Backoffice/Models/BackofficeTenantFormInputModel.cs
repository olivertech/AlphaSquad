using System.ComponentModel.DataAnnotations;

namespace AlphaSquad.Backoffice.Models;

/// <summary>
/// Representa o wizard basico de onboarding de uma academia no backoffice.
/// </summary>
public sealed class BackofficeTenantFormInputModel
{
    [Required(ErrorMessage = "Informe o nome da academia.")]
    [Display(Name = "Nome da academia")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome curto da academia.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cor primaria.")]
    [Display(Name = "Cor primaria")]
    public string PrimaryColor { get; set; } = "#2563eb";

    [Required(ErrorMessage = "Informe a cor secundaria.")]
    [Display(Name = "Cor secundaria")]
    public string SecondaryColor { get; set; } = "#14b8a6";

    [Required(ErrorMessage = "Informe o nome do administrador inicial.")]
    [Display(Name = "Nome do administrador inicial")]
    public string AdminName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail do administrador inicial.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
    [Display(Name = "E-mail do administrador inicial")]
    public string AdminEmail { get; set; } = string.Empty;

    [Display(Name = "Ativa")]
    public bool IsActive { get; set; } = true;

    public List<string> FeatureCodes { get; set; } = [];
}
