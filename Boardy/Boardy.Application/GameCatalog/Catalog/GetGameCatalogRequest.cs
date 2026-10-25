namespace Boardy.Application.GameCatalog.Catalog;

public record GetGameCatalogRequest(string? SearchQuery, int? PlayersCount);
