public static class PopConstants {
	public static Dictionary<PopProfession, string> title = new Dictionary<PopProfession, string>() {
		{ PopProfession.Slaves, "Slaves" },
		{ PopProfession.Laborers, "Laborers" },
		{ PopProfession.Artisans, "Artisans" },
		{ PopProfession.Aristocrats, "Aristocrats" },
	};
}

public enum PopProfession {
	Slaves,
	Laborers,
	Artisans,
	Aristocrats
}

// relation on Pop to Tile
public class PopTile {}

public class PopData {
	public int size;
	public PopProfession profession;

	public float laborPower => size;
}