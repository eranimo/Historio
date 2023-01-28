using System;
using RelEcs;
using System.Reactive.Subjects;
using System.Collections.Generic;
using Godot;
using MessagePack;

public static class Tile {
	// controls terrain texture
	public enum BiomeType {
		Ocean,
		Coast,
		Temperate,
		Desert,
		Arctic,
		Freshwater,
		Tropical,
	}

	// Controls terrain shape
	public enum TerrainType {
		Plains, // mostly flat
		Hills, // a few small round hills
		Mountains, // one single mountain peak, foothills around it
	}

	public enum FeatureType {
		Grassland,
		Forest,
		Shrubland,
		Lake,
		River,
	}
}

public enum TileSideFeature {
	Stream,
	StreamFord,
}

[MessagePackObject]
public partial class TileData {
	[Key(0)] public float height;
	[Key(1)] public float temperature;
	[Key(2)] public float rainfall;

	[Key(3)] public Tile.BiomeType biome;
	[Key(4)] public Tile.TerrainType terrain;
	[Key(5)] public Tile.FeatureType feature;

	[Key(6)] public Dictionary<HexDirection, bool> streams = new Dictionary<HexDirection, bool>();

	[Key(8)] public float soilFertility = 1000f;
	[Key(9)] public float plantSpace = 5000f;
	[Key(10)] public float plantSpaceUsed = 0f;
	[Key(11)] public float animalSpace = 1000f;
	[Key(12)] public float animalSpaceUsed = 0f;

	[Key(13)] public float waterHeight;
	[Key(14)] public HexDirection flowDir;
	[Key(15)] public float riverFlow;

	[Key(16)] public Dictionary<HexDirection, float> streamFlow = new Dictionary<HexDirection, float>();


	public int? GetTerrainTilesetIndex() {
		switch (biome) {
			case Tile.BiomeType.Ocean: return 0;
			case Tile.BiomeType.Coast: return 1;
			case Tile.BiomeType.Temperate: return 2;
			case Tile.BiomeType.Desert: return 3;
			case Tile.BiomeType.Arctic: return 4;
			case Tile.BiomeType.Freshwater: return 5;
			default: return null;
		}
	}

	public int? GetFeatureTilesetIndex() {
		if (feature == Tile.FeatureType.Forest) {
			return 0;
		}
		return null;
	}

	[IgnoreMember]
	public bool IsLand {
		get {
			return biome != Tile.BiomeType.Ocean && biome != Tile.BiomeType.Coast;
		}
	}

	[IgnoreMember]
	public float movementCost {
		get {
			return 5f;
		}
	}

	public Color GetMinimapColor() {
		if (biome == Tile.BiomeType.Ocean) {
			return new Color("#006eaa");
		} else if (biome == Tile.BiomeType.Coast) {
			return new Color("#0082cc");
		} else if (biome == Tile.BiomeType.Temperate) {
			return new Color("#378c31");
		} else if (biome == Tile.BiomeType.Desert) {
			return new Color("#d1c075");
		} else if (biome == Tile.BiomeType.Arctic) {
			return new Color("#dce0e3");
		} else if (biome == Tile.BiomeType.Freshwater) {
			return new Color("#0091eb");
		}
		return new Color("#000000");
	}
}

// Added to entities with Hex components to propagate areas where that country is "observing" a tile
[MessagePackObject]
public partial class ViewStateNode {
	[Key(0)]
	public int range;
}

// relation on entity that has a ViewStateNode to Country entity
[MessagePackObject]
public partial class ViewStateOwner {}


// Trigger when view state nodes for a country updates
public partial class ViewStateNodeUpdated {
	public Entity entity; // entity with ViewStateNode component
}

// trigger when Country view state is updated
public partial class ViewStateUpdated {
	public Entity country;
}

public enum ViewState {
	Unexplored,
	Unobserved,
	Observed,
}

public static class ViewStateMethods {
	public static int GetTileMapTile(this ViewState tileViewState) {
		if (tileViewState == ViewState.Unexplored) {
			return 0;
		} else if (tileViewState == ViewState.Unobserved) {
			return 1;
		}
		return -1;
	}
}