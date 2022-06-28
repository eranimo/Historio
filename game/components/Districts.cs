using System.Collections.Generic;

public static class District {
	public enum DistrictType {
		Camp,
		Village,
		Town,
		CityCenter,
		Neighborhood,
		Castle,
		Harbor,
	}

	public static Dictionary<DistrictType, string> spritePath = new Dictionary<DistrictType, string>() {
		{ DistrictType.Camp, "res://assets/sprites/districts/camp.tres" },
		{ DistrictType.Village, "res://assets/sprites/districts/village.tres" },
		{ DistrictType.Town, "res://assets/sprites/districts/town.tres" },
		{ DistrictType.CityCenter, "res://assets/sprites/districts/citycenter.tres" },
		{ DistrictType.Neighborhood, "res://assets/sprites/districts/neighborhood.tres" },
		{ DistrictType.Castle, "res://assets/sprites/districts/castle.tres" },
		{ DistrictType.Harbor, "res://assets/sprites/districts/harbor.tres" },
	};
}

public class DistrictData {
	public District.DistrictType type;
}
// relation to Country
public class DistrictOwner { }