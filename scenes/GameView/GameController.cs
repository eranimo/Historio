using Godot;
using System;

public class GameController : Control {
	private Game activeGame;

	public void StartGame(Game game) {
		this.activeGame = game;
		GD.PrintS("(GameController) start game");
		game.Start();
		var gameMap = GetNode<GameMap>("ViewportContainer/Viewport/GameMap");
		gameMap.RenderMap(game);
	}

	public override void _Process(float delta) {
		activeGame.Process(delta);
	}
}
