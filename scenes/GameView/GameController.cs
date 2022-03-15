using Godot;
using System;

public class GameController : Control {
	public void StartGame(Game game) {
		GD.PrintS("(GameController) start game");
		game.Start();
		var gameMap = GetNode<GameMap>("ViewportContainer/Viewport/GameMap");
		gameMap.RenderMap(game);
	}
}
