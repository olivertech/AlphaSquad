namespace AlphaSquad.Shared.DTOs.Common;

/// <summary>
/// Contrato padrao para feeds cursor-based consumidos pelo app.
/// Esse formato foi pensado para scroll infinito, onde o cliente precisa saber apenas os itens retornados,
/// se ainda existe mais conteudo e qual cursor deve ser usado na proxima busca.
/// </summary>
public record CursorFeedResponse<TItem>(
    IReadOnlyList<TItem> Items,
    string? NextCursor,
    bool HasMore
);
