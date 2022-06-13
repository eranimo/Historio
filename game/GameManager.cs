using System.Collections.Generic;
using RelEcs;
using System;
using Godot;

public class PhysicsDelta {
	public float delta;
}

public class GameManager {
	public RelEcs.World state;

	// systems that run when the game starts (either initially or when loading a game)
	SystemGroup startSystems = new SystemGroup();

	// runs every day (game)
    SystemGroup daySystems = new SystemGroup();

	// runs every 60 FPS, before day systems
	SystemGroup tickSystems = new SystemGroup();

	// runs after game ends or player exists
    SystemGroup stopSystems = new SystemGroup();

	// runs every day and at game start
   	SystemGroup renderSystems = new SystemGroup();

	// runs every tick and at game start
	SystemGroup frameSystems = new SystemGroup();

	public World world;
	private PhysicsDelta physicsDelta;

	public GameManager() {
		world = new World(this);
		state = new RelEcs.World();
		state.AddElement(new Layout(new Point(16.666, 16.165), new Point(16 + .5, 18 + .5)));
		state.AddElement(new MapViewState(this));
		physicsDelta = new PhysicsDelta();
		state.AddElement(physicsDelta);
		state.AddElement(new SelectedUnit());

		// services
		state.AddElement(world);
		state.AddElement(new Pathfinder(this));

		// factories
		state.AddElement(new UnitFactory(this));


		daySystems
			.Add(new ActionSystem())
			.Add(new PathfindingSystem())
			.Add(new MovementSystem());
		
		tickSystems
			.Add(new MovementTweenSystem())
			.Add(new ViewStateSystem());
	
		startSystems
			.Add(new ViewStateStartupSystem())
			.Add(new ViewStateSystem())
			.Add(new PlayerStartSystem());

		renderSystems
			.Add(new SpriteRenderSystem())
			.Add(new UnitRenderSystem())
			.Add(new BorderRenderSystem());
		
		frameSystems
			.Add(new UnitSelectionSystem())
			.Add(new UnitPanelUISystem())
			.Add(new ActionTickSystem());
	}

	// called when game starts
	public void Start(GameDate date) {
		state.AddElement<GameDate>(date);
		Godot.GD.PrintS("(GameManager) start");
		startSystems.Run(state);
		renderSystems.Run(state);
		frameSystems.Run(state);
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

	public void Process(float delta) {
		try {
			physicsDelta.delta = delta;
			tickSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing tick: ", err);
		}
	}

	public void UIProcess(float delta) {
		frameSystems.Run(state);
	}
}
