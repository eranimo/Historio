using Godot;
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

		commands.Receive((UnitMoved e) => {
			if (e.unit == selectedUnit) {
				unitPanel.update(e.unit);
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
	private Button stopButton;
	private Button moveButton;

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

		// action buttons
		stopButton = GetNode<Button>("UnitInfo/Footer/StopButton");
		moveButton = GetNode<Button>("UnitInfo/Footer/MoveButton");
		stopButton.Connect("pressed", this, nameof(stopButtonPressed));

		closeButton.Connect("pressed", this, nameof(closeButtonPressed));
		Hide();
	}

	public Entity currentUnit => gameView.game.manager.state.GetElement<SelectedUnit>().unit;

	private void closeButtonPressed() => state.Send(new SelectedUnitUpdate { unit = null });
	private void stopButtonPressed() => state.Send(new ActionQueueClear { owner = currentUnit });

	public void update(Entity unit) {
		var unitData = unit.Get<UnitData>();
		var location = unit.Get<Location>();
		unitNameLabel.Text = unitData.type.name;
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
				state.Send(new ActionQueueRemove { owner = unit, action = action });
			};
			queueItem.QueueNumber = num.ToString();
			queueItem.ActionLabel = action.GetLabel();
			num++;
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			state.Send(new SelectedUnitUpdate { unit = null });
			GetTree().SetInputAsHandled();
		}
	}
}
