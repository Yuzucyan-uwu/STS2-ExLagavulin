shader_type canvas_item;
render_mode blend_mix;

#include "res://shaders/vfx/_util/erosion_from_factors.gdshaderinc"

uniform sampler2D animation_texture : repeat_enable;

uniform vec4 outline_color : source_color;

uniform float distortion_strength;
uniform sampler2D distortion_curve;
uniform vec2 distortion_speed;

uniform sampler2D erosion_curve;
uniform float erosion_offset = 0.25;

varying float lifetime;
varying float flipbook_offset;
varying vec4 vertex_color;

void vertex() {
	vertex_color = COLOR;
	lifetime = INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w;
	flipbook_offset = INSTANCE_CUSTOM.z;
}

void fragment() {
	vec4 distortion_texture = texture(animation_texture, UV + vec2(flipbook_offset) + (TIME * distortion_speed));
	float distortion_curve_val = texture(distortion_curve, vec2(lifetime)).r;
	vec2 distortion = (distortion_texture.bb - 0.5) * 2.0 * distortion_strength * distortion_curve_val;

	vec4 tex = texture(TEXTURE, UV + distortion);

	vec4 erosion_texture = texture(animation_texture, UV + vec2(flipbook_offset));

	float erosion_curve_val = texture(erosion_curve, vec2(lifetime)).r;
	float erosion_val = erosion_from_factors(vec2(erosion_curve_val, erosion_offset), erosion_texture.g);

	vec4 final_color = mix(outline_color, vertex_color, tex.r);
	final_color.a *= erosion_val * tex.a;

	COLOR = final_color;
}
