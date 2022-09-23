using Godot;
using System;

public abstract class GamePanelView : Control {
	protected GameView gameView;
	protected RelEcs.World state;
	protected GamePanel gamePanel;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		state = gameView.game.manager.state;
		gamePanel = state.GetElement<GamePanel>();
	}

	public abstract void UpdateView(Entity entity);

	public abstract void ResetView(Entity entity);
}
