using System.Collections.Generic;

public class Settlement : Entity {
	public string name;

	public HashSet<Tile> territory = new HashSet<Tile>();

	public Settlement(string name) {
		this.name = name;
	}
}
