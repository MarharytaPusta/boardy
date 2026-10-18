using System;

namespace Boardy.Application.Player
{
    public record UpdatePlayerRequest(Guid PlayerId, string Name);
}
