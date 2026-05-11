using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Web.Models;
using System.Globalization;

namespace AlphaSquad.Web.Plans;

/// <summary>
/// Converte os contratos do modulo de planos em textos amigaveis para o dashboard.
/// </summary>
public static class PlanPresentationMapper
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static PlanListItemViewModel ToListItem(MembershipPlanResponseDto plan)
    {
        var durationDays = plan.DurationDays ?? 0;

        return new PlanListItemViewModel
        {
            Id = plan.Id ?? Guid.Empty,
            Name = string.IsNullOrWhiteSpace(plan.Name) ? "Plano sem nome" : plan.Name,
            Description = string.IsNullOrWhiteSpace(plan.Description) ? "Sem descrição cadastrada." : plan.Description,
            PriceLabel = (plan.Price ?? 0d).ToString("C", PtBr),
            DurationDays = durationDays,
            IsActive = plan.IsActive == true,
            StatusLabel = plan.IsActive == true ? "Ativo" : "Inativo",
            CreatedAt = plan.CreatedAt
        };
    }

    public static InactivePlanUserViewModel ToInactiveUser(UserWithoutActivePlanResponseDto user)
    {
        return new InactivePlanUserViewModel
        {
            UserId = user.UserId ?? Guid.Empty,
            UserName = string.IsNullOrWhiteSpace(user.UserName) ? "Aluno sem nome" : user.UserName,
            Email = user.Email ?? "E-mail não informado",
            DaysWithoutPlanLabel = BuildDaysLabel(user.DaysWithoutActivePlan),
            LastPlanName = user.LastPlanName,
            LastPlanEndedAt = user.LastPlanEndedAt
        };
    }

    private static string BuildDaysLabel(int? days)
    {
        if (!days.HasValue)
            return "Sem referência de dias";

        return days.Value == 1
            ? "1 dia sem plano"
            : $"{days.Value} dias sem plano";
    }
}
