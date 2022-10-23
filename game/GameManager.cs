using System.Collections.Generic;
using RelEcs;
using System;
using Godot;
using System.Linq;

public class PhysicsDelta {
	public float delta;
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

public class DebugDaySystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		this.GetElement<GameController>().PrintStrayNodes();
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

public class DebugStartSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		GD.PrintS("(DebugStartSystem) entity counts:");
		
		GD.PrintS("\t Tiles:", this.Query<TileData>().Count());
		GD.PrintS("\t Countries:", this.Query<CountryData>().Count());
		GD.PrintS("\t Settlements:", this.Query<SettlementData>().Count());
		GD.PrintS("\t Country Tiles:", this.Query<CountryTile>().Count());
		GD.PrintS("\t Units:", this.Query<UnitData>().Count());
	}
}

public class DebugTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		foreach (var e in this.Receive<DebugObserve>()) {
			this.GetElement<Player>().playerCountry = null;
			this.Send(new PlayerChanged());
		}

		foreach (var e in this.Receive<DebugPlay>()) {
			var countries = this.Query<Entity, CountryData>();
			foreach (var (country, countryData) in countries) {
				if (countryData.name == e.countryName) {
					GD.PrintS("Play as", e.countryName);
					this.GetElement<Player>().playerCountry = country;
					this.Send(new PlayerChanged());
					break;
				}
			}
		}

		foreach (var e in this.Receive<DebugListCountries>()) {
			var countries = this.Query<CountryData>();
			var countryList = new List<string>();
			foreach (var countryData in countries) {
				countryList.Add(countryData.name);
			}
			this.GetElement<GameController>().console.WriteLine(String.Join(", ", countryList));
		}
	} 
}

public class GameManager {
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
			.Add(new DebugDaySystem())
			.Add(new BiotaDaySystem());
		
		playSystems
			.Add(new MovementTweenPlaySystem());

		initSystems
			.Add(new SaveSystem());
	
		startSystems
			.Add(new DebugStartSystem())
			.Add(new GameMapTickSystem());

		renderSystems
			.Add(new SpriteRenderSystem())
			.Add(new UnitRenderSystem())
			.Add(new BorderRenderSystem())
			.Add(new MinimapRenderSystem());
		
		tickSystems
			.Add(new DebugTickSystem())
			.Add(new UnitPanelTickSystem())
			.Add(new ActionTickSystem())
			.Add(new UnitPathTickSystem())
			.Add(new ViewStateSystem());

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

	public void ProcessPlaying(float delta) {
		try {
			physicsDelta.delta = delta;
			playSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing playing tick: ", err);
		}
	}

	public void Process(float delta) {
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
		return state.GetComponent<T>(entity.Identity);
	}

	public T Get<T>(Entity entity, Entity target) where T : class {
		return state.GetComponent<T>(entity.Identity, target.Identity);
	}

	public T Get<T>(Entity entity, Type type) where T : class {
		var typeIdentity = state.GetTypeIdentity(type);
		return state.GetComponent<T>(entity.Identity, typeIdentity);
	}

	public Entity GetTarget<T>(Entity entity) where T : class {
		return state.GetTarget<T>(entity.Identity);
	}

	public EntityBuilder Spawn() {
		return new EntityBuilder(state, state.Spawn().Identity);
	}

	public EntityBuilder On(Entity entity) {
		return new EntityBuilder(state, entity.Identity);
	}
}
