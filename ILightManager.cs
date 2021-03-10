namespace TestDrivenAssignment
{
    public interface ILightManager : IManager
    {
        void SetLight(bool isOn, int lightID);
        void SetAllLights(bool isOn);
    }
}
