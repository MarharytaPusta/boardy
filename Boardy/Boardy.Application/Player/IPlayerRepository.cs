using System.Threading.Tasks;

namespace Boardy.Application.Player
{
    public interface IPlayerRepository
    {
        Task AddAsync(Domain.Entities.Player player);
    }
}