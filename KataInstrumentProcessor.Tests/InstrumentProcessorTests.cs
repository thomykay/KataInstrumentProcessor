namespace KataInstrumentProcessor.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Summary description for InstrumentTests
    /// </summary>
    [TestClass]
    public class InstrumentProcessorTests
    {
        [TestMethod]
        public void CreateInstrumentProcessor()
        {
            var instrument = new Mock<IInstrument>();
            var taskDispatcher = new Mock<ITaskDispatcher>();
            var instrumentProcessor = new InstrumentProcessor(instrument.Object, taskDispatcher.Object);
            Assert.IsNotNull(instrumentProcessor);
        }

        [TestMethod]
        public void when_the_method_Process_is_called_then_the_InstrumentProcessor_gets_the_next_task_from_the_task_dispatcher_and_executes_it_on_the_instrument()
        {
            ////Arrange
            var instrument = new Mock<IInstrument>();
            var taskDispatcher = new Mock<ITaskDispatcher>();
            var instrumentProcessor = new InstrumentProcessor(instrument.Object, taskDispatcher.Object);
            var message = Guid.NewGuid().ToString();
            taskDispatcher.Setup(d => d.GetTask()).Returns(message);

            ////Act
            instrumentProcessor.Process();

            ////Assert
            instrument.Verify(i => i.Execute(message), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void when_the_Execute_method_of_the_instrument_throws_an_exception_then_this_exception_is_passed_on_to_the_caller_of_the_Process_method()
        {
            ////Arrange
            var instrument = new Mock<IInstrument>();
            var taskDispatcher = new Mock<ITaskDispatcher>();
            var instrumentProcessor = new InstrumentProcessor(instrument.Object, taskDispatcher.Object);
            instrument.Setup(i => i.Execute(It.IsAny<string>())).Throws(new Exception());

            ////Act
            instrumentProcessor.Process();

            ////Assert
        }

        [TestMethod]
        public void when_the_instrument_fires_the_Finished_event_then_the_InstrumentProcessor_calls_the_task_dispatchers_FinishedTask_method_with_the_correct_task()
        {
            ////Arrange
            var instrument = new Mock<IInstrument>();
            var taskDispatcher = new Mock<ITaskDispatcher>();
            var instrumentProcessor = new InstrumentProcessor(instrument.Object, taskDispatcher.Object);
            var message = Guid.NewGuid().ToString();
            taskDispatcher.Setup(d => d.GetTask()).Returns(message);
            instrument.Setup(i => i.Execute(message)).Raises(i => i.Finished += null, EventArgs.Empty);

            ////Act
            instrumentProcessor.Process();

            ////Assert
            taskDispatcher.Verify(t => t.FinishedTask(message), Times.Once());
        }

        [TestMethod]
        public void when_the_instrument_fires_the_Finished_event_then_the_InstrumentProcessor_calls_the_task_dispatchers_FinishedTask_method_with_the_correct_task_multiple()
        {
            ////Arrange
            var instrument = new Mock<IInstrument>();
            var taskDispatcher = new Mock<ITaskDispatcher>();
            var instrumentProcessor = new InstrumentProcessor(instrument.Object, taskDispatcher.Object);

            ////Act
            var messages = new Queue<string>();
            for (int i1 = 0; i1 < 10; i1++)
            {
                var message = Guid.NewGuid().ToString();
                messages.Enqueue(message);
                taskDispatcher.Setup(d => d.GetTask()).Returns(message);
                instrumentProcessor.Process();
            }

            ////Assert
            for (int i2 = 0; i2 < 10; i2++)
            {
                instrument.Raise(i => i.Finished += null, EventArgs.Empty);
                taskDispatcher.Verify(t => t.FinishedTask(messages.Dequeue()), Times.Once());
            }
        }

        [TestMethod]
        public void when_the_instrument_fires_the_Error_event_then_the_InstrumentProcessor_writes_the_string_Error_occurred_to_the_console()
        {
           Assert.Inconclusive(); 
        }
    }
}
