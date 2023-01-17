shader_type spatial;
render_mode specular_toon;

uniform float height_scale = 0.5;

uniform sampler2D heightmap;

// chunk position in map units
uniform vec2 chunkPosition;

// position of hex in top left of chunk
uniform vec2 chunkOriginHexes;

// size of world in map units
uniform vec2 worldSize;

// size of chunk in map units
uniform vec2 chunkSize;

// size of world in hexes
uniform vec2 worldSizeHexes;

// size of chunk in map units
uniform vec2 chunkSizeHexes;

// height in map units of each hex
uniform sampler2D hexHeight;

uniform float hexSize;

uniform sampler2D splatmap;
uniform sampler2D grass;
uniform sampler2D rock;
uniform sampler2D sand;
uniform float sandres = 1;
uniform float grassres = 1;
uniform float rockres = 1;

varying vec2 tex_position;
varying vec2 hex_position;
varying float hex_height;
varying vec3 color;

#define DIR_SOUTHEAST 0
#define DIR_NORTHEAST 1
#define DIR_NORTH 2
#define DIR_NORTHWEST 3
#define DIR_SOUTHWEST 4
#define DIR_SOUTH 5

// odd-q layout
const ivec2 directions_even[] = {
	ivec2(+1,  0), ivec2(+1, -1), ivec2(0, -1),
	ivec2(-1, -1), ivec2(-1,  0), ivec2(0, +1)
};

const ivec2 directions_odd[] = {
	ivec2(+1, +1), ivec2(+1,  0), ivec2(0, -1),
	ivec2(-1,  0), ivec2(-1, +1), ivec2(0, +1)
};

const int direction_left[] = {
	// SE
	DIR_SOUTH,
	// NE
	DIR_SOUTHEAST,
	// N
	DIR_NORTHEAST,
	// NW
	DIR_NORTH,
	// SW
	DIR_NORTHWEST,
	// S
	DIR_SOUTHWEST
};

const int direction_right[] = {
	// SE
	DIR_NORTHEAST,
	// NE
	DIR_NORTH,
	// N
	DIR_NORTHWEST,
	// NW
	DIR_SOUTHWEST,
	// SW
	DIR_SOUTH,
	// S
	DIR_SOUTHEAST
};

const int direction_opposite[] = {
	// SE
	DIR_NORTHWEST,
	// NE
	DIR_SOUTHWEST,
	// N
	DIR_SOUTH,
	// NW
	DIR_SOUTHEAST,
	// SW
	DIR_NORTHEAST,
	// S
	DIR_NORTH
};

vec2 get_position(vec2 pos) {
	vec2 position_in_chunk = pos / chunkSize;
	vec2 inv_atlas_size = vec2(1.0, 1.0) / worldSize;
	vec2 uv_offset = chunkPosition * inv_atlas_size;
	vec2 uv_scale = chunkSize * inv_atlas_size;
	return position_in_chunk * uv_scale + uv_offset;
}

ivec2 get_neighbor(ivec2 hex, int direction) {
	int parity = hex.y & 1;
	ivec2 diff;
	if (parity == 0) {
		diff = directions_even[direction];
	} else if(parity == 1) {
		diff = directions_odd[direction];
	}
	return hex + diff;
}

float get_hex_height(ivec2 hex) {
	return texelFetch(hexHeight, hex, 0).x;
}

void vertex() {
	tex_position = get_position(VERTEX.xz);
	// hex_position = tex_position * worldSizeHexes;

	// float height = texture(heightmap, tex_position).x;

	// find height of current hex
	hex_position = round(chunkOriginHexes + UV2);
	ivec2 hex = ivec2(int(hex_position.x), int(hex_position.y));
	hex_height = get_hex_height(hex);

	// determine vertex height
	// VERTEX.y += hex_height * height_scale;

	// get the direction this vertex is facing
	int dir = int(round(CUSTOM0.a * 5.0));
	vec3 vertex_pos_in_face = CUSTOM0.rgb;
	ivec2 neighbor = get_neighbor(hex, dir);
	ivec2 neighbor_left = get_neighbor(hex, direction_left[dir]);
	ivec2 neighbor_right = get_neighbor(hex, direction_right[dir]);
	float hex_height_neighbor = get_hex_height(neighbor);
	vec3 face_heights = vec3(
		(hex_height + hex_height_neighbor + get_hex_height(neighbor_right)) / 3.0,
		(hex_height + hex_height_neighbor + get_hex_height(neighbor_left)) / 3.0,
		hex_height
	);
	float final_height = (face_heights.x * vertex_pos_in_face.x) + (face_heights.y * vertex_pos_in_face.y) + (face_heights.z * vertex_pos_in_face.z);

	VERTEX.y += final_height * height_scale;

	color = vec3(hex_height, hex_height, hex_height);
	// color = vec3(final_height, final_height, final_height);
	// color = vertex_pos_in_face;

	// generate normals
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

	// ALBEDO = sandcolor + grasscolor + rockcolor;

	// if (hex_position.x < 0.0 || hex_position.y < 0.0 || hex_position.x >= worldSizeHexes.x || hex_position.y >= worldSizeHexes.y) {
	// 	color = vec3(0.0, 0.0, 0.0);
	// } else {
	// 	color = vec3(hex_height, hex_height, hex_height);
	// }
	// ALBEDO = color.rgb;

	ALBEDO = color;
}