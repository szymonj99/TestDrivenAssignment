namespace TestDrivenAssignment
{
    public interface IWebService
    {
        void LogStateChange(string logDetails);
        void LogEngineerRequired(string logDetails);
        void LogFireAlarm(string logDetails);
    }
}
