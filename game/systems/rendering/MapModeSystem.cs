using System;
using Godot;
using SciColorMaps;

public partial class MapModeSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		foreach (var e in World.Receive<GameStart>(this)) {
			calculate();
		}
	}

	private void calculate() {
		var tiles = World.Query<Location, TileData>().Build();
		foreach (var (location, tileData) in tiles) {
			MapModes.terrain.Overlay.SetValue(location.hex, tileData.height / 255f);
			MapModes.temperature.Overlay.SetValue(location.hex, tileData.temperature / 255f);
			MapModes.rainfall.Overlay.SetValue(location.hex, tileData.rainfall / 255f);
			MapModes.rivers.Overlay.SetValue(
				location.hex,
				Math.Clamp(tileData.riverFlow / 5_000f, 0, 1)
			);
		}
	}
}