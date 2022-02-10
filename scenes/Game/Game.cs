using Godot;
using System;

public class Game : Node2D {
	public override void _Ready() {
		var gameMap = GetNode<GameMap>("GameMap");

		var generator = new WorldGenerator();
		GameWorld world = generator.Generate();

		gameMap.RenderMap(world);
	}
}
