using System.Collections.Generic;
using RelEcs;
using System;
using Godot;
using System.Linq;

public partial class PhysicsDelta {
	public double delta;
}

public static class Defs {
	public static DefStore<DistrictType> District = new DefStore<DistrictType>("District", "districts");
	public static DefStore<ImprovementType> Improvement = new DefStore<ImprovementType>("Improvement", "improvements");
	public static DefStore<UnitType> Unit = new DefStore<UnitType>("Unit", "units");
	public static DefStore<ResourceType> Resource = new DefStore<ResourceType>("Resource", "resources");
	public static DefStore<BuildingType> Building = new DefStore<BuildingType>("Building", "buildings");
	public static DefStore<PopProfessionType> PopProfession = new DefStore<PopProfessionType>("PopProfession", "popProfessions");
	public static DefStore<BiotaType> BiotaType = new DefStore<BiotaType>("BiotaType", "biotaType");
}

public partial class DebugDaySystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		// World.GetElement<GameController>().PrintOrphanNodes();
	}
}

public static class QueryExtensions {
	public static int Count<T>(this Query<T> query) where T : class {
		int c = 0;
		foreach (var item in query) {
			c++;
		}
		return c;
	}
}

public partial class DebugStartSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		GD.PrintS("(DebugStartSystem) entity counts:");
		
		GD.PrintS("\t Tiles:", World.Query<TileData>().Build().Count());
		GD.PrintS("\t Countries:", World.Query<CountryData>().Build().Count());
		GD.PrintS("\t Settlements:", World.Query<SettlementData>().Build().Count());
		GD.PrintS("\t Country Tiles:", World.Query<CountryTile>().Build().Count());
		GD.PrintS("\t Units:", World.Query<UnitData>().Build().Count());
	}
}

public partial class DebugTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		foreach (var e in World.Receive<DebugObserve>(this)) {
			World.GetElement<Player>().playerCountry = null;
			World.Send(new PlayerChanged());
		}

		foreach (var e in World.Receive<DebugPlay>(this)) {
			var countries = World.Query<Entity, CountryData>().Build();
			foreach (var (country, countryData) in countries) {
				if (countryData.name == e.countryName) {
					GD.PrintS("Play as", e.countryName);
					World.GetElement<Player>().playerCountry = country;
					World.Send(new PlayerChanged());
					break;
				}
			}
		}

		// foreach (var e in World.Receive<DebugListCountries>(this)) {
		// 	var countries = World.Query<CountryData>().Build();
		// 	var countryList = new List<string>();
		// 	foreach (var countryData in countries) {
		// 		countryList.Add(countryData.name);
		// 	}
		// 	World.GetElement<GameController>().console.WriteLine(String.Join(", ", countryList));
		// }
	} 
}

public partial class GameManager {
	public RelEcs.World state;

	// runs when game is starts (before scene is loaded)
	SystemGroup initSystems = new SystemGroup();

	// systems that run when the game starts (scene is loaded)
	SystemGroup startSystems = new SystemGroup();

	// runs every day (depending on game speed)
    SystemGroup daySystems = new SystemGroup();

	// runs every tick while game is playing, before day systems, and also at game start
	SystemGroup playSystems = new SystemGroup();

	// runs every tick regardless of play state, and also at game start
	SystemGroup tickSystems = new SystemGroup();

	// runs after game ends or player exists
    SystemGroup stopSystems = new SystemGroup();

	// runs every day and at game start
   	SystemGroup renderSystems = new SystemGroup();

	// runs on every tick when game menu is open
	SystemGroup menuSystems = new SystemGroup();

	public WorldService world;
	private PhysicsDelta physicsDelta;

	public GameManager() {
		daySystems
			.Add(new ActionDaySystem())
			.Add(new MovementDaySystem())
			.Add(new DebugDaySystem());
			// .Add(new BiotaDaySystem());
		
		playSystems
			.Add(new MovementTweenPlaySystem());

		initSystems
			.Add(new SaveSystem());
	
		startSystems
			.Add(new DebugStartSystem())
			.Add(new GameMapTickSystem());

		renderSystems
			.Add(new MapModeSystem())
			.Add(new SpriteRenderSystem())
			.Add(new UnitRenderSystem())
			.Add(new BorderRenderSystem());
		
		tickSystems
			.Add(new DebugTickSystem())
			.Add(new UnitPanelTickSystem())
			.Add(new ActionTickSystem())
			.Add(new UnitPathTickSystem())
			.Add(new ViewStateSystem())
			.Add(new MinimapRenderSystem());

		menuSystems
			.Add(new SaveSystem())
			.Add(new SaveModalTickSystem());

		world = new WorldService(this);
		state = new RelEcs.World();
		state.AddElement(new Layout(new Point(16.666, 16.165), new Point(16 + .5, 18 + .5)));
		physicsDelta = new PhysicsDelta();
		state.AddElement(physicsDelta);
		state.AddElement(this);

		// services
		state.AddElement(world);
		state.AddElement(new PathfindingService(this));
		state.AddElement(new Factories(this));
		state.AddElement(new BiotaService(this));
		state.AddElement(new SaveManager());
	}

	public void Init() {
		initSystems.Run(state);
	}

	// called when game starts
	public void Start(GameDate date) {
		try {
			state.AddElement<GameDate>(date);
			Godot.GD.PrintS("(GameManager) start");
			startSystems.Run(state);
			tickSystems.Run(state);
			playSystems.Run(state);
			renderSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error starting game: ", err);
		}
	}

	// called when game stops
	public void Stop() {
		stopSystems.Run(state);
	}

	public void ProcessDay() {
		try {
			daySystems.Run(state);
			renderSystems.Run(state);
			state.Tick();
		} catch (Exception err) {
			GD.PrintErr("Error processing day: ", err);
		}
	}

	public void ProcessPlaying(double delta) {
		try {
			physicsDelta.delta = delta;
			playSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing playing tick: ", err);
		}
	}

	public void Process(double delta) {
		try {
			tickSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing tick: ", err);
		}
	}

	public void ProcessMenu() {
		try {
			menuSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing menu tick: ", err);
		}
	}

	public T Get<T>(Entity entity) where T : class {
		return state.GetComponent<T>(entity);
	}

	public T Get<T>(Entity entity, Entity target) where T : class {
		return state.GetComponent<T>(entity, target);
	}

	public T Get<T>(Entity entity, Type type) where T : class {
		var typeIdentity = state.GetTypeEntity(type);
		return state.GetComponent<T>(entity, typeIdentity);
	}

	public Entity GetTarget<T>(Entity entity) where T : class {
		return state.GetTarget<T>(entity);
	}

	public EntityBuilder Spawn() {
		return state.Spawn();
	}

	public EntityBuilder On(Entity entity) {
		return new EntityBuilder(state, entity);
	}
}
