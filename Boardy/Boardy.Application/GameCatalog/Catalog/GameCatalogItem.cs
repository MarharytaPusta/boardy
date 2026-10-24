namespace Boardy.Application.GameCatalog.Catalog;

public record GameCatalogItem(
    Guid Id,
    string Name,
    string Description,
    int MinPlayers,
    int MaxPlayers);
