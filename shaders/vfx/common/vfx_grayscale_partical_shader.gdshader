shader_type canvas_item;
render_mode blend_mix;

uniform vec2 pivot_offset = vec2(0.0, 0.0);

uniform sampler2D lut;

varying vec4 vertex_color;

void vertex() {
	vertex_color = COLOR;
	VERTEX += pivot_offset * (1.0/TEXTURE_PIXEL_SIZE);
}

void fragment() {
	vec4 grayscale = texture(TEXTURE, UV);

	vec4 lut_tex = texture(lut, vec2(grayscale.r));
	vec4 color_sub = lut_tex * vertex_color;

	COLOR = vec4(color_sub.rgb, vertex_color.a * grayscale.r);
}