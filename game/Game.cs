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
	public int Seed = 1234;
	public WorldOptions world = new WorldOptions();
}

public interface IGeneratorStep {
	void Generate(GameOptions options, GameManager manager);
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

	public SavedGameMetadata savedGame { get; set; }

	public delegate void GameLoaded(SavedGameEntry entry);
	public event GameLoaded OnGameLoaded;

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

	public RelEcs.World state => manager.state;

	public IObservable<GameDate> GameDateChanged { get => gameDateChanged; }
	public IObservable<GameSpeed> Speed { get => speed; }

	public void Init() {
		manager.Init();
	}

	public void Process(float delta, bool force = false) {
		manager.Process(delta);
		if (!force && !IsPlaying) {
			return;
		}

		manager.ProcessPlaying(delta);

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

	public void Slower() {
		if (speed.Value == GameSpeed.Normal) {
			speed.OnNext(GameSpeed.Slow);
		} else if (speed.Value == GameSpeed.Fast) {
			speed.OnNext(GameSpeed.Normal);
		}
	}

	public void Faster() {
		if (speed.Value == GameSpeed.Slow) {
			speed.OnNext(GameSpeed.Normal);
		} else if (speed.Value == GameSpeed.Normal) {
			speed.OnNext(GameSpeed.Fast);
		}
	}

	public void ToggleSpeed() {
		if (speed.Value == GameSpeed.Slow) {
			speed.OnNext(GameSpeed.Normal);
		} else if (speed.Value == GameSpeed.Normal) {
			speed.OnNext(GameSpeed.Fast);
		} else if (speed.Value == GameSpeed.Fast) {
			speed.OnNext(GameSpeed.Slow);
		}
	}

	public void HandleGameLoaded(SavedGameEntry entry) {
		OnGameLoaded?.Invoke(entry);
	}

}
