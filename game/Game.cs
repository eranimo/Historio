global using RelEcs;
global using System.Collections.Generic;
using System;
using System.Reactive.Subjects;

public enum GameSpeed {
	Slow,
	Normal,
	Fast,
}

public class GameOptions {
	public int Seed = 12345;
	public WorldOptions world = new WorldOptions();
}

public interface IGeneratorStep {
	void Generate(GameOptions options, GameManager manager);
}

public class GameGenerator {
	public GameOptions options;
	private GameManager manager;
	public Subject<(string, int)> progress = new Subject<(string, int)>();

	public GameGenerator(
		GameOptions gameOptions,
		GameManager manager
	) {
		this.options = gameOptions;
		this.manager = manager;
	}

	public void Generate() {
		progress.OnNext(("Generating world", 0));
		new WorldGenerator().Generate(options, manager);
		progress.OnNext(("Generating polities", 50));
		new PolityGenerator().Generate(options, manager);
		progress.OnNext(("Finished world generation", 100));
	}
}

/*
- Handles play state, speed, and game date
- contains GameManager
*/
public class Game {
	public static readonly int TICKS_PER_DAY = 5;
	private BehaviorSubject<bool> playState = new BehaviorSubject<bool>(false);
	private BehaviorSubject<GameSpeed> speed = new BehaviorSubject<GameSpeed>(GameSpeed.Slow);

	private Subject<GameDate> gameDateChanged = new Subject<GameDate>();
	public GameDate date;
	public readonly GameManager manager;
	public int ticksLeftInDay = 0;

	public Game() {
		this.date = new GameDate(0);
		this.manager = new GameManager();
		manager.state.AddElement<Game>(this);
	}

	public IObservable<bool> PlayState { get => playState; }
	public bool IsPlaying { get => playState.Value; }
	public void Play() {
		playState.OnNext(true);
	}
	public void Pause() {
		playState.OnNext(false);
	}

	public RelEcs.World GameState => manager.state;

	public IObservable<GameDate> GameDateChanged { get => gameDateChanged; }
	public IObservable<GameSpeed> Speed { get => speed; }

	public void Process(float delta, bool force = false) {
		manager.UIProcess(delta);
		if (!force && !IsPlaying) {
			return;
		}

		manager.Process(delta);

		if (this.ticksLeftInDay == 0) {
			int ticksLeft = this.speedTicks;
			this.ProcessDay();
			this.ticksLeftInDay = ticksLeft;
		} else {
			this.ticksLeftInDay--;
		}
	}

	public void ProcessDay() {
		date.NextDay();
		gameDateChanged.OnNext(date);
		manager.ProcessDay();
	}

	public int speedTicks {
		get {
			switch (this.speed.Value) {
				case GameSpeed.Slow: return 4 * Game.TICKS_PER_DAY;
				case GameSpeed.Normal: return 2 * Game.TICKS_PER_DAY;
				case GameSpeed.Fast: return 1 * Game.TICKS_PER_DAY;
				default: throw new Exception("Unknown Speed");
			}
		}
	}

	public bool isLastDayTick {
		get {
			return dayTicks == (speedTicks - 1);
		}
	}

	public bool isFirstDayTick {
		get { return dayTicks == 1; }
	}

	public float dayTicks {
		get { return ((float) speedTicks - this.ticksLeftInDay); }
	}

	public float percentInDay {
		get { return this.dayTicks / ((float) speedTicks); }
	}

	public void Start() {
		manager.Start(this.date);
	}

}
