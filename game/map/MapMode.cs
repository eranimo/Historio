using System.Reactive.Subjects;
using Godot;

public enum MapModeType {
	Political,
	Terrain,
}

public class MapMode {
	private readonly MapModeType type;
	private Dictionary<Hex, Color> hexColors = new Dictionary<Hex, Color>();

	private readonly Color EMPTY_HEX = new Color("#333333");

	public MapMode(MapModeType type) {
		this.type = type;
	}

	private Dictionary<MapModeType, string> mapModeNames = new Dictionary<MapModeType, string>() {
		[MapModeType.Political] = "Political",
		[MapModeType.Terrain] = "Terrain",
	};

	public string Name => mapModeNames[type];

	public bool HasColor(Hex hex) {
		return hexColors.ContainsKey(hex);
	}

	public Color GetColor(Hex hex) {
		return hexColors[hex];
		// if (hexColors.ContainsKey(hex)) {
		// 	return hexColors[hex];
		// }
		// return EMPTY_HEX;
	}

	public void SetColor(Hex hex, Color color) {
		hexColors[hex] = color;
	}
}

public static class MapModes {
	public static MapMode political = new MapMode(MapModeType.Political);
	public static MapMode terrain = new MapMode(MapModeType.Terrain);

	public static List<MapMode> List = new List<MapMode> {
		political,
		terrain,
	};

	public static BehaviorSubject<MapMode> CurrentMapMode = new BehaviorSubject<MapMode>(MapModes.political);
}