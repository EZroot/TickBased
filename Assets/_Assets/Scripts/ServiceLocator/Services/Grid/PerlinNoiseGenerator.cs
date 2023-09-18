using UnityEngine;

public class PerlinNoiseGenerator
{
    private float _scale;
    private Vector2 _gridCenter;
    private float _distanceFromEdge;
    private float _secondLayerScale; // Scale for the second noise layer

    public PerlinNoiseGenerator(float layer1Scale, float layer2Scale, Vector2 gridCenter, float islandDistanceFromEdge)
    {
        _scale = layer1Scale;
        _secondLayerScale = layer2Scale;
        _gridCenter = gridCenter;
        _distanceFromEdge = islandDistanceFromEdge;
    }

    public float GetNoiseValue(int x, int y)
    {
        // Base Perlin noise layer
        float baseNoise = Mathf.PerlinNoise(x * _scale, y * _scale);

        // Additional noise layer for detail
        float secondLayerNoise = Mathf.PerlinNoise(x * _secondLayerScale, y * _secondLayerScale);
        
        // Combine noise layers
        float combinedNoise = Mathf.Lerp(baseNoise, secondLayerNoise, 0.5f);
        
        // Apply island shaping
        float islandFactor = GetIslandFactor(x, y, _distanceFromEdge);

        return combinedNoise * islandFactor;
    }
    
    private float GetIslandFactor(int x, int y, float distFromEdge)
    {
        float distanceToCenter = Vector2.Distance(new Vector2(x, y), _gridCenter) / (_gridCenter.x);
        float gradient = Mathf.Clamp01(1 - distanceToCenter * distFromEdge);
        return gradient * gradient; // Square the gradient for more natural falloff
    }

    public Color GetTerrainColor(float noiseValue)
    {
        Color perlinColor;

        if (noiseValue < 0.2f)
        {
            perlinColor = Color.blue; // Water
        }
        else if (noiseValue < 0.4f)
        {
            perlinColor = new Color(0.9f, 0.7f, 0.4f); // Sand
        }
        else if (noiseValue < 0.6f)
        {
            perlinColor = new Color(0.5f, 0.35f, 0.1f); // Ground
        }
        else if (noiseValue < 0.8f)
        {
            perlinColor = Color.green; // Grass
        }
        else
        {
            perlinColor = Color.grey; // Mountain
        }

        return perlinColor;
    }
}
