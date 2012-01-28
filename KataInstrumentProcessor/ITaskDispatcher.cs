namespace KataInstrumentProcessor
{
    public interface ITaskDispatcher
    {
        string GetTask();
        void FinishedTask(string task);
    }
}