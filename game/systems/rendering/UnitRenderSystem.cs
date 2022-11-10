using Godot;
using RelEcs;

public partial class UnitRenderSystem : ISystem {
	public RelEcs.World World { get; set; }

	private PackedScene unitIconScene;

	public UnitRenderSystem() {
		unitIconScene = ResourceLoader.Load<PackedScene>("res://view/UnitIcon/UnitIcon.tscn");
	}

	private Dictionary<Entity, UnitIcon> unitIcons = new Dictionary<Entity, UnitIcon>();

	public void Run() {
		var gameMap = World.GetElement<GameMap>();

		foreach (var e in World.Receive<UnitAdded>(this)) {
			GD.PrintS("(UnitRenderSystem) Render unit", e.unit);
			var location = World.GetComponent<Location>(e.unit);
			var unitData = World.GetComponent<UnitData>(e.unit);
			var unitIcon = unitIconScene.Instantiate<UnitIcon>();
			unitIcon.entity = e.unit;
			unitIcon.UnitType = unitData.type;
			unitIcon.Position = gameMap.layout.HexToPixel(location.hex).ToVector();
			unitIcons.Add(e.unit, unitIcon);
			World.On(e.unit).Add(unitIcon);
			gameMap.spriteContainer.AddChild(unitIcon);
		}

		foreach (var e in World.Receive<UnitRemoved>(this)) {
			GD.PrintS("(UnitRenderSystem) RemoveAt unit", e.unit);
			var unitIcon = unitIcons[e.unit];
			unitIcon.QueueFree();
		}
	}
}