public class Tile {
    public readonly OffsetCoord coord;

	public float height;
	public float temperature;
	public float rainfall;

	public TerrainType terrainType;
	public FeatureType featureType;

    public Tile(OffsetCoord coord) {
        this.coord = coord;
    }
}