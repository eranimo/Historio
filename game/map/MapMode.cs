using System;
using System.Reactive.Subjects;
using Godot;
using SciColorMaps;

public class MapMode {
	private readonly string name;
	private readonly MapModeOverlay overlay;
	private readonly bool showRivers;

	public MapMode(
		string name,
		MapModeOverlay overlay = null,
		bool showRivers = false
	) {
		this.name = name;
		this.overlay = overlay;
		this.showRivers = showRivers;
	}

	public string Name => name;
	public MapModeOverlay Overlay => overlay;
	public bool ShowRivers => showRivers;
}

public class MapModeOverlay {
	private readonly ColorMap colorMap;
	private Dictionary<Hex, Color> hexColors = new Dictionary<Hex, Color>();

	public MapModeOverlay(ColorMap colorMap) {
		this.colorMap = colorMap;
	}

	public bool HasColor(Hex hex) {
		return hexColors.ContainsKey(hex);
	}

	public Color GetColor(Hex hex) {
		return hexColors[hex];
	}

	public void SetValue(Hex hex, float value) {
		hexColors[hex] = colorMap[value].ToGodot();
	}
}

public static class MapModes {
	public static MapMode political = new MapMode(
		name: "Political",
		showRivers: true
	);
	public static MapMode terrain = new MapMode(
		name: "Terrain",
		overlay: new MapModeOverlay(new ColorMap("terrain"))
	);
	public static MapMode temperature = new MapMode(
		name: "Temperature",
		overlay: new MapModeOverlay(new ColorMap("reds"))
	);
	public static MapMode rainfall = new MapMode(
		name: "Rainfall",
		overlay: new MapModeOverlay(new ColorMap("ocean")),
		showRivers: true
	);
	public static MapMode rivers = new MapMode(
		name: "Rivers",
		overlay: new MapModeOverlay(new ColorMap("ocean")),
		showRivers: true
	);

	public static List<MapMode> List = new List<MapMode> {
		political,
		terrain,
		temperature,
		rainfall,
		rivers,
	};

	public static BehaviorSubject<MapMode> CurrentMapMode = new BehaviorSubject<MapMode>(MapModes.political);
}