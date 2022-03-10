using Godot;
using System;

public class GameController : Control {
	public Game game;

	public void StartGame(Game game) {
		this.game = game;
		var gameMap = GetNode<GameMap>("ViewportContainer/Viewport/GameMap");
		gameMap.RenderMap(game.world);
	}
}
