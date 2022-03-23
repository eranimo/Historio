using Godot;
using System;
using System.Threading;
using System.Reactive.Subjects;

public delegate void GeneratorProgress(string label, int value);
public delegate void WorldGeneratedCallback();

public class WorldGeneratorThread {
	private readonly WorldOptions options;
	private GeneratorProgress progress;
	private WorldGeneratedCallback callback;

	public Game game;
	private Random random = new Random();

	public WorldGeneratorThread(
		WorldOptions options,
		GeneratorProgress progress,
		WorldGeneratedCallback callback
	) {
		this.options = options;
		this.progress = progress;
		this.callback = callback;
	}

	public void Generate(object obj) {
		progress("Generating world", 0);
		var watch = System.Diagnostics.Stopwatch.StartNew();
		var generator = new WorldGenerator(options);

		generator.options.Size = WorldSize.Medium;
		generator.options.Seed = random.Next();

		generator.Generate(game.manager);

		GD.PrintS($"WorldGenerator: {watch.ElapsedMilliseconds}ms");
		progress("Generating world", 100);
		if (callback != null) {
			callback();
		}
	}
}

public class GameView : Control {
	SimpleInjector.Container container;
	public GameContext context;
	private Label desc;
	private ProgressBar progress;
	private WorldGeneratorThread generatorThread;
	private GameController gameController;

	public override void _Ready() {
		context = (GameContext) GetTree().Root.GetNode("GameContext");
		desc = (Label) GetNode("LoadingDisplay/MarginContainer/VBoxContainer/Desc");
		progress = (ProgressBar) GetNode("LoadingDisplay/MarginContainer/VBoxContainer/ProgressBar");

		var gameControllerScene = (PackedScene) ResourceLoader.Load("res://scenes/GameView/GameController.tscn");
		gameController = (GameController) gameControllerScene.Instance();

		var options = new WorldOptions();
		generatorThread = new WorldGeneratorThread(
			options,
			new GeneratorProgress(OnGeneratorProgress),
			new WorldGeneratedCallback(OnWorldGenerated)
		);
		generatorThread.game = new Game();
		var t = new System.Threading.Thread(generatorThread.Generate);
		t.Start(options);
	}

	private void OnGeneratorProgress(string label, int value) {
		desc.Text = label;
		progress.Value = value;
	}

	private void OnWorldGenerated() {
		GD.PrintS("World generated");
		context.OnGameInit(generatorThread.game);

		var watch = System.Diagnostics.Stopwatch.StartNew();
		gameController.StartGame(generatorThread.game);
		CallDeferred("add_child", gameController);
		GD.PrintS($"GameController init: {watch.ElapsedMilliseconds}ms");
	}
}
