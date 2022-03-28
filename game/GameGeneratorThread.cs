using System;
using Godot;

public delegate void GeneratorProgress(string label, int value);
public delegate void GameGeneratedCallback();

public class GameGeneratorThread {
	private readonly GameOptions options;
	private GeneratorProgress progress;
	private GameGeneratedCallback callback;

	public Game game;
	private Random random = new Random();

	public GameGeneratorThread(
		GameOptions options,
		GeneratorProgress progress,
		GameGeneratedCallback callback
	) {
		this.options = options;
		this.progress = progress;
		this.callback = callback;
	}

	public void Generate() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		
		var gameGen = new GameGenerator(options, game.manager);
		gameGen.progress.Subscribe(i => progress(i.Item1, i.Item2));
		gameGen.Generate();

		GD.PrintS($"GameGeneratorThread: {watch.ElapsedMilliseconds}ms");
		if (callback != null) {
			callback();
		}
	}
}