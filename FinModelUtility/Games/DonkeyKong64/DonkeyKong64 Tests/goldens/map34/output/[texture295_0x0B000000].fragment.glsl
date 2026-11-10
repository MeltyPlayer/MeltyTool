#version 450
precision mediump float;


struct Texture {
  sampler2D sampler;
  vec2 clampMin;
  vec2 clampMax;
};


vec2 norm2denorm(sampler2D tex, vec2 uv) {
  return uv * vec2(textureSize(tex, 0)) - 0.5;
}

ivec2 denorm2idx(vec2 d_uv) {
  return ivec2(floor(d_uv));
}

ivec2 norm2idx(sampler2D tex, vec2 uv) {
  return denorm2idx(norm2denorm(tex, uv));
}

vec2 idx2norm(sampler2D tex, ivec2 idx) {
  vec2 denorm_uv = vec2(idx) + 0.5;
  vec2 size = vec2(textureSize(tex, 0));
  return denorm_uv / size;
}

vec4 texel_fetch(sampler2D tex, ivec2 idx) {
  vec2 uv = idx2norm(tex, idx);
  return texture(tex, uv);
}

#if 0
float find_mipmap_level(in vec2 texture_coordinate) // in texel units {
    vec2  dx_vtc        = dFdx(texture_coordinate);
    vec2  dy_vtc        = dFdy(texture_coordinate);
    float delta_max_sqr = max(dot(dx_vtc, dx_vtc), dot(dy_vtc, dy_vtc));
    float mml = 0.5 * log2(delta_max_sqr);
    return max( 0, mml ); // Thanks @Nims
}
#endif


/*
 * Unlike Nintendo's documentation, the N64 does not use
 * the 3 closest texels.
 * The texel grid is triangulated:
 *
 *     0 .. 1        0 .. 1
 *   0 +----+      0 +----+
 *     |   /|        |\   |
 *   . |  / |        | \  |
 *   . | /  |        |  \ |
 *     |/   |        |   \|
 *   1 +----+      1 +----+
 *
 * If the sampled point falls above the diagonal,
 * The top triangle is used; otherwise, it's the bottom.
 */

vec4 texture_3point(sampler2D tex, vec2 uv) {
    vec2 denorm_uv = norm2denorm(tex, uv);
    ivec2 idx_low = denorm2idx(denorm_uv);
    vec2 ratio = denorm_uv - vec2(idx_low);

#define FLIP_DIAGONAL

#ifndef FLIP_DIAGONAL
    // this uses one diagonal orientation
    #if 0
    // using conditional, might not be optimal
    bool lower_flag = 1.0 < ratio.s + ratio.t;
    ivec2 corner0 = lower_flag ? ivec2(1, 1) : ivec2(0, 0);
    #else
    // using step() function, might be faster
    int lower_flag = int(step(1.0, ratio.s + ratio.t));
    ivec2 corner0 = ivec2(lower_flag, lower_flag);
    #endif
    ivec2 corner1 = ivec2(0, 1);
    ivec2 corner2 = ivec2(1, 0);
#else
    // orient the triangulated mesh diagonals the other way
    #if 0
    bool lower_flag = ratio.s - ratio.t > 0.0;
    ivec2 corner0 = lower_flag ? ivec2(1, 0) : ivec2(0, 1);
    #else
    int lower_flag = int(step(0.0, ratio.s - ratio.t));
    ivec2 corner0 = ivec2(lower_flag, 1 - lower_flag);
    #endif
    ivec2 corner1 = ivec2(0, 0);
    ivec2 corner2 = ivec2(1, 1);
#endif
    ivec2 idx0 = idx_low + corner0;
    ivec2 idx1 = idx_low + corner1;
    ivec2 idx2 = idx_low + corner2;

    vec4 t0 = texel_fetch(tex, idx0);
    vec4 t1 = texel_fetch(tex, idx1);
    vec4 t2 = texel_fetch(tex, idx2);

    // This is standard (Crammer's rule) barycentric coordinates calculation.
    vec2 v0 = vec2(corner1 - corner0);
    vec2 v1 = vec2(corner2 - corner0);
    vec2 v2 = ratio   - vec2(corner0);
    float den = v0.x * v1.y - v1.x * v0.y;
    /*
     * Note: the abs() here is necessary because we don't guarantee
     * the proper order of vertices, so some signed areas are negative.
     * But since we only interpolate inside the triangle, the areas
     * are guaranteed to be positive, if we did the math more carefully.
     */
    float lambda1 = abs((v2.x * v1.y - v1.x * v2.y) / den);
    float lambda2 = abs((v0.x * v2.y - v2.x * v0.y) / den);
    float lambda0 = 1.0 - lambda1 - lambda2;

    return lambda0*t0 + lambda1*t1 + lambda2*t2;
}
uniform Texture texture0;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = texture_3point(texture0.sampler, clamp(uv0, texture0.clampMin, texture0.clampMax)).rgb*vertexColor0.rgb;

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, 1);

  if (!(alphaComponent > 0.01)) {
    discard;
  }
}