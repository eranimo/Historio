using Godot;
using System;
using System.Reactive.Subjects;

// console triggers
public partial class DebugObserve {
}

public partial class DebugPlay {
	public string countryName;
}

public partial class DebugListCountries {}


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

public partial class GameStart {} 

public partial class GameController : Control {
	public Game game;

	public SubViewport GameViewport { get; private set; }

	public GamePanel GamePanel;

	public GameMenu GameMenu { get; private set; }

	public GameMap GameMap;

	public Subject<GameMapInput> gameMapInputSubject = new Subject<GameMapInput>();

	public override void _Ready() {
		GD.PrintS("(GameController) start game");
		GameMap = GetNode<GameMap>("GameViewport/SubViewport/GameMap");
		var minimap = GetNode<Minimap>("Minimap");
		game.manager.state.AddElement(this);
		game.manager.state.AddElement<GameMap>(GameMap);
		game.manager.state.AddElement<Minimap>(minimap);
		GameMap.RenderMap(game);
		minimap.RenderMap(game);
		
		GameViewport = (SubViewport) GetNode("GameViewport/SubViewport");
		GamePanel = (GamePanel) GetNode("GamePanel");
		GameMenu = (GameMenu) GetNode("%GameMenu");

		try {
			game.state.Send(new GameStart());
			game.Start();
		} catch (Exception err) {
			GD.PrintErr("Error starting game:", err);
		}

		// setup map mode dropdown
		var mapModeDropdown = GetNode<OptionButton>("%MapModeDropdown");
		PopupMenu popupMenu = mapModeDropdown.GetPopup();
		foreach (var (mapMode, index) in MapModes.List.WithIndex()) {
			popupMenu.AddItem(mapMode.Name, index);
		}
		mapModeDropdown.Connect("item_selected",new Callable(this,nameof(handleMapModeSelect)));
		MapModes.CurrentMapMode.Subscribe((MapMode mapMode) => {
			mapModeDropdown.Select(MapModes.List.IndexOf(mapMode));
		});

		setupConsole();
	}

	private void handleMapModeSelect(int index) {
		var mapMode = MapModes.List[index];
		MapModes.CurrentMapMode.OnNext(mapMode);
	}

	private void setupConsole() {
		// console = GetTree().Root.GetNode<Console>("CSharpConsole");

		// console.RemoveCommand("next_day");
		// console.RemoveCommand("observe");
		// console.RemoveCommand("play");

		// console.AddCommand("next_day", this, nameof(handleNextDay))
		// 	.SetDescription("Processes the next day in the game")
		// 	.Register();

		// console.AddCommand("observe", this, nameof(handleObserve))
		// 	.SetDescription("Enable observer mode")
		// 	.Register();

		// console.AddCommand("play", this, nameof(handlePlay))
		// 	.SetDescription("Play as as the specified country")
		// 	.AddArgument("country_name", Variant.Type.String)
		// 	.Register();

		// console.AddCommand("list_countries", this, nameof(handleListCountries))
		// 	.SetDescription("List all countries in the game")
		// 	.Register();
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
		game.state.Send(new DebugObserve());
	}

	private void handlePlay(string countryName) {
		game.state.Send(new DebugPlay { countryName = countryName });
	}

	private void handleListCountries() {
		game.state.Send(new DebugListCountries());
	}

	public override void _Process(double delta) {
		game.Process(delta);
	}
}
