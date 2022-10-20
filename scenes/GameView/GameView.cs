using RelEcs;
using Godot;
using System;
using System.Reactive.Subjects;
using System.Linq;


public class GameGeneratorThread {
	private readonly GameOptions options;
	public event Progress OnProgress;
	public event Done OnDone;

	public delegate void Progress(string label, int value);
	public delegate void Done();

	public Game game;

	public GameGeneratorThread(GameOptions options) {
		this.options = options;
	}

	public void Generate() {
		var watch = System.Diagnostics.Stopwatch.StartNew();

		OnProgress.Invoke("Generating world", 0);
		new WorldGenerator().Generate(options, game.manager);

		OnProgress.Invoke("Generating countries", 50);
		new CountryGenerator().Generate(options, game.manager);

		OnProgress.Invoke("Finished game generation", 100);

		GD.PrintS($"GameGeneratorThread: {watch.ElapsedMilliseconds}ms");
		if (OnDone != null) {
			OnDone.Invoke();
		}
	}
}

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
		desc = (Label) GetNode("%Desc");
		progress = (ProgressBar) GetNode("%ProgressBar");

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

	public override void _ExitTree() {
		base._ExitTree();
		GameController.QueueFree();
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

	private void handleConsoleToggle(bool toggled) {
		isConsoleToggled = toggled;
	}

	/*
	LOAD GAME
	*/
	private void handleLoadGame() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		GD.PrintS("(GameView) handle load game");
		game = new Game();
		GameController.game = game;
		game.state.Send(new LoadGameTrigger {
			savedGame = loadState.savedGame,
			saveEntry = loadState.saveEntry,
		});
		desc.Text = "Loading game";
		progress.Value = 0;


		game.OnGameLoaded += (SavedGameEntry entry) => {
			GD.PrintS($"(GameView) on game loaded: {watch.ElapsedMilliseconds}ms");
			desc.Text = "Loaded game";
			progress.Value = 100;
			CallDeferred("add_child", GameController);
		};

		game.Init();
		loadState.savedGame = null;
		loadState.saveEntry = null;
	}

	/*
	NEW GAME
	*/
	private void handleNewGame() {
		var options = new GameOptions();
		generatorThread = new GameGeneratorThread(options);
		generatorThread.OnProgress += (string label, int value) => {
			desc.Text = label;
			progress.Value = value;
		};
		generatorThread.OnDone += onGameGenerated;

		generatorThread.game = new Game();
		var t = new System.Threading.Thread(generatorThread.Generate);
		t.Start();
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
}
