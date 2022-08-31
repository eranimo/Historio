using Godot;
using RelEcs;
using System;
using System.Linq;
using System.Collections.Generic;

public class UnitPathSystem : ISystem {
	public void Run(Commands commands) {
		var selectedUnitPath = commands.GetElement<SelectedUnitPath>();
		var selectedUnit = commands.GetElement<SelectedUnit>();
		commands.Receive((SelectedUnitUpdate e) => {
			selectedUnitPath.render(commands, e.unit);
		});
		commands.Receive((UnitMoved e) => {
			if (e.unit == selectedUnit.unit) {
				selectedUnitPath.render(commands, e.unit);
			}
		});

		commands.Receive((UnitMovementPathUpdated e) => {
			if (e.unit == selectedUnit.unit) {
				selectedUnitPath.render(commands, e.unit);
			}
		});
	}
}

public class SelectedUnitPath : TileMap {
	private GameView gameView;

	enum TileMapIndex {
		Target = 2,
		HexInterval = 3,
		DayInterval = 4,
	}

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		gameView.game.state.AddElement(this);
	}

	public void render(Commands commands, Entity unit) {
		GD.PrintS("(SelectedUnitPath) render path for unit", unit);
		Clear();
		if (unit is null) {
			return;
		}
		var pathfinder = commands.GetElement<Pathfinder>();
		var world = commands.GetElement<World>();
		var location = unit.Get<Location>();

		var actionQueue = unit.Get<ActionQueue>();

		// current action path
		Hex currentHex = location.hex;
		if (actionQueue.currentAction is not null) {
			if (actionQueue.currentAction is MovementAction movementAction) {
				var fromTile = world.GetTile(location.hex);
				var toTile = world.GetTile(movementAction.target);
				var path = pathfinder.getPath(fromTile, toTile);
				foreach (var loc in path) {
					SetCellv(loc.ToVector(), (int) TileMapIndex.HexInterval);
				}
				SetCellv(movementAction.target.ToVector(), (int) TileMapIndex.Target);
				currentHex = movementAction.target;
			}
		}

		foreach(var action in actionQueue.actions) {
			if (action is MovementAction movementAction) {
				var fromTile = world.GetTile(currentHex);
				var toTile = world.GetTile(movementAction.target);
				var path = pathfinder.getPath(fromTile, toTile);
				if (path is not null) {
					var pathpart = path.ToArray()[1..^1];
					foreach (var loc in pathpart) {
						SetCellv(loc.ToVector(), (int) TileMapIndex.HexInterval);
					}
					SetCellv(movementAction.target.ToVector(), (int) TileMapIndex.Target);
					currentHex = movementAction.target;
				}
			}
		}
	}
}
