using Godot;
using SciColorMaps;

public class MapModeSystem : ISystem {
	public RelEcs.World World { get; set; }


	private ColorMap terrainColorMap = new ColorMap("inferno");

	public void Run() {
		var tiles = this.Query<Location, TileData>();

		foreach (var (location, tileData) in tiles) {
			float v = tileData.height / 255f;
			var color = terrainColorMap[v].ToGodot();
			MapModes.terrain.SetColor(location.hex, color);
		}
	}
}