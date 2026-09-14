// Shared sky, sun and fog. Included by both the sky and lit passes so fogged geometry and the sky
// behind it cannot drift apart. Linear radiance throughout; tone mapping happens at composite.

uniform vec3 sunDirection;      // normalized, pointing from the scene toward the sun
uniform vec3 sunColor;          // linear radiance of the solar disc
uniform float sunAngularRadius; // radians. The real sun is 0.00465, widened a little for looks
uniform float sunGlowFalloff;   // higher concentrates the halo nearer the disc

uniform vec3 skyZenithColor;
uniform vec3 skyHorizonColor;
uniform vec3 groundColor;

uniform float fogDensity;
uniform float fogHeightFalloff;
uniform float fogHeight;

const float Pi = 3.14159265359;

// Sky dome only, no ground. Uses rayDir.y directly rather than remapping to [0,1]: remapping puts
// the horizon halfway up the gradient and tints distant haze with zenith blue.
vec3 SkyGradient(vec3 rayDir)
{
    float elevation = pow(clamp(rayDir.y, 0.0, 1.0), 0.45);
    vec3 sky = mix(skyHorizonColor, skyZenithColor, elevation);

    // Forward scattering: the sky brightens toward the sun across a wide angle.
    float cosAngle = clamp(dot(rayDir, sunDirection), -1.0, 1.0);
    float halo = pow(max(cosAngle, 0.0), sunGlowFalloff);

    return sky + (sunColor * halo * 0.10);
}

// Sky radiance along a view ray, excluding the solar disc itself. Below the horizon the ray would
// meet terrain, so it fades toward the ground tint.
vec3 SkyRadiance(vec3 rayDir)
{
    return mix(groundColor, SkyGradient(rayDir), smoothstep(-0.10, 0.02, rayDir.y));
}

// The solar disc, with a soft limb so it does not alias into a hard polygon.
vec3 SunDisc(vec3 rayDir)
{
    float cosAngle = clamp(dot(rayDir, sunDirection), -1.0, 1.0);
    float angle = acos(cosAngle);

    float disc = 1.0 - smoothstep(sunAngularRadius * 0.85, sunAngularRadius * 1.35, angle);

    // Tight bloom outside the limb, so the sun reads as blinding rather than as a flat circle.
    float bloom = exp(-angle / max(sunAngularRadius, 1e-4) * 0.8) * 0.30;

    return sunColor * (disc * 9.0 + bloom);
}

// How strongly this pixel should contribute to light shafts: bright at the sun, zero elsewhere.
float SunOcclusionMask(vec3 rayDir)
{
    float cosAngle = clamp(dot(rayDir, sunDirection), -1.0, 1.0);
    return pow(max(cosAngle, 0.0), sunGlowFalloff * 0.35);
}

// Analytic integral of exponential height fog along a ray, after Inigo Quilez. Integrating density
// rather than assuming it uniform lets fog thin with altitude instead of reading as a flat curtain.
float FogAmount(vec3 cameraPos, vec3 rayDir, float distance)
{
    float falloff = max(fogHeightFalloff, 1e-6);
    float verticalRate = rayDir.y * falloff;

    float densityAtCamera = fogDensity * exp(-(cameraPos.y - fogHeight) * falloff);

    // The limit of (1 - exp(-t)) / t as t goes to zero is t, which is the horizontal-ray case.
    float integral = abs(verticalRate) > 1e-5
        ? (1.0 - exp(-distance * verticalRate)) / verticalRate
        : distance;

    return 1.0 - exp(-densityAtCamera * integral);
}

// Fog picks up the sun when looking toward it, so haze reads as sunlit air rather than grey wash.
vec3 FogColor(vec3 rayDir)
{
    float cosAngle = clamp(dot(rayDir, sunDirection), -1.0, 1.0);
    float towardSun = pow(max(cosAngle, 0.0), 6.0);

    // Ungrounded gradient on purpose: haze over distant ground is lit air, not terrain colour.
    return mix(SkyGradient(rayDir), sunColor * 0.85, towardSun * 0.45);
}
