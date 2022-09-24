using Godot;
using RelEcs;
using System;
using System.Linq;
using System.Collections.Generic;


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


	public void ClearPath() {
		Clear();
	}

	public void RenderPath(Entity unit) {
		Clear();
		if (unit is null) {
			GD.PrintS("(SelectedUnitPath) clear path for unit", unit);
			return;
		}
		GD.PrintS("(SelectedUnitPath) render path for unit", unit);
		var pathfinder = gameView.game.state.GetElement<Pathfinder>();
		var world = gameView.game.state.GetElement<World>();
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
				if (path is not null && path.Count() > 1) {
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
