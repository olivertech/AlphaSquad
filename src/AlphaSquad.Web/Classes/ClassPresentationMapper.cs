using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Users;

namespace AlphaSquad.Web.Classes;

/// <summary>
/// Concentra as regras de apresentação do módulo de aulas.
/// Assim, as Razor Pages ficam mais limpas e as decisões de texto/indicador permanecem em um único lugar.
/// </summary>
public static class ClassPresentationMapper
{
    public static ClassDetailsViewModel ToDetails(
        GymClassResponseDto gymClass,
        int bookingCount)
    {
        var listItem = ToListItem(gymClass, bookingCount);

        return new ClassDetailsViewModel
        {
            Id = listItem.Id,
            Name = listItem.Name,
            Description = listItem.Description,
            InstructorName = listItem.InstructorName,
            InstructorUserId = gymClass.InstructorUserId,
            ScheduleLabel = listItem.ScheduleLabel,
            StartsAt = listItem.StartsAt,
            EndsAt = listItem.EndsAt,
            Location = listItem.Location,
            Capacity = listItem.Capacity,
            BookingCount = listItem.BookingCount,
            OccupancyRate = listItem.OccupancyRate,
            OccupancyLabel = listItem.OccupancyLabel,
            IsActive = listItem.IsActive,
            StatusLabel = listItem.StatusLabel,
            IsSpecialClass = listItem.IsSpecialClass,
            SpecialClassLabel = listItem.SpecialClassLabel,
            CreatedAt = gymClass.CreatedAt
        };
    }

    public static ClassFormInputModel ToFormInput(GymClassResponseDto gymClass)
    {
        return new ClassFormInputModel
        {
            Name = gymClass.Name?.Trim() ?? string.Empty,
            Description = gymClass.Description?.Trim(),
            Location = gymClass.Location?.Trim(),
            Capacity = gymClass.Capacity,
            StartsAt = gymClass.StartsAt?.ToLocalTime().DateTime,
            EndsAt = gymClass.EndsAt?.ToLocalTime().DateTime,
            InstructorUserId = gymClass.InstructorUserId,
            IsSpecialClass = gymClass.IsSpecialClass == true,
            IsActive = gymClass.IsActive == true
        };
    }

    public static ClassListItemViewModel ToListItem(
        GymClassResponseDto gymClass,
        int bookingCount)
    {
        var capacity = Math.Max(gymClass.Capacity ?? 0, 0);
        var occupancyRate = capacity > 0
            ? Math.Round((decimal)bookingCount / capacity * 100m, 1)
            : 0m;

        return new ClassListItemViewModel
        {
            Id = gymClass.Id ?? Guid.Empty,
            Name = string.IsNullOrWhiteSpace(gymClass.Name) ? "Aula sem nome" : gymClass.Name,
            Description = string.IsNullOrWhiteSpace(gymClass.Description) ? "Sem descrição informada." : gymClass.Description.Trim(),
            InstructorName = string.IsNullOrWhiteSpace(gymClass.InstructorName) ? "Instrutor não definido" : gymClass.InstructorName,
            StartsAt = gymClass.StartsAt,
            EndsAt = gymClass.EndsAt,
            ScheduleLabel = BuildScheduleLabel(gymClass.StartsAt, gymClass.EndsAt),
            Location = string.IsNullOrWhiteSpace(gymClass.Location) ? "Local não informado" : gymClass.Location.Trim(),
            Capacity = capacity,
            BookingCount = bookingCount,
            OccupancyRate = occupancyRate,
            OccupancyLabel = $"{bookingCount}/{capacity} vagas ({occupancyRate:0.#}%)",
            IsActive = gymClass.IsActive == true,
            StatusLabel = gymClass.IsActive == true ? "Ativa" : "Inativa",
            IsSpecialClass = gymClass.IsSpecialClass == true,
            SpecialClassLabel = gymClass.IsSpecialClass == true ? "Aulão" : "Turma regular"
        };
    }

    public static ClassBookingListItemViewModel ToBookingItem(ClassBookingManagementResponseDto booking)
    {
        return new ClassBookingListItemViewModel
        {
            BookingId = booking.BookingId ?? Guid.Empty,
            GymClassId = booking.GymClassId ?? Guid.Empty,
            UserId = booking.UserId ?? Guid.Empty,
            ClassName = string.IsNullOrWhiteSpace(booking.ClassName) ? "Aula sem nome" : booking.ClassName,
            UserName = string.IsNullOrWhiteSpace(booking.UserName) ? "Aluno sem nome" : booking.UserName,
            UserEmail = string.IsNullOrWhiteSpace(booking.UserEmail) ? "E-mail não informado" : booking.UserEmail,
            UserRoleLabel = UserPresentationMapper.ToRoleLabel(booking.UserRole),
            ScheduleLabel = BuildScheduleLabel(booking.StartsAt, booking.EndsAt),
            BookedAt = booking.BookedAt,
            IsSpecialClass = booking.IsSpecialClass == true
        };
    }

    private static string BuildScheduleLabel(DateTimeOffset? startsAt, DateTimeOffset? endsAt)
    {
        if (!startsAt.HasValue)
            return "Horário não informado";

        var localStart = startsAt.Value.ToLocalTime();
        var baseLabel = localStart.ToString("ddd, dd/MM 'às' HH:mm");

        if (!endsAt.HasValue)
            return baseLabel;

        return $"{baseLabel} - {endsAt.Value.ToLocalTime():HH:mm}";
    }
}
