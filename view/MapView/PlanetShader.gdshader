shader_type spatial;
render_mode diffuse_lambert;
// render_mode diffuse_toon, specular_toon, vertex_lighting;

uniform sampler2D terrainTexture;
uniform sampler2D normalTexture;
uniform float heightScale : hint_range(0, 5) = 1.0;

uniform sampler2D splatmap;
uniform sampler2D grass;
uniform sampler2D rock;
uniform sampler2D sand;

uniform float sandres = 1;
uniform float grassres = 1;
uniform float rockres = 1;

void vertex() {
	vec4 height = texture(terrainTexture, UV);
	float displacement = height.x / heightScale;
	VERTEX *= 1.0 + displacement;

	float texelSize = 1.0 / float(textureSize(terrainTexture, 0).x); 
    float n = texture(terrainTexture, UV + vec2(0, -texelSize)).x; 
    float s = texture(terrainTexture, UV + vec2(0, texelSize)).x; 
    float e = texture(terrainTexture, UV + vec2(-texelSize, 0)).x; 
    float w = texture(terrainTexture, UV + vec2(texelSize, 0)).x;
	vec3 ew = normalize(vec3(2.0 * texelSize, e - w, 0.0)); 
    vec3 ns = normalize(vec3(0.0, s - n, 2.0 * texelSize)); 
    NORMAL = cross(ew, ns);

	// NORMAL = normalize(VERTEX).xyz;
}

void fragment() {
	vec3 sandcolor;
	vec3 grasscolor;
	vec3 rockcolor;
	vec3 splatmapcolor;

	splatmapcolor = texture(splatmap, UV).rgb;

	sandcolor = texture(sand, UV * sandres).rgb * splatmapcolor.r;
	grasscolor = texture(grass, UV * grassres).rgb * splatmapcolor.g;
	rockcolor = texture(rock, UV * rockres).rgb * splatmapcolor.b;

	ALBEDO = sandcolor + grasscolor + rockcolor;
	// NORMAL_MAP = texture(normalTexture, UV).xyz;
}
