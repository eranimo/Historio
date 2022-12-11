using Godot;

public struct SphericalCoordinate {
	public float Radius;
	public float Polar;
	public float Elevation;

	public SphericalCoordinate(float radius, float polar, float elevation) {
		this.Radius = radius;
		this.Polar = polar;
		this.Elevation = elevation;
	}

	public Vector2 ToLatLong() {
		return new Vector2(Polar, Elevation);
	}
}

public static class CoordinateConversion {
	public static Vector3 SphericalToCartesian(SphericalCoordinate spherical) {
		float a = spherical.Radius * Mathf.Cos(spherical.Elevation);
		var outCart = new Vector3();
		outCart.x = a * Mathf.Cos(spherical.Polar);
		outCart.y = spherical.Radius * Mathf.Sin(spherical.Elevation);
		outCart.z = a * Mathf.Sin(spherical.Polar);
		return outCart;
	}

	public static SphericalCoordinate CartesianToSpherical(Vector3 cartCoords) {
		if (cartCoords.x == 0) {
			cartCoords.x = Mathf.Epsilon;
		}
		var outRadius = Mathf.Sqrt((cartCoords.x * cartCoords.x)
			+ (cartCoords.y * cartCoords.y)
			+ (cartCoords.z * cartCoords.z));
		var outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
		if (cartCoords.x < 0) {
			outPolar += Mathf.Pi;
		}
		var outElevation = Mathf.Asin(cartCoords.y / outRadius);
		return new SphericalCoordinate(outRadius, outPolar, outElevation);
	}
}