using System.ComponentModel.DataAnnotations;

namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa os dados digitados pelo administrador ao criar ou editar um usuário.
/// O mesmo modelo base é reutilizado nas telas para manter consistência visual e de validação.
/// </summary>
public sealed class UserFormInputModel
{
    [Required(ErrorMessage = "Informe o nome do usuário.")]
    [Display(Name = "Nome completo")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail do usuário.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Senha inicial")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o perfil de acesso.")]
    [Display(Name = "Perfil")]
    public int? Role { get; set; }

    [Display(Name = "Usuário ativo")]
    public bool IsActive { get; set; } = true;
}
