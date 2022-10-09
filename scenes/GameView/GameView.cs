using RelEcs;
using Godot;
using System;
using System.Reactive.Subjects;
using System.Linq;

public class GameView : Control {
	private Label desc;
	private ProgressBar progress;
	private GameGeneratorThread generatorThread;
	public GameController GameController;

	public Game game;
	public BehaviorSubject<float> zoom = new BehaviorSubject<float>(1);
	public BehaviorSubject<Vector2> pan = new BehaviorSubject<Vector2>(new Vector2(0, 0));
	public IObservable<float> OnZoom { get => zoom; }
	public bool isConsoleToggled { get; private set; }

	public GameCamera camera;
	private LoadState loadState;

	public override void _Ready() {
		desc = (Label) GetNode("LoadingDisplay/MarginContainer/VBoxContainer/Desc");
		progress = (ProgressBar) GetNode("LoadingDisplay/MarginContainer/VBoxContainer/ProgressBar");

		var gameControllerScene = (PackedScene) ResourceLoader.Load("res://scenes/GameView/GameController.tscn");
		GameController = (GameController) gameControllerScene.Instance();

		var console = GetTree().Root.GetNode<CanvasLayer>("Console");
		console.Connect("toggled", this, nameof(handleConsoleToggle));

		loadState = (LoadState) GetTree().Root.GetNode("LoadState");

		if (loadState.savedGame is null) {
			GD.PrintS("(GameView) new game");
			handleNewGame(); 
		} else {
			GD.PrintS("(GameView) load game");
			handleLoadGame();
		}
	}

	private void handleLoadGame() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		game = new Game();
		GameController.game = game;
		game.state.Send(new LoadGameTrigger {
			savedGame = loadState.savedGame,
			saveEntry = loadState.saveEntry,
		});

		game.OnGameLoaded += () => {
			onGameLoaded();
			GD.PrintS($"(GameView) on game loaded: {watch.ElapsedMilliseconds}ms");
		};

		game.Init();

		desc.Text = "Loading game";
		progress.Value = 0;
		loadState.savedGame = null;
		loadState.saveEntry = null;
	}

	private void onGameLoaded() {
		desc.Text = "Loaded game";
		progress.Value = 100;
		CallDeferred("add_child", GameController);
	}

	private void handleNewGame() {
		var options = new GameOptions();
		generatorThread = new GameGeneratorThread(
			options,
			new GeneratorProgress(onGeneratorProgress),
			new GameGeneratedCallback(onGameGenerated)
		);
		generatorThread.game = new Game();
		var t = new System.Threading.Thread(generatorThread.Generate);
		t.Start();
	}

	private void handleConsoleToggle(bool toggled) {
		isConsoleToggled = toggled;
	}

	private void onGeneratorProgress(string label, int value) {
		desc.Text = label;
		progress.Value = value;
	}

	private void onGameGenerated() {
		GD.PrintS("Game generated");
		game = generatorThread.game;

		// generate new SavedGame
		var countryName = game.manager.Get<CountryData>(game.state.GetElement<Player>().playerCountry).name;
		var countryNameSafe = System.IO.Path.GetInvalidFileNameChars().Aggregate(countryName, (f, c) => f.Replace(c, '_'));
		var rng = new Godot.RandomNumberGenerator();
		var saveName = $"{countryNameSafe}-{rng.RandiRange(1, 10000)}";
		game.savedGame = new SavedGameMetadata { name = saveName };

		var watch = System.Diagnostics.Stopwatch.StartNew();
		GameController.game = game;

		game.Init();
		CallDeferred("add_child", GameController);
		GD.PrintS($"(GameView) on game generated: {watch.ElapsedMilliseconds}ms");
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (isConsoleToggled) {
			return;
		}

		if (@event.IsActionPressed("game_toggle_play")) {
			if (game.IsPlaying) {
				game.Pause();
			} else {
				game.Play();
			}
			GetTree().SetInputAsHandled();
		} else if (@event.IsActionPressed("game_speed_down")) {
			game.Slower();
			GetTree().SetInputAsHandled();
		} else if (@event.IsActionPressed("game_speed_up")) {
			game.Faster();
			GetTree().SetInputAsHandled();
		} else if (@event.IsActionPressed("ui_cancel")) {
			GameController.GameMenu.ShowMenu();
			GetTree().SetInputAsHandled();
		}
	}
}
