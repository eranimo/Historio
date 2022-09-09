using Godot;
using System;

public class GameController : Control {
	public Game game;

	public Viewport GameViewport { get; private set; }

	public override void _Ready() {
		GD.PrintS("(GameController) start game");
		var gameMap = GetNode<GameMap>("GameViewport/Viewport/GameMap");
		var minimap = GetNode<Minimap>("Minimap");
		game.manager.state.AddElement<GameMap>(gameMap);
		game.manager.state.AddElement<Minimap>(minimap);
		gameMap.RenderMap(game);
		minimap.RenderMap(game);
		
		GameViewport = (Viewport) GetNode("GameViewport/Viewport");

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
