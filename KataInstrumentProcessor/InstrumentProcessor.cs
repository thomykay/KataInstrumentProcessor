namespace KataInstrumentProcessor
{
    using System;
    using System.Collections.Generic;

    public class InstrumentProcessor : IInstrumentProcessor
    {
        private IInstrument instrument;

        private ITaskDispatcher taskDispatcher;

        private Queue<string> currentTasks = new Queue<string>();

        public InstrumentProcessor(IInstrument instrument, ITaskDispatcher taskDispatcher)
        {
            this.instrument = instrument;
            this.taskDispatcher = taskDispatcher;

            this.instrument.Finished += (sender, args) =>
                {
                    this.taskDispatcher.FinishedTask(currentTasks.Dequeue());
                };
        }

        public void Process()
        {
            var task = this.taskDispatcher.GetTask();
            this.currentTasks.Enqueue(task);
            this.instrument.Execute(task);
        }
    }
}