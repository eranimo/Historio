shader_type spatial;
render_mode diffuse_toon;

uniform sampler2D noiseTexture;
uniform float offset = 0.5;
uniform float scale = 1.0;

void vertex() {
	vec4 height = texture(noiseTexture, UV);
	float displacement = (offset + (height.x / scale));
	VERTEX = VERTEX + NORMAL * displacement;
}

void fragment() {
	ALBEDO = texture(noiseTexture, UV).xyz;
}
