shader_type spatial;
render_mode specular_toon;

uniform float height_scale = 20.0;
uniform float terrain_noise_size = 5.0;

// uniform sampler2D heightmap;

group_uniforms WorldParameters;
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

// (level, biome, terrain, [unused])
uniform sampler2D hexData;

uniform float hexSize;
group_uniforms;

group_uniforms Colors;
uniform vec3 color_ocean : source_color;
uniform vec3 color_coast : source_color;
uniform vec3 color_temperate : source_color;
uniform vec3 color_desert : source_color;
uniform vec3 color_arctic : source_color;
uniform vec3 color_freshwater : source_color;
uniform vec3 color_tropical : source_color;
group_uniforms;

group_uniforms TerrainTypeNoiseScale;
uniform float terrain_noise_scale_plains = 100.0;
uniform float terrain_noise_scale_hills = 500.0;
uniform float terrain_noise_scale_mountains = 1000.0;
group_uniforms;

varying vec2 tex_position;
varying flat ivec2 current_hex;
varying vec3 color;

#define DIR_SOUTHEAST 0
#define DIR_NORTHEAST 1
#define DIR_NORTH 2
#define DIR_NORTHWEST 3
#define DIR_SOUTHWEST 4
#define DIR_SOUTH 5

#define BIOME_OCEAN 0
#define BIOME_COAST 1
#define BIOME_TEMPERATE 2
#define BIOME_DESERT 3
#define BIOME_ARCTIC 4
#define BIOME_FRESHWATER 5
#define BIOME_TROPICAL 6

#define TERRAIN_PLAINS 0
#define TERRAIN_HILLS 1
#define TERRAIN_MOUNTAINS 2

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
	int parity = hex.x & 1;
	ivec2 diff;
	if (parity == 0) {
		diff = directions_even[direction];
	} else if(parity == 1) {
		diff = directions_odd[direction];
	}
	return hex + diff;
}

float get_hex_height(ivec2 hex) {
	return (texelFetch(hexData, hex, 0).r * 255.0) / height_scale;
}

int get_hex_biome(ivec2 hex) {
	return int(texelFetch(hexData, hex, 0).g * 255.0);
}

int get_terrain_type(ivec2 hex) {
	return int(texelFetch(hexData, hex, 0).b * 255.0);
}

vec3 get_biome_color(int biome) {
	if (biome == BIOME_OCEAN) {
		return color_ocean;
	} else if (biome == BIOME_COAST) {
		return color_coast;
	} else if (biome == BIOME_TEMPERATE) {
		return color_temperate;
	} else if (biome == BIOME_DESERT) {
		return color_desert;
	} else if (biome == BIOME_ARCTIC) {
		return color_arctic;
	} else if (biome == BIOME_FRESHWATER) {
		return color_freshwater;
	} else if (biome == BIOME_TROPICAL) {
		return color_tropical;
	}
}

vec2 random(vec2 uv){
    uv = vec2( dot(uv, vec2(127.1,311.7) ),
               dot(uv, vec2(269.5,183.3) ) );
    return -1.0 + 2.0 * fract(sin(uv) * 43758.5453123);
}

float noise(vec2 uv) {
    vec2 uv_index = floor(uv);
    vec2 uv_fract = fract(uv);

    vec2 blur = smoothstep(0.0, 1.0, uv_fract);

    return mix( mix( dot( random(uv_index + vec2(0.0,0.0) ), uv_fract - vec2(0.0,0.0) ),
                     dot( random(uv_index + vec2(1.0,0.0) ), uv_fract - vec2(1.0,0.0) ), blur.x),
                mix( dot( random(uv_index + vec2(0.0,1.0) ), uv_fract - vec2(0.0,1.0) ),
                     dot( random(uv_index + vec2(1.0,1.0) ), uv_fract - vec2(1.0,1.0) ), blur.x), blur.y) + 0.5;
}

float get_terrain_noise_height(ivec2 hex, vec2 pos) {
	int terrain_type = get_terrain_type(hex);
	float value = 0.0;
	vec2 loc = (pos / hexSize);
	if (terrain_type == TERRAIN_PLAINS) {
		return noise(loc * terrain_noise_scale_plains);
	} else if (terrain_type == TERRAIN_HILLS) {
		return noise(loc * terrain_noise_scale_hills);
	} else if (terrain_type == TERRAIN_MOUNTAINS) {
		return noise(loc * terrain_noise_scale_mountains);
	}
}

void vertex() {
	tex_position = get_position(VERTEX.xz);

	// find height of current hex
	current_hex = ivec2(round(chunkOriginHexes + UV2));
	float hex_height = get_hex_height(current_hex);

	// get the direction this vertex is facing
	int dir = int(round(CUSTOM0.a * 5.0));
	vec3 vertex_pos_in_face = CUSTOM0.rgb;
	float center_dist = COLOR.r; // distance to center
	vec3 face_weight = vertex_pos_in_face;
	ivec2 neighbor = get_neighbor(current_hex, dir);
	ivec2 neighbor_left = get_neighbor(current_hex, direction_left[dir]);
	ivec2 neighbor_right = get_neighbor(current_hex, direction_right[dir]);
	float hex_height_neighbor = get_hex_height(neighbor);
	int triangle_id = int(round(CUSTOM1.r * 6.0));
	vec3 face_heights = vec3(1.0, 1.0, 1.0);

	if (triangle_id == 1 || triangle_id == 2 || triangle_id == 3 || triangle_id == 4) {
		face_heights = vec3(
			(hex_height + hex_height_neighbor + get_hex_height(neighbor_right)) / 3.0,
			(hex_height + hex_height_neighbor) / 2.0,
			hex_height
		);
	} else if (triangle_id == 5 || triangle_id == 6 || triangle_id == 7 || triangle_id == 8) {
		face_heights = vec3(
			(hex_height + hex_height_neighbor + get_hex_height(neighbor_left)) / 3.0,
			(hex_height + hex_height_neighbor) / 2.0,
			hex_height
		);
	}
	float final_height = (face_heights.x * face_weight.x) + (face_heights.y * face_weight.y) + (face_heights.z * face_weight.z);

	// VERTEX.y += final_height * height_scale;

	// get terrain noise height
	float terrain_noise = ((get_terrain_noise_height(current_hex, tex_position) * 2.0) - 1.0) * face_weight.z;
	float terrain_noise_height = terrain_noise_size * terrain_noise;
	VERTEX.y += (final_height * height_scale) + terrain_noise_height;
	// VERTEX.y = terrain_noise_height;
	// VERTEX.y = terrain_noise;

	// decide color

	int biome = get_hex_biome(current_hex);
	vec3 hex_biome_color = get_biome_color(biome);
	vec3 hex_biome_color_neighbor = get_biome_color(get_hex_biome(neighbor));
	vec3 hex_biome_color_neighbor_left = get_biome_color(get_hex_biome(neighbor_left));
	vec3 hex_biome_color_neighbor_right = get_biome_color(get_hex_biome(neighbor_right));

	vec3 blended_color = vec3(1.0, 1.0, 1.0);
	if (triangle_id == 1 || triangle_id == 2 || triangle_id == 3 || triangle_id == 4) {
		vec3 A = (hex_biome_color + hex_biome_color_neighbor + hex_biome_color_neighbor_right) / 3.0;
		vec3 B = (hex_biome_color + hex_biome_color_neighbor) / 2.0;
		vec3 C = hex_biome_color;
		blended_color = (A * face_weight.x) + (B * face_weight.y) + (C * face_weight.z);
	} else if (triangle_id == 5 || triangle_id == 6 || triangle_id == 7 || triangle_id == 8) {
		vec3 A = (hex_biome_color + hex_biome_color_neighbor + hex_biome_color_neighbor_left) / 3.0;
		vec3 B = (hex_biome_color + hex_biome_color_neighbor) / 2.0;
		vec3 C = hex_biome_color;
		blended_color = (A * face_weight.x) + (B * face_weight.y) + (C * face_weight.z);
	}

	color = blended_color;

	// color = vec3(hex_height, hex_height, hex_height);
	// color = vec3(final_height, final_height, final_height);
	// color = vertex_pos_in_face;
	// color = vec3(terrain_noise, terrain_noise, terrain_noise);

	// generate normals
	// float texelSize = 1.0 / float(textureSize(heightmap, 0).x); 
    // float n = texture(heightmap, tex_position + vec2(0, -texelSize)).x; 
    // float s = texture(heightmap, tex_position + vec2(0, texelSize)).x; 
    // float e = texture(heightmap, tex_position + vec2(-texelSize, 0)).x; 
    // float w = texture(heightmap, tex_position + vec2(texelSize, 0)).x;
    // NORMAL.x = n - s;
	// NORMAL.y = 2.0;
	// NORMAL.z = e - w;
	// NORMAL = normalize(NORMAL);
	NORMAL = vec3(0.0, final_height, 0.0);
	// TODO: calculate normals using a compute shader
}

void fragment() {
	vec3 sandcolor;
	vec3 grasscolor;
	vec3 rockcolor;
	vec3 splatmapcolor;

	// if (hex_position.x < 0.0 || hex_position.y < 0.0 || hex_position.x >= worldSizeHexes.x || hex_position.y >= worldSizeHexes.y) {
	// 	color = vec3(0.0, 0.0, 0.0);
	// } else {
	// 	color = vec3(hex_height, hex_height, hex_height);
	// }
	// ALBEDO = color.rgb;

	ALBEDO = color;
}