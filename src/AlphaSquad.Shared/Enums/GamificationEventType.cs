namespace AlphaSquad.Shared.Enums;

/// <summary>
/// Representa os tipos de evento que podem gerar pontuacao para alunos.
/// A tabela de regras permite configurar quantos pontos cada evento vale.
/// </summary>
public enum GamificationEventType
{
    CheckIn = 1,
    SocialPost = 2,
    ClassSpecialParticipation = 3,
    OutdoorEventParticipation = 4,
    StorePurchase = 5,
    MembershipPaymentOnTime = 6,
    PlanRenewal = 7
}
