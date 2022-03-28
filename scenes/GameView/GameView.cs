using Godot;

public class GameView : Control {
	SimpleInjector.Container container;
	public GameContext context;
	private Label desc;
	private ProgressBar progress;
	private GameGeneratorThread generatorThread;
	private GameController gameController;

	public override void _Ready() {
		context = (GameContext) GetTree().Root.GetNode("GameContext");
		desc = (Label) GetNode("LoadingDisplay/MarginContainer/VBoxContainer/Desc");
		progress = (ProgressBar) GetNode("LoadingDisplay/MarginContainer/VBoxContainer/ProgressBar");

		var gameControllerScene = (PackedScene) ResourceLoader.Load("res://scenes/GameView/GameController.tscn");
		gameController = (GameController) gameControllerScene.Instance();

		var options = new GameOptions();
		generatorThread = new GameGeneratorThread(
			options,
			new GeneratorProgress(OnGeneratorProgress),
			new GameGeneratedCallback(OnGameGenerated)
		);
		generatorThread.game = new Game();
		var t = new System.Threading.Thread(generatorThread.Generate);
		t.Start();
	}

	private void OnGeneratorProgress(string label, int value) {
		desc.Text = label;
		progress.Value = value;
	}

	private void OnGameGenerated() {
		GD.PrintS("Game generated");
		context.OnGameInit(generatorThread.game);

		var watch = System.Diagnostics.Stopwatch.StartNew();
		gameController.StartGame(generatorThread.game);
		CallDeferred("add_child", gameController);
		GD.PrintS($"GameController init: {watch.ElapsedMilliseconds}ms");
	}
}
