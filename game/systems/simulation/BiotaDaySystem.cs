using System.Linq;

/*

Biota System:

Each tile has:
- plant space used, plant space max
- animal space used, animal space max
- soil fertility amount

On each day:
	For each tile with biota:
		- handle plant growth:
			- calculate remaining plant space at this tile
			- order each plant biota by fitness score, ordered highest first
				- higher score means this tile fits its requirements
			- plants with higher fitness score grow first. Growth stops when capacity is reached
			- growth is proportional to the remaining capacity
		- consumption for animals:
			- process plant consumption for each animal
			- process animal consumption for each animal
		- handle plant spread:
			- increased chance of spreading if not growing (migrate due to competition)
			- new biota in new tile starts with a small population
		- handle animal migration:
			- increased chance of spreading if declining population (migrate to find better conditions)
			- size of migration is proportional to the size of the population
*/


public class BiotaDaySystem : ISystem {
	public void Run(Commands commands) {
		var biotaService = commands.GetElement<BiotaService>();
		commands.Receive((BiotaAdded biotaAdded) => {
			biotaService.AddBiota(biotaAdded);
		});

		var tiles = commands.Query<Entity>().Has<TileData>();

		foreach (var tile in tiles) {
			biotaService.CalculateTile(tile);
			biotaService.DebugTile(tile);
		}
	}
}