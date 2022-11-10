using Godot;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

class NameFactory {
	private int seed;
	private Random rng;

	private Dictionary<char, List<char>> markov = new Dictionary<char, List<char>>();
	private HashSet<char> possibleFirstLetters = new HashSet<char>(); 

	public NameFactory(string name) {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		this.seed = 0;
		rng = new Random(seed);

		var file = FileAccess.Open($"res://assets/namelists/{name}.txt", FileAccess.ModeFlags.Read);
		while (!file.EofReached()) {
			addName(file.GetLine());
		}
		// GD.PrintS($"NameGenerator init: {watch.ElapsedMilliseconds}ms");
	}

	private void addName(string name) {
		possibleFirstLetters.Add(Char.ToLower(name[0]));
		for (int i = 0; i < name.Length; i++) {
			char letter = Char.ToLower(name[i]);
			char nextLetter;
			if (i == (name.Length - 1)) {
				nextLetter = '.';
			} else {
				nextLetter = Char.ToLower(name[i+1]); 
			}
			if (!markov.ContainsKey(letter)) {
				markov[letter] = new List<char>();
			}
			markov[letter].Add(nextLetter);
		}
	}

	public int Seed {
		get => seed;
		set {
			seed = value;
			rng = new Random(seed);
		}
	}

	public string GetName(int minLength = 3, int maxLength = 7) {
		int count = 1;
		var lastChar = possibleFirstLetters.ElementAt(rng.Next(possibleFirstLetters.Count));
		string name = lastChar.ToString().Capitalize();

		while (count < maxLength) {
			var nextLetter = getNextLetter(lastChar);
			if (nextLetter == null) {
				return GetName(minLength, maxLength);
			}
			if (nextLetter == '.') {
				if (count > minLength) {
					return name;
				}
			} else {
				name += (char) nextLetter;
				lastChar = (char) nextLetter;
				count++;
			}
		}
		return name;
	}

	private char? getNextLetter(char letter) {
		if (!markov.ContainsKey(letter)) {
			return null;
		}
		return markov[letter].ElementAt(rng.Next(markov[letter].Count));
	}
}