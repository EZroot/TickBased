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

    public Color GetTerrainColor(float noiseValue, int x, int y)
    {
        Color perlinColor;

        // Biome noise
        float biomeNoise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);

        // Check for specific biomes
        if (biomeNoise > 0.7f)
        {
            // Desert Biome
            if (noiseValue < 0.2f) perlinColor = new Color(0.1f, 0.1f, 0.5f); // Dark Water
            else if (noiseValue < 0.4f) perlinColor = new Color(0.95f, 0.85f, 0.4f); // Light Sand
            else perlinColor = new Color(0.8f, 0.7f, 0.3f); // Dark Sand
        }
        else if (biomeNoise < 0.3f)
        {
            // Forest Biome
            if (noiseValue < 0.2f) perlinColor = new Color(0.0f, 0.2f, 0.6f); // Water
            else if (noiseValue < 0.4f) perlinColor = new Color(0.35f, 0.45f, 0.25f); // Mossy Ground
            else perlinColor = new Color(0.1f, 0.5f, 0.1f); // Forest Ground
        }
        else
        {
            // Default Biome
            if (noiseValue < 0.2f) perlinColor = new Color(0.0f, 0.2f, 0.6f); // Water
            else if (noiseValue < 0.4f) perlinColor = new Color(0.85f, 0.7f, 0.45f); // Sand
            else if (noiseValue < 0.6f) perlinColor = new Color(0.45f, 0.3f, 0.15f); // Ground
            else if (noiseValue < 0.8f) perlinColor = new Color(0.2f, 0.6f, 0.2f); // Grass
            else perlinColor = new Color(0.6f, 0.6f, 0.6f); // Mountain
        }


        return perlinColor;
    }
}
