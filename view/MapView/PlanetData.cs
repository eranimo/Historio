using Godot;
using LibNoise.Primitive;


public partial class NoiseGen {
	public int octaves;
	public float frequency;
	public float amplitude;
	private LibNoise.Primitive.ImprovedPerlin noise;

	public NoiseGen(int seed, int octaves = 5, float frequency = 1f, float amplitude = 0.05f) {
		this.octaves = octaves;
		this.frequency = frequency;
		this.amplitude = amplitude;
		this.noise = new LibNoise.Primitive.SimplexPerlin(seed, LibNoise.NoiseQuality.Best);
	}

	public float Get(float x, float y, float z) {
		float amplitude = 1;
		float freq = 1;
		var v = 0f;
		for (var i = 0; i < this.octaves; i++) {
			v += this.noise.GetValue(x * freq, y * freq, z * freq) * amplitude;
			amplitude *= this.amplitude;
			freq *= this.frequency;
		}

		v = (v + 1.0f) / 2.0f;
		return v;
	}
}

public class PlanetData {
	private readonly int seed;
	private readonly int width;
	private readonly int height;
	private NoiseGen heightNoise;

	public PlanetData(int seed, int width, int height) {
		this.seed = seed;
		this.width = width;
		this.height = height;
		this.heightNoise = new NoiseGen(seed);
	}

	public Vector3 GetPoint(Vector3 pointOnSphere) {
		float radius = 1.0f;
		float f = 1f;
		var elevation = heightNoise.Get(pointOnSphere.x * f, pointOnSphere.y * f, pointOnSphere.z * f);
		elevation = Mathf.Clamp(elevation, 0.01f, 1f);
		return pointOnSphere * radius * (elevation - 1.0f);
	}
}