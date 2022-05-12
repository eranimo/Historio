using System;
using LibNoise;

public class WorldNoise {
	public int width;
	public int height;
	public int octaves;
	public int frequency;
	public float amplitude;
	private LibNoise.Primitive.ImprovedPerlin noise;

	public WorldNoise(int width, int height, int seed, int octaves = 5, int frequency = 2, float amplitude = 0.5f) {
		this.width = width;
		this.height = height;
		this.octaves = octaves;
		this.frequency = frequency;
		this.amplitude = amplitude;
		this.noise = new LibNoise.Primitive.SimplexPerlin(seed, NoiseQuality.Best);
	}

	/// <summary>Gets a coordinate noise value projected onto a sphere</summary>
	public float Get(int x, int y) {
		var coordLong = ((x / (double) this.width) * 360) - 180;
		var coordLat = ((-y / (double) this.height) * 180) + 90;
		var inc = ((coordLat + 90.0) / 180.0) * Math.PI;
		var azi = ((coordLong + 180.0) / 360.0) * (2 * Math.PI);
		var nx = (float) (Math.Sin(inc) * Math.Cos(azi));
		var ny = (float) (Math.Sin(inc) * Math.Sin(azi));
		var nz = (float) (Math.Cos(inc));

		float amplitude = 1;
		float freq = 1;
		var v = 0f;
		for (var i = 0; i < this.octaves; i++) {
			v += this.noise.GetValue(nx * freq, ny * freq, nz * freq) * amplitude;
			amplitude *= this.amplitude;
			freq *= this.frequency;
		}

		v = (v + 1) / 2;
		return v;
	}
}
