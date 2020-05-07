using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMap
{
    private const float H = 0.0625f;

    // Get height at world position (x,z)
    public static float GetHeight(float x, float z)
    {
        return Mathf.PerlinNoise(x / 7.0f, z / 7.0f);
    }

    // Get normal at world position (x,z)
    public static Vector3 GetNormal(float x, float z)
    {
        float xSample = x / 7.0f;
        float zSample = z / 7.0f;
        float sample = Mathf.PerlinNoise(xSample, zSample);
        float xPartial = (Mathf.PerlinNoise(xSample + H, zSample) - sample) / H;
        float zPartial = (Mathf.PerlinNoise(xSample, zSample + H) - sample) / H;
        return new Vector3(-xPartial, 1.0f, -zPartial).normalized;
    }
}