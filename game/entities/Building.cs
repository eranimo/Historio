public class Building : Entity {
  public BuildingType type;
  public Tile tile;

	public Building(BuildingType type, Tile tile) {
    this.type = type;
    this.tile = tile;
	}

	public enum BuildingType {
		Village,
		Fort,
		Farm,
		Woodcutter,
		Mine,
	}  
}
