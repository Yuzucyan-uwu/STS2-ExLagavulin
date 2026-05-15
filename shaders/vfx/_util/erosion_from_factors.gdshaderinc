float erosion_from_factors(vec2 factors, float input)
{
	return smoothstep(factors.x, factors.x + factors.y, input);
}