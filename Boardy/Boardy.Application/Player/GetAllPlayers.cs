using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Boardy.Application.Player
{
    public class GetAllPlayers
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<GetAllPlayers> _logger;

        public GetAllPlayers(IPlayerRepository playerRepository, ILogger<GetAllPlayers> logger)
        {
            _playerRepository = playerRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<Domain.Entities.Player>>> ExecuteAsync()
        {
            var players = await _playerRepository.GetAllAsync();

            _logger.LogInformation("Loaded players list");
            return Result<IEnumerable<Domain.Entities.Player>>.Success(players);
        }
    }
}
