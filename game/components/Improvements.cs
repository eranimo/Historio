using System.Collections.Generic;

public enum ImprovementType {
	Farm,
	LoggingCamp,
	Mine,
	Pasture,
}

public static class ImprovementConstants {
	public static Dictionary<ImprovementType, string> title = new Dictionary<ImprovementType, string>() {
		{ ImprovementType.Farm, "Farm" },
		{ ImprovementType.LoggingCamp, "Logging Camp" },
		{ ImprovementType.Mine, "Mine" },
		{ ImprovementType.Pasture, "Pasture" },
	};

	public static Dictionary<ImprovementType, string> spritePath = new Dictionary<ImprovementType, string>() {
		{ ImprovementType.Farm, "res://assets/sprites/improvements/farm.tres" },
		{ ImprovementType.LoggingCamp, "res://assets/sprites/improvements/loggingcamp.tres" },
		{ ImprovementType.Mine, "res://assets/sprites/improvements/mine.tres" },
		{ ImprovementType.Pasture, "res://assets/sprites/improvements/pasture.tres" },
	};
}

public static class ImprovementTypeExtensions {
	public static string getSpritePath(this ImprovementType type) => ImprovementConstants.spritePath[type];
	public static string getTitle(this ImprovementType type) => ImprovementConstants.title[type];
}

public class ImprovementData {
	public ImprovementType type;
}
// relation to Country
public class ImprovementOwner { }