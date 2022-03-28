using Godot;

public class PolityGenerator : IGeneratorStep {
	public void Generate(GameOptions options, GameManager manager) {
		var namegen = new NameGenerator("greek");
		for (int i = 0; i < 10; i++) {
			var name = namegen.GetName();
			var polity = new Polity(name);
			manager.AddEntity(polity);
		}
	}
}