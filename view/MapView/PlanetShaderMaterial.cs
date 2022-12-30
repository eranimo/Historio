using Godot;


[Tool]
public partial class PlanetShaderMaterial : ShaderMaterial {
	private WorldNoise worldNoise;

	public PlanetShaderMaterial() {
	}

	public void GenerateTerrain(int width, int height, int seed, float seaLevel) {
		GD.PrintS("\tCreating Planet with size", width, height);
		worldNoise = new WorldNoise(width, height, seed);

		// create terrain texture
		var terrainImage = Image.Create(width, height, false, Image.Format.Rgb8);
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				var h = worldNoise.Get(x, y);
				terrainImage.SetPixel(x, y, new Color(h, h, h));
			}
		}
		var terrainTexture = ImageTexture.CreateFromImage(terrainImage);
		SetShaderParameter("terrainTexture", terrainTexture);

		// make normal map texture
		var normalImage = Image.Create(width, height, false, Image.Format.Rgb8);
		var normalTexture = ImageTexture.CreateFromImage(normalImage);
		SetShaderParameter("normalTexture", normalTexture);
	}

	public void GenerateTexture(int width, int height, int seed, float seaLevel) {
		// create splatmap
		var COLOR_OCEAN = new Color(1f, 0f, 0f);
		var COLOR_LAND = new Color(0f, 1f, 0f);
		var image = Image.Create(width, height, false, Image.Format.Rgb8);
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				var h = worldNoise.Get(x, y);
				if (h < seaLevel) {
					image.SetPixel(x, y, COLOR_OCEAN);
				} else {
					image.SetPixel(x, y, COLOR_LAND);
				}
			}
		}
		var splatmapTexture = ImageTexture.CreateFromImage(image);
		SetShaderParameter("splatmap", splatmapTexture);
	}
} 