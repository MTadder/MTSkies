namespace UAPObservationMod
{
    public interface IUAPBehavior
    {
        void Initialize(UAPEntity entity);
        void Tick(UAPEntity entity, float deltaTime);
        void Shutdown(UAPEntity entity);
    }
}
