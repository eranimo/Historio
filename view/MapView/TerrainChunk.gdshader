shader_type spatial;
render_mode specular_toon;

uniform float height_scale = 0.5;
uniform sampler2D heightmap;
uniform vec2 chunkPosition;
uniform vec2 worldSize;
uniform vec2 chunkSize;

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

	float texelSize = 1.0 / float(textureSize(heightmap, 0).x); 
    float n = texture(heightmap, tex_position + vec2(0, -texelSize)).x; 
    float s = texture(heightmap, tex_position + vec2(0, texelSize)).x; 
    float e = texture(heightmap, tex_position + vec2(-texelSize, 0)).x; 
    float w = texture(heightmap, tex_position + vec2(texelSize, 0)).x;
    NORMAL.x = n - s;
	NORMAL.y = 2.0;
	NORMAL.z = e - w;
	NORMAL = normalize(NORMAL);
}

void fragment() {
	vec3 sandcolor;
	vec3 grasscolor;
	vec3 rockcolor;
	vec3 splatmapcolor;

	// NORMAL_MAP = texture(normalmap, tex_position).xyz;
	splatmapcolor = texture(splatmap, tex_position).rgb;

	sandcolor = texture(sand, UV * sandres).rgb * splatmapcolor.r;
	grasscolor = texture(grass, UV * grassres).rgb * splatmapcolor.g;
	rockcolor = texture(rock, UV * rockres).rgb * splatmapcolor.b;

	ALBEDO = sandcolor + grasscolor + rockcolor;
}