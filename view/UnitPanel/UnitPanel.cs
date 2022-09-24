using Godot;

public class UnitPanel : GamePanelView {
	private Label unitPositionLabel;
	private Label currentActionLabel;
	private Control queueItemList;
	private Control movementRow;
	private Label movementTarget;
	private PackedScene queueItemScene;
	private Button stopButton;
	private Button moveButton;

	public override void _Ready() {
		base._Ready();
		state.AddElement(this);

		unitPositionLabel = GetNode<Label>("UnitInfo/Content/Details/DetailRow/UnitPosition");
		currentActionLabel = GetNode<Label>("UnitInfo/Content/Actions/CurrentActionRow/CurrentActionLabel");

		queueItemList = GetNode<Control>("UnitInfo/Content/Actions/ActionQueue/MarginContainer/QueueItemList");
		queueItemScene = ResourceLoader.Load<PackedScene>("res://view/UnitPanel/QueueItem.tscn");

		// action buttons
		stopButton = GetNode<Button>("UnitInfo/Footer/StopButton");
		moveButton = GetNode<Button>("UnitInfo/Footer/MoveButton");

		// stopButton.Connect("pressed", this, nameof(stopButtonPressed));

		UpdateView(gamePanel.CurrentPanel.Value.entity);
	}

	// private void stopButtonPressed() => state.Send(new ActionQueueClear { owner = currentUnit });

	public override void UpdateView(Entity unit) {
		var unitData = unit.Get<UnitData>();
		var location = unit.Get<Location>();
		var selectedUnitPath = state.GetElement<SelectedUnitPath>();
		selectedUnitPath.RenderPath(unit);
		unit.Get<UnitIcon>().Selected = true;
		gamePanel.SetTitle($"Unit ({unitData.type.name})");
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

	public override void ResetView(Entity entity) {
		entity.Get<UnitIcon>().Selected = false;
		var selectedUnitPath = state.GetElement<SelectedUnitPath>();
		selectedUnitPath.ClearPath();
	}
}
