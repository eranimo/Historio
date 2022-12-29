shader_type spatial;
render_mode diffuse_toon, specular_toon;

uniform sampler2D noiseTexture;
uniform float offset = 0.5;
uniform float scale = 1.0;

void vertex() {
	vec4 height = texture(noiseTexture, UV);
	float displacement = (offset + (height.x / scale));
	VERTEX = VERTEX + NORMAL * displacement;
	NORMAL = normalize(VERTEX).xyz;
}

void fragment() {
	// NORMAL_MAP = texture(noiseTexture, UV).xyz;
	// ALBEDO = texture(noiseTexture, UV).xyz;
}
