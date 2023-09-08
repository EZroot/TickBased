namespace FearProj.ServiceLocator
{
    public interface IServiceGameManager : IService
    {
        GameSettings GameSettings { get; }
    }
}