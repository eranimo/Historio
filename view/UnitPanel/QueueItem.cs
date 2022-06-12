using Godot;
using System.Reactive.Subjects;

public delegate void HandleRemove();

public class QueueItem : PanelContainer {
	private Label queueNumber;
	private Label actionLabel;
	private ToolButton removeActionButton;
	public HandleRemove handleRemove;

	public override void _Ready() {
		queueNumber = GetNode<Label>("Columns/QueueNumber");
		actionLabel = GetNode<Label>("Columns/ActionLabel");
		removeActionButton = GetNode<ToolButton>("Columns/RemoveActionButton");

		removeActionButton.Connect("pressed", this, nameof(onRemove));
		handleRemove = delegate () {};
	}

	private void onRemove() {
		handleRemove();
	}

	public string QueueNumber {
		get => queueNumber.Text;
		set => queueNumber.Text = value;
	}

	public string ActionLabel {
		get => actionLabel.Text;
		set => actionLabel.Text = value;
	}
}
