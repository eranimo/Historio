using Godot;
using System;

public class GameController : Control {
	public Game game;

	public override void _Ready() {
		GD.PrintS("(GameController) start game");
		var gameMap = GetNode<GameMap>("GameViewport/Viewport/GameMap");
		game.manager.state.AddElement<GameMap>(gameMap);
		gameMap.RenderMap(game);
		game.Start();


		var console = GetTree().Root.GetNode<Console>("CSharpConsole");

		console.AddCommand("next_day", this, nameof(CommandNextDay))
			.SetDescription("Processes the next day in the game")
			.Register();
	}

	private void CommandNextDay() {
		for (int i = 0; i <= game.speedTicks; i++) {
			game.Process(1f, true);
		}
	}

	public override void _Process(float delta) {
		game.Process(delta);
	}
}
