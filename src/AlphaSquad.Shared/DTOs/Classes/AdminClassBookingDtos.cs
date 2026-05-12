namespace AlphaSquad.Shared.DTOs.Classes;

/// <summary>
/// DTO usado pela gestão da academia para reservar uma aula em nome de um aluno.
/// Esse fluxo ajuda a recepção e o atendimento presencial a confirmarem vagas sem depender do app do aluno.
/// </summary>
public record CreateClassBookingForUserRequest(Guid UserId);
