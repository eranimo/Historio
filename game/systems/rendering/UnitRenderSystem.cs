using Godot;
using RelEcs;

public class UnitRenderSystem : ISystem {
	private PackedScene unitIconScene;

	public UnitRenderSystem() {
		unitIconScene = ResourceLoader.Load<PackedScene>("res://view/UnitIcon/UnitIcon.tscn");
	}

	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();

		commands.Receive((UnitAdded e) => {
			GD.PrintS("(UnitRenderSystem) Render unit", e.unit);
			var location = e.unit.Get<Location>();
			var unitData = e.unit.Get<UnitData>();
			var unitIcon = unitIconScene.Instance<UnitIcon>();
			unitIcon.entity = e.unit;
			unitIcon.UnitType = unitData.type;
			e.unit.Add(unitIcon);
			unitIcon.Position = gameMap.layout.HexToPixel(location.hex).ToVector();
			gameMap.spriteContainer.AddChild(unitIcon);
		});

		commands.Receive((UnitRemoved e) => {
			GD.PrintS("(UnitRenderSystem) Remove unit", e.unit);
			var unitIcon = e.unit.Get<UnitIcon>();
			gameMap.spriteContainer.RemoveChild(unitIcon);
		});
	}
}