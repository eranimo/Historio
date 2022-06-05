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
	}

	public override void _Process(float delta) {
		game.Process(delta);
	}
}
