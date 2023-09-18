using UnityEngine;

namespace FearProj.ServiceLocator
{
    public interface IServiceLightManager : IService
    {
        void GenerateLightMesh(GridManager.GridData gridData);
        void AddRadialLight(string id, int sourceX, int sourceY, int radius, Color lightColor);
        //void ApplyAccumulatedLight();
        void UpdateLightSourcePosition(string id, int newX, int newY);
    }
}