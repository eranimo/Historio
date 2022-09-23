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

public class GameController : Control {
	public Game game;

	public Viewport GameViewport { get; private set; }

	public GamePanel GamePanel;
	public GameMap GameMap;

	public Subject<GameMapInput> gameMapInputSubject = new Subject<GameMapInput>();

	public override void _Ready() {
		GD.PrintS("(GameController) start game");
		GameMap = GetNode<GameMap>("GameViewport/Viewport/GameMap");
		var minimap = GetNode<Minimap>("Minimap");
		game.manager.state.AddElement<GameMap>(GameMap);
		game.manager.state.AddElement<Minimap>(minimap);
		GameMap.RenderMap(game);
		minimap.RenderMap(game);
		
		GameViewport = (Viewport) GetNode("GameViewport/Viewport");
		GamePanel = (GamePanel) GetNode("GamePanel");

		game.Start();


		var console = GetTree().Root.GetNode<Console>("CSharpConsole");

		console.AddCommand("next_day", this, nameof(CommandNextDay))
			.SetDescription("Processes the next day in the game")
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

	private void CommandNextDay() {
		for (int i = 0; i <= game.speedTicks; i++) {
			game.Process(1f, true);
		}
	}

	public override void _Process(float delta) {
		game.Process(delta);
	}
}
