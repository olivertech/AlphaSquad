using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Users;

/// <summary>
/// Centraliza a transformação dos contratos da API em textos e modelos amigáveis para o dashboard.
/// Isso evita duplicar regras de apresentação em várias Razor Pages.
/// </summary>
public static class UserPresentationMapper
{
    public static IReadOnlyList<UserRoleOptionViewModel> RoleOptions { get; } =
    [
        new UserRoleOptionViewModel
        {
            Value = 1,
            Label = "Administrador",
            Description = "Pode acessar todas as áreas administrativas e configurar a academia."
        },
        new UserRoleOptionViewModel
        {
            Value = 2,
            Label = "Professor",
            Description = "Acompanha a operação e apoia rotinas pedagógicas do dia a dia."
        },
        new UserRoleOptionViewModel
        {
            Value = 3,
            Label = "Aluno",
            Description = "Participa do ecossistema pelo app, com plano ativo e experiências da academia."
        }
    ];

    /// <summary>
    /// Converte um usuário retornado pela API em item pronto para a listagem do dashboard.
    /// </summary>
    public static UserListItemViewModel ToListItem(UserResponseDto user)
    {
        return new UserListItemViewModel
        {
            Id = user.Id ?? Guid.Empty,
            Name = string.IsNullOrWhiteSpace(user.Name) ? "Usuário sem nome" : user.Name,
            Email = user.Email ?? "E-mail não informado",
            RoleValue = user.Role,
            RoleLabel = ToRoleLabel(user.Role),
            StatusLabel = user.IsActive == true ? "Ativo" : "Inativo",
            IsActive = user.IsActive == true,
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Constrói a visão detalhada de um usuário com rótulos mais explicativos para a tela de detalhes.
    /// </summary>
    public static UserDetailsViewModel ToDetails(UserResponseDto user)
    {
        var name = string.IsNullOrWhiteSpace(user.Name) ? "Usuário sem nome" : user.Name;

        return new UserDetailsViewModel
        {
            Id = user.Id ?? Guid.Empty,
            Name = name,
            Email = user.Email ?? "E-mail não informado",
            RoleLabel = ToRoleLabel(user.Role),
            RoleDescription = ToRoleDescription(user.Role),
            StatusLabel = user.IsActive == true ? "Ativo" : "Inativo",
            IsActive = user.IsActive == true,
            Initials = BuildInitials(name),
            CreatedAt = user.CreatedAt
        };
    }

    public static string ToRoleLabel(int? role)
    {
        return role switch
        {
            1 => "Administrador",
            2 => "Professor",
            3 => "Aluno",
            _ => "Perfil não identificado"
        };
    }

    public static string ToRoleDescription(int? role)
    {
        return role switch
        {
            1 => "Controla o painel, define parâmetros da academia e acessa módulos administrativos sensíveis.",
            2 => "Apoia o funcionamento da academia com foco em aulas, operação e acompanhamento de alunos.",
            3 => "Usa o app da academia para treinos, check-ins, comunidade e experiências do ecossistema.",
            _ => "Este perfil ainda não possui uma descrição de uso definida."
        };
    }

    public static string BuildInitials(string? name)
    {
        var normalizedName = string.IsNullOrWhiteSpace(name) ? "Usuário" : name;

        return string.Concat(
            normalizedName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part => char.ToUpperInvariant(part[0])));
    }
}
