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
		commands.Receive((CurrentActionChanged e) => {
			if (e.entity == selectedUnit) {
				unitPanel.update(e.entity);
			}
		});

		commands.Receive((ActionQueueChanged e) => {
			if (e.entity == selectedUnit) {
				unitPanel.update(e.entity);
			}
		});
	}
}

public class UnitPanel : PanelContainer {
	private GameView gameView;
	private Label unitNameLabel;
	private Label unitPositionLabel;
	private Label currentActionLabel;
	private Control queueItemList;
	private Control movementRow;
	private Label movementTarget;
	private RelEcs.World state;
	private PackedScene queueItemScene;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		gameView.game.manager.state.AddElement(this);
		state = gameView.game.manager.state;

		var closeButton = GetNode<ToolButton>("UnitInfo/Header/CloseButton");
		unitNameLabel = GetNode<Label>("UnitInfo/Header/UnitNameLabel");
		unitPositionLabel = GetNode<Label>("UnitInfo/Content/Details/DetailRow/UnitPosition");
		currentActionLabel = GetNode<Label>("UnitInfo/Content/Actions/CurrentActionRow/CurrentActionLabel");

		queueItemList = GetNode<Control>("UnitInfo/Content/Actions/ActionQueue/MarginContainer/QueueItemList");
		queueItemScene = ResourceLoader.Load<PackedScene>("res://view/UnitPanel/QueueItem.tscn");

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
		
		var actionQueue = unit.Get<ActionQueue>();
		if (actionQueue.currentAction is null) {
			currentActionLabel.Text = "Idle";
		} else {
			currentActionLabel.Text = actionQueue.currentAction.GetLabel();
		}

		foreach (var item in queueItemList.GetChildren()) {
			queueItemList.RemoveChild((Node) item);
		}

		int num = 1;
		foreach (var action in actionQueue.actions) {
			var queueItem = queueItemScene.Instance<QueueItem>();
			queueItemList.AddChild(queueItem);
			queueItem.handleRemove = () => {
				actionQueue.removeAction(action);
				state.Send(new ActionQueueChanged { entity = unit });
			};
			queueItem.QueueNumber = num.ToString();
			queueItem.ActionLabel = action.GetLabel();
			num++;
		}
	}
}
