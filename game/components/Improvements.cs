using System.Collections.Generic;

public static class Improvement {
	public enum ImprovementType {
		Farm,
		LoggingCamp,
		Mine,
		Pasture,
	}

	public static Dictionary<ImprovementType, string> spritePath = new Dictionary<ImprovementType, string>() {
		{ ImprovementType.Farm, "res://assets/sprites/improvements/farm.tres" },
		{ ImprovementType.LoggingCamp, "res://assets/sprites/improvements/loggingcamp.tres" },
		{ ImprovementType.Mine, "res://assets/sprites/improvements/mine.tres" },
		{ ImprovementType.Pasture, "res://assets/sprites/improvements/pasture.tres" },
	};
}

public class ImprovementData {
	public Improvement.ImprovementType type;
}
// relation to Country
public class ImprovementOwner { }