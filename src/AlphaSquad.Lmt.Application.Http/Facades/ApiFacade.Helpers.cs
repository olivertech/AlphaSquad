using System;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade
{
    private static Guid ParseRequiredGuid(string value, string parameterName)
    {
        if (Guid.TryParse(value, out var parsed))
            return parsed;

        throw new ArgumentException($"O valor informado para '{parameterName}' precisa ser um GUID valido.", parameterName);
    }
}
