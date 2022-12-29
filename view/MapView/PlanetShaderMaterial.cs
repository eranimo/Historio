using Godot;


[Tool]
public partial class PlanetShaderMaterial : ShaderMaterial {
	public PlanetShaderMaterial() {
	}

	public void Generate(int width, int height, int seed) {
		GD.PrintS("\tCreating Planet with size", width, height);
		var image = Image.Create(width, height, false, Image.Format.Rgb8);
		var worldNoise = new WorldNoise(width, height, seed);
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				var h = worldNoise.Get(x, y);
				image.SetPixel(x, y, new Color(h, h, h));
			}
		}
		var texture = ImageTexture.CreateFromImage(image);
		SetShaderParameter("noiseTexture", texture);
	}
} 