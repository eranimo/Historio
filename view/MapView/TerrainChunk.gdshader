shader_type spatial;
render_mode diffuse_lambert;

uniform float height_scale = 0.5;
uniform sampler2D heightmap;
uniform sampler2D normalmap;
uniform vec2 chunkPosition;
uniform vec2 worldSize;
uniform vec2 chunkSize;
uniform vec2 terrainSize;

varying vec2 tex_position;

uniform sampler2D splatmap;
uniform sampler2D grass;
uniform sampler2D rock;
uniform sampler2D sand;
uniform float sandres = 1;
uniform float grassres = 1;
uniform float rockres = 1;


void vertex() {
	vec2 position_in_chunk = VERTEX.xz / chunkSize + 0.5;
	vec2 inv_atlas_size = vec2(1.0, 1.0) / worldSize;
	vec2 uv_offset = chunkPosition * inv_atlas_size;
	vec2 uv_scale = chunkSize * inv_atlas_size;
	tex_position = position_in_chunk * uv_scale + uv_offset;

	float height = texture(heightmap, tex_position).x;
	VERTEX.y += height * height_scale;
}

void fragment() {
	vec3 sandcolor;
	vec3 grasscolor;
	vec3 rockcolor;
	vec3 splatmapcolor;

	NORMAL_MAP = texture(normalmap, tex_position).xyz;
	splatmapcolor = texture(splatmap, tex_position).rgb;

	sandcolor = texture(sand, UV * sandres).rgb * splatmapcolor.r;
	grasscolor = texture(grass, UV * grassres).rgb * splatmapcolor.g;
	rockcolor = texture(rock, UV * rockres).rgb * splatmapcolor.b;

	ALBEDO = sandcolor + grasscolor + rockcolor;
}