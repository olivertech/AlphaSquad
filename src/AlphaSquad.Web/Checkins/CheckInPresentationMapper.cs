using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Checkins;

/// <summary>
/// Converte dados de check-in e reengajamento em textos prontos para o dashboard.
/// </summary>
public static class CheckInPresentationMapper
{
    public static CheckInListItemViewModel ToListItem(CheckInResponseDto item)
    {
        var notes = string.IsNullOrWhiteSpace(item.Notes) ? "Sem observações." : item.Notes.Trim();

        return new CheckInListItemViewModel
        {
            Id = item.Id ?? Guid.Empty,
            UserId = item.UserId ?? Guid.Empty,
            UserName = string.IsNullOrWhiteSpace(item.UserName) ? "Usuário sem nome" : item.UserName,
            UserRoleLabel = item.UserRole switch
            {
                1 => "Administrador",
                2 => "Professor",
                3 => "Aluno",
                _ => "Perfil não identificado"
            },
            NotesPreview = notes.Length > 80 ? $"{notes[..80]}..." : notes,
            CheckedInAt = item.CheckedInAt
        };
    }

    public static InactiveCheckInUserViewModel ToInactiveUser(UserWithoutRecentCheckInResponseDto user)
    {
        return new InactiveCheckInUserViewModel
        {
            UserId = user.UserId ?? Guid.Empty,
            UserName = string.IsNullOrWhiteSpace(user.UserName) ? "Aluno sem nome" : user.UserName,
            Email = user.Email ?? "E-mail não informado",
            DaysWithoutCheckInLabel = BuildDaysLabel(user.DaysWithoutCheckIn),
            ActivePlanName = user.ActivePlanName,
            LastCheckedInAt = user.LastCheckedInAt
        };
    }

    private static string BuildDaysLabel(int? days)
    {
        if (!days.HasValue)
            return "Sem referência de dias";

        return days.Value == 1
            ? "1 dia sem check-in"
            : $"{days.Value} dias sem check-in";
    }
}
