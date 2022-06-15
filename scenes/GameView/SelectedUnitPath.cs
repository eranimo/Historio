using Godot;
using RelEcs;
using System;
using System.Collections.Generic;

public class UnitPathSystem : ISystem {
	public void Run(Commands commands) {
		var selectedUnitPath = commands.GetElement<SelectedUnitPath>();
		var selectedUnit = commands.GetElement<SelectedUnit>();
		commands.Receive((SelectedUnitUpdate e) => {
			selectedUnitPath.render(e.unit);
		});
		commands.Receive((UnitMoved e) => {
			if (e.unit == selectedUnit.unit) {
				selectedUnitPath.render(e.unit);
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

	public void render(Entity unit) {
		GD.PrintS("(SelectedUnitPath) render path for unit", unit);
		Clear();
		if (unit is null) {
			return;
		}
		var movement = unit.Get<Movement>();
		if (movement.currentTarget is null) {
			return;
		}
		SetCellv(movement.currentTarget.ToVector(), (int) TileMapIndex.Target);
		GD.PrintS(movement.currentTarget.ToVector());

		for(int i = movement.currentPathIndex; i < movement.path.Count - 1; i++) {
			SetCellv(movement.path[i].ToVector(), (int) TileMapIndex.HexInterval);
		}
	}
}
