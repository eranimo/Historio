using Godot;
using RelEcs;

public class UnitRenderSystem : ISystem {
	public RelEcs.World World { get; set; }

	private PackedScene unitIconScene;

	public UnitRenderSystem() {
		unitIconScene = ResourceLoader.Load<PackedScene>("res://view/UnitIcon/UnitIcon.tscn");
	}

	private Dictionary<Entity, UnitIcon> unitIcons = new Dictionary<Entity, UnitIcon>();

	public void Run() {
		var gameMap = this.GetElement<GameMap>();

		foreach (var e in this.Receive<UnitAdded>()) {
			GD.PrintS("(UnitRenderSystem) Render unit", e.unit);
			var location = this.GetComponent<Location>(e.unit);
			var unitData = this.GetComponent<UnitData>(e.unit);
			var unitIcon = unitIconScene.Instance<UnitIcon>();
			unitIcon.entity = e.unit;
			unitIcon.UnitType = unitData.type;
			unitIcon.Position = gameMap.layout.HexToPixel(location.hex).ToVector();
			unitIcons.Add(e.unit, unitIcon);
			this.On(e.unit).Add(unitIcon);
			gameMap.spriteContainer.AddChild(unitIcon);
		}

		foreach (var e in this.Receive<UnitRemoved>()) {
			GD.PrintS("(UnitRenderSystem) Remove unit", e.unit);
			var unitIcon = unitIcons[e.unit];
			unitIcon.QueueFree();
		}
	}
}