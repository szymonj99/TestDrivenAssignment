namespace TestDrivenAssignment
{
    public interface IDoorManager : IManager
    {
        bool OpenDoor(int doorID);
        bool LockDoor(int doorID);
        bool OpenAllDoors();
        bool LockAllDoors();
    }
}
