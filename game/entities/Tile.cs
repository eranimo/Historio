using System;
using System.Reactive.Subjects;
using System.Collections.Generic;

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

public class Tile : Entity {
	public readonly OffsetCoord coord;

	public float height;
	public float temperature;
	public float rainfall;

	public BiomeType biome;
	public TerrainType terrain;
	public FeatureType feature;
	public HashSet<Tile> roadConnections = new HashSet<Tile>();
	public HashSet<Tile> riverConnections = new HashSet<Tile>();

	public Tile(OffsetCoord coord) {
		this.coord = coord;
	}

	public override string ToString() {
		return base.ToString() + string.Format("({0}, {1})", this.coord.col, this.coord.row);
	}

	public int? GetTerrainTilesetIndex() {
		switch (biome) {
			case BiomeType.Ocean: return 0;
			case BiomeType.Coast: return 1;
			case BiomeType.Temperate: return 2;
			case BiomeType.Desert: return 3;
			case BiomeType.Arctic: return 4;
			default: return null;
		}
	}

	public int? GetFeatureTilesetIndex() {
		if (feature == FeatureType.Forest) {
			return 0;
		}
		return null;
	}
}
