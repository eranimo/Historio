using RelEcs;
using Godot;
using System;
using System.Reactive.Subjects;

public class GameView : Control {
	private Label desc;
	private ProgressBar progress;
	private GameGeneratorThread generatorThread;
	private GameController gameController;

	public Game game;
	public BehaviorSubject<float> zoom = new BehaviorSubject<float>(1);
	public BehaviorSubject<bool> mapInputEnabled = new BehaviorSubject<bool>(false);
	public IObservable<float> OnZoom { get => zoom; }

	public override void _Ready() {
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
		game = generatorThread.game;

		var watch = System.Diagnostics.Stopwatch.StartNew();
		gameController.game = game;
		CallDeferred("add_child", gameController);
		GD.PrintS($"GameController init: {watch.ElapsedMilliseconds}ms");
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (@event.IsActionPressed("game_toggle_play")) {
			if (game.IsPlaying) {
				game.Pause();
			} else {
				game.Play();
			}
		} else if (@event.IsActionPressed("game_speed_down")) {
			game.Slower();
		} else if (@event.IsActionPressed("game_speed_up")) {
			game.Faster();
		}
	}
}
