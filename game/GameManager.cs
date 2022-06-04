using System.Collections.Generic;
using RelEcs;


public class ViewSystem : ISystem {
	void ISystem.Run(Commands commands) {
		Process(commands);
	}
	public virtual void Process(Commands commands) {}
}

public class GameManager {
	public RelEcs.World state;

	// systems that run when the game starts (either initially or when loading a game)
	SystemGroup startSystems = new SystemGroup();

	// runs every day (game)
    SystemGroup runSystems = new SystemGroup();

	// runs after game ends or player exists
    SystemGroup stopSystems = new SystemGroup();

	// runs every day and at game start
   	SystemGroup renderSystems = new SystemGroup();

	public World world;

	public GameManager() {
		world = new World(this);
		state = new RelEcs.World();
		state.AddElement(world);
		state.AddElement(new Layout(new Point(16.666, 16.165), new Point(16 + .5, 18 + .5)));
		state.AddElement(new MapViewState(this));
		state.AddElement(new Pathfinder(this));

		runSystems
			.Add(new PathfindingSystem())
			.Add(new MovementSystem());
		
		startSystems
			.Add(new ViewStateStartupSystem());

		renderSystems
			.Add(new SpriteRenderSystem())
			.Add(new TerritoryRenderSystem())
			.Add(new ViewStateSystem());
	}

	// called when game starts
	public void Start() {
		Godot.GD.PrintS("(GameManager) start");
		startSystems.Run(state);
		renderSystems.Run(state);
	}

	// called when game stops
	public void Stop() {
		stopSystems.Run(state);
	}

	public void Process(GameDate date) {
		runSystems.Run(state);
		renderSystems.Run(state);
		state.Tick();
	}
}
