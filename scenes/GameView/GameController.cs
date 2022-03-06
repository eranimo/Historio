using Godot;
using System;

public class GameController : Control {
	public Game game;

	public override void _Ready() {
		var gameMap = GetNode<GameMap>("ViewportContainer/Viewport/GameMap");
		gameMap.RenderMap(game.world);
	}
}
