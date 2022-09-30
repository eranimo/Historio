using Godot;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

public enum GamePanelType {
	Unit,
	Tile,
}

public struct GamePanelState {
	public GamePanelType type;
	public Entity entity;
}

public class GamePanel : PanelContainer {
	private GameView gameView;
	private PackedScene unitPanelScene;
	private PackedScene tilePanelScene;
	private Label title;
	private TextureButton backButton;
	private TextureButton forwardButton;
	private TextureButton closeButton;
	private Control content;

	private List<GamePanelState> panelHistory = new List<GamePanelState>();
	private int currentPanelIndex = 0;
	private GamePanelView currentPanelView = null;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");

		gameView.game.state.AddElement(this);

		unitPanelScene = ResourceLoader.Load<PackedScene>("res://view/UnitPanel/UnitPanel.tscn");
		tilePanelScene = ResourceLoader.Load<PackedScene>("res://view/TilePanel/TilePanel.tscn");

		title = (Label) GetNode("VBoxContainer/Header/Title");
		backButton = (TextureButton) GetNode("VBoxContainer/Header/BackButton");
		forwardButton = (TextureButton) GetNode("VBoxContainer/Header/ForwardButton");
		closeButton = (TextureButton) GetNode("VBoxContainer/Header/CloseButton");
		content = (Control) GetNode("VBoxContainer/Content");

		PanelSubject
			.Scan(
				new {
					Previous = (GamePanelState?) null,
					Current = (GamePanelState?) null
				},
				(acc, cur) => new {
					Previous = acc.Current,
					Current = cur
				}
			)
			.Subscribe(result => this.updatePanel(result.Current, result.Previous));

		backButton.Connect("pressed", this, nameof(handleBack));
		forwardButton.Connect("pressed", this, nameof(handleForward));
		closeButton.Connect("pressed", this, nameof(handleClose));
	}

	private void updatePanel(GamePanelState? newPanelState, GamePanelState? oldPanelState) {
		if (oldPanelState.HasValue) {
			GD.PrintS("(GamePanel) remove old panel view", oldPanelState.Value.type, oldPanelState.Value.entity);
			currentPanelView.ResetView(oldPanelState.Value.entity);
			currentPanelView.QueueFree();
		}

		if (!newPanelState.HasValue) {
			GD.PrintS("(GamePanel) hide panel");
			Hide();
			return;
		}
		Show();
		GD.PrintS($"(GamePanel) load panel", newPanelState.Value.type, newPanelState.Value.entity);

		if (CanGoBack) {
			backButton.Modulate = new Color(1, 1, 1, 1f);
		} else {
			backButton.Modulate = new Color(1, 1, 1, 0.5f);
		}

		if (CanGoForward) {
			forwardButton.Modulate = new Color(1, 1, 1, 1f);
		} else {
			forwardButton.Modulate = new Color(1, 1, 1, 0.5f);
		}

		if (newPanelState.Value.type == GamePanelType.Unit) {
			currentPanelView = (GamePanelView) unitPanelScene.Instance();
		} else if (newPanelState.Value.type == GamePanelType.Tile) {
			currentPanelView = (GamePanelView) tilePanelScene.Instance();
		}
		content.AddChild(currentPanelView);
	}

	private void handleBack() {
		if (CanGoBack) {
			PanelBack();
		}
		GetTree().SetInputAsHandled();
	}

	private void handleForward() {
		if (CanGoForward) {
			PanelForward();
		}
		GetTree().SetInputAsHandled();
	}

	private void handleClose() {
		PanelClose();
		GetTree().SetInputAsHandled();
	}

	public void SetTitle(string panelTitle) {
		title.Text = panelTitle;
	}

	public void PanelSet(GamePanelState gamePanelState) {
		panelHistory.Add(gamePanelState);
		currentPanelIndex = panelHistory.Count - 1;
		PanelSubject.OnNext(panelHistory[currentPanelIndex]);
	}

	public bool CanGoBack {
		get { return currentPanelIndex > 0; }
	}

	public bool CanGoForward {
		get { return (currentPanelIndex + 1) < (panelHistory.Count); }
	}

	public void PanelBack() {
		if (currentPanelIndex > 0) {
			currentPanelIndex--;
		}
		PanelSubject.OnNext(panelHistory[currentPanelIndex]);
	}

	public void PanelForward() {
		currentPanelIndex++;
		PanelSubject.OnNext(panelHistory[currentPanelIndex]);
	}

	public void PanelClose() {
		panelHistory.Clear();
		currentPanelIndex = 0;
		PanelSubject.OnNext(null);
	}

	public GamePanelState? CurrentPanel {
		get { return PanelSubject.Value; }
	}

	public BehaviorSubject<GamePanelState?> PanelSubject = new BehaviorSubject<GamePanelState?>(null);

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			if (CurrentPanel.HasValue) {
				PanelClose();
				GetTree().SetInputAsHandled();
			}
		}
	}
}
