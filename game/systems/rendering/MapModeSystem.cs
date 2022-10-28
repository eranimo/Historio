using Godot;
using SciColorMaps;

public class MapModeSystem : ISystem {
	public RelEcs.World World { get; set; }


	private ColorMap terrainColorMap = new ColorMap("inferno");

	public void Run() {
		var tiles = this.Query<Location, TileData>();

		foreach (var (location, tileData) in tiles) {
			float v = tileData.height / 255f;
			// MapModes.terrain.SetColor(location.hex, Color.FromHsv(0f, 1f, v));
			var color = terrainColorMap[v].ToGodot();
			GD.PrintS(color);
			MapModes.terrain.SetColor(location.hex, color);
		}
	}
}