namespace AlphaSquad.Shared.Enums;

/// <summary>
/// Representa o estado operacional do pedido na loja interna da academia.
/// A V1 usa um fluxo presencial de separacao, retirada e pagamento local.
/// </summary>
public enum StoreOrderStatus
{
    PendingApproval = 1,
    Reserved = 2,
    ReadyForPickup = 3,
    PaidLocally = 4,
    Cancelled = 5
}
