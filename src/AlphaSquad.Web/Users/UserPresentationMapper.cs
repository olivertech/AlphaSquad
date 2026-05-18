using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Web.Models;
using System.Globalization;

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
            PhoneNumber = string.IsNullOrWhiteSpace(user.PhoneNumber) ? "Não informado" : user.PhoneNumber,
            BirthDate = FormatBirthDate(user.BirthDate),
            ActiveMembershipPlanName = BuildMembershipPlanName(user),
            MembershipBillingDueDayLabel = BuildMembershipBillingDueDayLabel(user),
            MembershipFinancialStatusLabel = BuildMembershipFinancialStatusLabel(user),
            MembershipFinancialStatusDescription = BuildMembershipFinancialStatusDescription(user),
            MembershipFinancialStatusToneClass = BuildMembershipFinancialStatusToneClass(user),
            RoleLabel = ToRoleLabel(user.Role),
            RoleDescription = ToRoleDescription(user.Role),
            StatusLabel = user.IsActive == true ? "Ativo" : "Inativo",
            GamificationStatusText = BuildGamificationStatusText(user),
            IsActive = user.IsActive == true,
            IsGamificationParticipant = user.IsGamificationParticipant == true,
            Initials = BuildInitials(name),
            TotalAccumulatedPoints = Convert.ToDecimal(user.TotalAccumulatedPoints ?? 0d),
            CreatedAt = user.CreatedAt
        };
    }

    public static MembershipPlanOptionViewModel ToMembershipPlanOption(MembershipPlanResponseDto plan)
    {
        var price = Convert.ToDecimal(plan.Price ?? 0d);
        var durationDays = plan.DurationDays ?? 0;

        return new MembershipPlanOptionViewModel
        {
            Id = plan.Id ?? Guid.Empty,
            IsActive = plan.IsActive == true,
            Label = $"{plan.Name} • {price:C} • {durationDays} dias{(plan.IsActive == true ? string.Empty : " (inativo)")}"
        };
    }

    public static UserGamificationHistoryViewModel ToGamificationHistory(UserGamificationHistoryResponseDto history)
    {
        var totalPoints = Convert.ToDecimal(history.TotalAccumulatedPoints ?? 0d);
        var isParticipant = history.IsGamificationParticipant == true;
        var entries = history.Entries?
            .Select(entry => new UserGamificationHistoryEntryViewModel
            {
                Id = entry.Id ?? Guid.Empty,
                EventTypeLabel = ToGamificationEventTypeLabel(entry.EventType),
                RuleName = string.IsNullOrWhiteSpace(entry.RuleName) ? "Regra não identificada" : entry.RuleName,
                PointsApplied = Convert.ToDecimal(entry.PointsApplied ?? 0d),
                SourceLabel = string.IsNullOrWhiteSpace(entry.SourceLabel) ? "Atividade" : entry.SourceLabel,
                SourceTitle = entry.SourceTitle,
                OccurredAt = entry.OccurredAt,
                Notes = entry.Notes
            })
            .ToList() ?? [];

        return new UserGamificationHistoryViewModel
        {
            UserId = history.UserId ?? Guid.Empty,
            UserName = string.IsNullOrWhiteSpace(history.UserName) ? "Usuário" : history.UserName,
            IsGamificationParticipant = isParticipant,
            TotalAccumulatedPoints = totalPoints,
            SummaryText = !isParticipant
                ? "Este perfil não participa da gamificação do app."
                : totalPoints <= 0
                    ? "Este aluno ainda não pontuou."
                    : $"Este aluno acumula {totalPoints:N2} ponto(s) e abaixo estão os registros que formam esse saldo.",
            Entries = entries
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

    private static string? FormatBirthDate(string? birthDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate))
            return "Não informado";

        var acceptedFormats = new[] { "yyyy-MM-dd", "dd/MM/yyyy" };
        if (!DateTime.TryParseExact(birthDate, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return birthDate;

        return parsed.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    private static string BuildGamificationStatusText(UserResponseDto user)
    {
        if (user.IsGamificationParticipant != true)
            return "Este perfil não participa da gamificação do app.";

        var totalPoints = Convert.ToDecimal(user.TotalAccumulatedPoints ?? 0d);
        return totalPoints <= 0
            ? "Este aluno ainda não pontuou."
            : $"{totalPoints:N2} ponto(s) acumulado(s) na gamificação.";
    }

    private static string BuildMembershipPlanName(UserResponseDto user)
    {
        if (user.IsGamificationParticipant != true)
            return "Não se aplica a este perfil";

        return string.IsNullOrWhiteSpace(user.ActiveMembershipPlanName)
            ? "Sem plano ativo"
            : user.ActiveMembershipPlanName;
    }

    private static string BuildMembershipBillingDueDayLabel(UserResponseDto user)
    {
        if (user.IsGamificationParticipant != true)
            return "Não se aplica";

        return user.MembershipBillingDueDay.HasValue
            ? $"Todo dia {user.MembershipBillingDueDay.Value:00}"
            : "Não definido";
    }

    private static string BuildMembershipFinancialStatusLabel(UserResponseDto user)
    {
        if (user.IsGamificationParticipant != true)
            return "Não se aplica";

        if (!user.ActiveMembershipPlanId.HasValue)
            return "Sem plano ativo";

        return user.IsMembershipInGoodStanding == true ? "Em dia" : "Em atraso";
    }

    private static string BuildMembershipFinancialStatusDescription(UserResponseDto user)
    {
        if (user.IsGamificationParticipant != true)
            return "Perfis administrativos e professores não possuem assinatura de plano.";

        if (!user.ActiveMembershipPlanId.HasValue)
            return "Este aluno ainda não possui um plano ativo vinculado ao cadastro.";

        return user.IsMembershipInGoodStanding == true
            ? "A mensalidade do ciclo atual está em dia dentro do vencimento informado."
            : "O vencimento do ciclo atual já passou e ainda não há pagamento em dia registrado.";
    }

    private static string BuildMembershipFinancialStatusToneClass(UserResponseDto user)
    {
        if (user.IsGamificationParticipant != true || !user.ActiveMembershipPlanId.HasValue)
            return "text-slate-700";

        return user.IsMembershipInGoodStanding == true ? "text-emerald-700" : "text-rose-700";
    }

    private static string ToGamificationEventTypeLabel(int? eventType)
    {
        return eventType switch
        {
            1 => "Check-in",
            2 => "Publicação social",
            3 => "Aula especial",
            4 => "Evento externo",
            5 => "Compra na loja",
            6 => "Pagamento em dia",
            7 => "Renovação de plano",
            _ => "Atividade"
        };
    }
}
