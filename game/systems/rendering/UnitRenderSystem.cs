using Godot;
using RelEcs;

public class UnitRenderSystem : ISystem {
	public RelEcs.World World { get; set; }

	private PackedScene unitIconScene;

	public UnitRenderSystem() {
		unitIconScene = ResourceLoader.Load<PackedScene>("res://view/UnitIcon/UnitIcon.tscn");
	}

	public void Run() {
		var gameMap = this.GetElement<GameMap>();

		foreach (var e in this.Receive<UnitAdded>()) {
			GD.PrintS("(UnitRenderSystem) Render unit", e.unit);
			var location = this.GetComponent<Location>(e.unit);
			var unitData = this.GetComponent<UnitData>(e.unit);
			var unitIcon = unitIconScene.Instance<UnitIcon>();
			unitIcon.entity = e.unit;
			unitIcon.UnitType = unitData.type;
			this.On(e.unit).Add(unitIcon);
			unitIcon.Position = gameMap.layout.HexToPixel(location.hex).ToVector();
			gameMap.spriteContainer.AddChild(unitIcon);
		}

		foreach (var e in this.Receive<UnitRemoved>()) {
			GD.PrintS("(UnitRenderSystem) Remove unit", e.unit);
			var unitIcon = this.GetComponent<UnitIcon>(e.unit);
			gameMap.spriteContainer.RemoveChild(unitIcon);
		}
	}
}