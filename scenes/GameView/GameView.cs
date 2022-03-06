using Godot;
using SimpleInjector;
using System;

public class GameViewAttribute : Attribute {}

public class GameView : Control {
	SimpleInjector.Container container;
	public GameContext context;

	public override void _Ready() {
		context = (GameContext) GetTree().Root.GetNode("GameContext");

		var watch = System.Diagnostics.Stopwatch.StartNew();
		var generator = new WorldGenerator();
		generator.options.Size = WorldSize.Medium;
		GameWorld world = generator.Generate();
		Game game = new Game();
		game.world = world;
		context.OnGameInit(game);
		GD.PrintS($"WorldGenerator: {watch.ElapsedMilliseconds}ms");

		watch = System.Diagnostics.Stopwatch.StartNew();
		var gameControllerScene = (PackedScene) ResourceLoader.Load("res://scenes/GameView/GameController.tscn");
		var gameController = (GameController) gameControllerScene.Instance();
		gameController.game = game;
		AddChild(gameController);
		GD.PrintS($"GameController init: {watch.ElapsedMilliseconds}ms");
	}

	public override void _ExitTree() {
		// TODO: implement exit
	}

	public override void _EnterTree() {
		base._EnterTree();
		container = new SimpleInjector.Container();
		container.Register<GameView>(Lifestyle.Singleton);
		container.Verify();
	}
}
