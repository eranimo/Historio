shader_type spatial;
render_mode specular_toon;

uniform float height_scale = 0.5;

uniform sampler2D heightmap;

// chunk position in map units
uniform vec2 chunkPosition;

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

uniform sampler2D splatmap;
uniform sampler2D grass;
uniform sampler2D rock;
uniform sampler2D sand;
uniform float sandres = 1;
uniform float grassres = 1;
uniform float rockres = 1;


const float SQRT_2 = 1.4142135623730951;
const float SQRT_3 = 1.73205080757;

// orientation
const float or_f0 = 3.0 / 2.0;
const float or_f1 = 0.0;
const float or_f2 = SQRT_3 / 2.0;
const float or_f3 = SQRT_3;
const float or_b0 = 2.0 / 3.0;
const float or_b1 = 0.0;
const float or_b2 = -1.0 / 3.0;
const float or_b3 = SQRT_3 / 3.0;
const float or_start_angle = 0.0;

const float layout_origin_x = 6.5;
const float layout_origin_y = 8.5;
const float layout_size_x = 1.0;
const float layout_size_y = 1.0;
 
// Function to convert cubial coords to axial coords.
vec2 cubeToAxial(vec3 cube) {
	return vec2(cube.x, cube.y);
}
 
// Function to convert axial coords to cubial coords.
vec3 axialToCube(vec2 axial) {
	float x = axial.x;
	float y = axial.y;
	float z = -x - y;
	return vec3(x, y, z);
}

vec2 axialToOffset(vec2 hex) {
	float col = hex.x;
	float row = hex.y + (hex.x - (float(int(hex.x) & 1))) / 2.0;
	return vec2(col, row);
}

vec2 offsetToAxial(vec2 hex) {
	float q = hex.y;
	float r = hex.x - (hex.y - (float(int(hex.y) & 1))) / 2.0;
	return vec2(q, r);
}
 
// Function to round float cubial coords to int cubial coords.
vec3 cubeRound(vec3 cube) {
	float rx = round(cube.x);
	float ry = round(cube.y);
	float rz = round(cube.z);
 
	float xDiff = abs(rx - cube.x);
	float yDiff = abs(ry - cube.y);
	float zDiff = abs(rz - cube.z);
 
	if ((xDiff > yDiff) && (xDiff > zDiff))
		rx = -ry - rz;
	else if (yDiff > zDiff)
		ry = -rx - rz;
	else
		rz = -rx - ry;

	return vec3(rx, ry, rz);
}
 
// Function to round float axial coords to int axial coords.
vec2 axialRound(vec2 axial) {
	return cubeToAxial(cubeRound(axialToCube(axial)));
}
 
// Function to return axial hex-grid coords, given a screen position (vertical, flat-top hex layout).
vec2 pixelToHex(vec2 point) {
	vec2 pt = vec2((point.x - layout_origin_x) / layout_size_x, (point.y - layout_origin_y) / layout_size_y);
	float q = or_b0 * pt.x + or_b1 * pt.y;
	float r = or_b2 * pt.x + or_b3 * pt.y;
	float s = -q - r;
	return cubeToAxial(cubeRound(vec3(q, r, s)));
}
 
// Function to return a screen position, given axial hex-grid coords (vertical, flat-top hex layout).
vec2 hexToPixel(vec2 hex) {
	float x = (or_f0 * hex.x + or_f1 * hex.y) * layout_size_x;
	float y = (or_f2 * hex.x + or_f3 * hex.y) * layout_size_y;
	return vec2(x + layout_origin_x, y + layout_origin_y);
}


varying vec2 tex_position;
varying vec2 hex_position;

void vertex() {
	vec2 position_in_chunk = VERTEX.xz / chunkSize;
	vec2 inv_atlas_size = vec2(1.0, 1.0) / worldSize;
	vec2 uv_offset = chunkPosition * inv_atlas_size;
	vec2 uv_scale = chunkSize * inv_atlas_size;
	tex_position = position_in_chunk * uv_scale + uv_offset;
	hex_position = tex_position * worldSizeHexes;

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

	// ALBEDO = sandcolor + grasscolor + rockcolor;

	vec2 hex = axialRound(pixelToHex(hex_position));
	vec2 offset = axialToOffset(hex);
 
	vec4 color;
	if (offset.x < 0.0 || offset.y < 0.0 || offset.x >= worldSizeHexes.x || offset.y >= worldSizeHexes.y) {
		color = vec4(0.0, 0.0, 0.0, 0.0);
	} else {
		color = texelFetch(hexHeight, ivec2(int(offset.x), int(offset.y)), 0);
	}

	ALBEDO = color.rgb;
}