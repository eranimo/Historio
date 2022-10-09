using Godot;
using System;
using System.Reactive.Subjects;


public enum GameMapInputType {
	LeftClick,
	RightClick,
	Hovered,
}
public struct GameMapInput {
	public Hex hex;
	public GameMapInputType type;
	public bool isShiftModifier;
}

public static class ConsoleExtensions {
	public static CommandBuilder RemoveCommand(this Console console, string name) {
		var _console = console.GetTree().Root.GetNode<CanvasLayer>("Console");
		Godot.Object consoleCommand = _console.Call("remove_command", name) as Godot.Object;
		return new CommandBuilder(consoleCommand);
	}
}

public class GameController : Control {
	public Game game;

	public Viewport GameViewport { get; private set; }

	public GamePanel GamePanel;

	public GameMenu GameMenu { get; private set; }

	public GameMap GameMap;

	public Subject<GameMapInput> gameMapInputSubject = new Subject<GameMapInput>();

	public override void _Ready() {
		GD.PrintS("(GameController) start game");
		GameMap = GetNode<GameMap>("GameViewport/Viewport/GameMap");
		var minimap = GetNode<Minimap>("Minimap");
		game.manager.state.AddElement(this);
		game.manager.state.AddElement<GameMap>(GameMap);
		game.manager.state.AddElement<Minimap>(minimap);
		GameMap.RenderMap(game);
		minimap.RenderMap(game);
		
		GameViewport = (Viewport) GetNode("GameViewport/Viewport");
		GamePanel = (GamePanel) GetNode("GamePanel");
		GameMenu = (GameMenu) GetNode("%GameMenu");

		game.Start();


		var console = GetTree().Root.GetNode<Console>("CSharpConsole");

		console.RemoveCommand("next_day");
		console.RemoveCommand("observe");
		console.RemoveCommand("play");

		console.AddCommand("next_day", this, nameof(handleNextDay))
			.SetDescription("Processes the next day in the game")
			.Register();

		console.AddCommand("observe", this, nameof(handleObserve))
			.SetDescription("Enable observer mode")
			.Register();

		console.AddCommand("play", this, nameof(handlePlay))
			.SetDescription("Play as as the specified country")
			.AddArgument("country_id", Variant.Type.Int)
			.Register();
	}

	public Entity currentUnit {
		get {
			if (GamePanel.CurrentPanel.HasValue && GamePanel.CurrentPanel.Value.type == GamePanelType.Unit) {
				return GamePanel.CurrentPanel.Value.entity;
			}
			return null;
		}
	}

	private void handleNextDay() {
		for (int i = 0; i <= game.speedTicks; i++) {
			game.Process(1f, true);
		}
	}

	private void handleObserve() {
		game.state.GetElement<Player>().playerCountry = null;
		game.state.Send(new PlayerChanged());
	}

	private void handlePlay(int country_id) {
		GD.PrintS("Play as", country_id);
	}

	public override void _Process(float delta) {
		game.Process(delta);
	}
}
