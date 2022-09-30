using System;
using RelEcs;
using System.Reactive.Subjects;
using System.Collections.Generic;
using Godot;

public static class Tile {
	public enum BiomeType {
		Ocean,
		Coast,
		Temperate,
		Desert,
		Arctic,
	}

	public enum TerrainType {
		Plains,
		Hills,
		Mountains,
	}

	public enum FeatureType {
		Grassland,
		Forest,
		Jungle,
	}
}

public class TileData {
	public float height;
	public float temperature;
	public float rainfall;

	public Tile.BiomeType biome;
	public Tile.TerrainType terrain;
	public Tile.FeatureType feature;

	public Dictionary<HexDirection, bool> riverSegments = new Dictionary<HexDirection, bool>();

	public float soilFertility = 1000f;
	public float plantSpace = 5000f;
	public float plantSpaceUsed = 0f;
	public float animalSpace = 1000f;
	public float animalSpaceUsed = 0f;

	public int? GetTerrainTilesetIndex() {
		switch (biome) {
			case Tile.BiomeType.Ocean: return 0;
			case Tile.BiomeType.Coast: return 1;
			case Tile.BiomeType.Temperate: return 2;
			case Tile.BiomeType.Desert: return 3;
			case Tile.BiomeType.Arctic: return 4;
			default: return null;
		}
	}

	public int? GetFeatureTilesetIndex() {
		if (feature == Tile.FeatureType.Forest) {
			return 0;
		}
		return null;
	}

	public bool IsLand {
		get {
			return biome != Tile.BiomeType.Ocean && biome != Tile.BiomeType.Coast;
		}
	}

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
		}
		return new Color("#000000");
	}
}

public class TileViewState {
	public Dictionary<Entity, ViewState> countriesToViewStates = new Dictionary<Entity, ViewState>();
}

// Added to entities with Hex components to propagate areas where that country is "observing" a tile
public class ViewStateNode {
	public Entity country;
	public int range;
}

// Trigger when view state nodes for a country updates
public class ViewStateNodeUpdated {
	public Entity entity; // entity with ViewStateNode component
}

// trigger when Country view state is updated
public class ViewStateUpdated {
	public Entity country;
}