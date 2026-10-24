namespace Boardy.Application.GameCatalog.Catalog;

public interface IGameCatalogRepository
{
    Task<IEnumerable<Domain.Entities.Game>> GetAllAsync();
}
