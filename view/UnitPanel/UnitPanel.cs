using Godot;
using RelEcs;
using System;
using System.Collections.Generic;

public class UnitPanelUISystem : ISystem {
	public void Run(Commands commands) {
		var unitPanel = commands.GetElement<UnitPanel>();

		if (unitPanel is null) {
			return;
		}

		commands.Receive((SelectedUnitUpdate e) => {
			if (e.unit is null) {
				unitPanel.Hide();
			} else {
				unitPanel.Show();
				unitPanel.update(e.unit);
			}
		});

		var selectedUnit = commands.GetElement<SelectedUnit>().unit;
		commands.Receive((UnitMoved e) => {
			if (e.unit == selectedUnit) {
				unitPanel.update(e.unit);
			}
		});
	}
}

public class UnitPanel : PanelContainer {
	private GameView gameView;
	private Label unitNameLabel;
	private Label unitPositionLabel;
	private Control movementRow;
	private Label movementTarget;
	private RelEcs.World state;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		gameView.game.manager.state.AddElement(this);
		state = gameView.game.manager.state;

		var closeButton = GetNode<ToolButton>("VBoxContainer/HBoxContainer/CloseButton");
		unitNameLabel = GetNode<Label>("VBoxContainer/HBoxContainer/UnitNameLabel");
		unitPositionLabel = GetNode<Label>("VBoxContainer/Details/DetailRow/UnitPosition");
		movementRow = GetNode<Control>("VBoxContainer/Details/MovementRow");
		movementTarget = GetNode<Label>("VBoxContainer/Details/MovementRow/MoveTarget");

		closeButton.Connect("pressed", this, nameof(closeButtonPressed));
		Hide();
	}

	public Entity currentUnit => gameView.game.manager.state.GetElement<SelectedUnit>().unit;

	private void closeButtonPressed() {
		state.Send(new SelectedUnitUpdate { unit = null });
	}

	public void update(Entity unit) {
		var unitData = unit.Get<UnitData>();
		var location = unit.Get<Location>();
		unitNameLabel.Text = Unit.unitNames[unitData.type];
		unitPositionLabel.Text = $"({location.hex.col}, {location.hex.row})";

		var movement = unit.Get<Movement>();
		if (movement.currentTarget is not null) {
			movementRow.Show();
			movementTarget.Text = $"({movement.currentTarget.col}, {movement.currentTarget.row})";
		} else {
			movementRow.Hide();
		}
	}
}
