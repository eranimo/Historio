using System.Collections.Generic;

public class Polity : Entity {
	public string name;

	public HashSet<Settlement> settlements = new HashSet<Settlement>();

	public Polity(string name) {
		this.name = name;
	}
}
