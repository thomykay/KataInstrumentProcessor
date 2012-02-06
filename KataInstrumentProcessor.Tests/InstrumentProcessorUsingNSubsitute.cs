namespace KataInstrumentProcessor.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NSubstitute;

    [TestClass]
    public class InstrumentProcessorUsingNSubsituteFluentAssertions
    {
        [TestMethod]
        public void CreateInstrumentProcessor()
        {
            new InstrumentProcessor(Substitute.For<IInstrument>(), Substitute.For<ITaskDispatcher>(), Substitute.For<IConsole>()).Should().NotBeNull();
        }

        [TestMethod]
        public void when_the_method_Process_is_called_then_the_InstrumentProcessor_gets_the_next_task_from_the_task_dispatcher_and_executes_it_on_the_instrument()
        {
            ////Arrange
            var instrument = Substitute.For<IInstrument>();
            var taskDispatcher = Substitute.For<ITaskDispatcher>();
            var instrumentProcessor = new InstrumentProcessor(instrument, taskDispatcher, Substitute.For<IConsole>());
            var message = Guid.NewGuid().ToString();
            taskDispatcher.GetTask().Returns(message);

            ////Act
            instrumentProcessor.Process();

            ////Assert
            instrument.Received().Execute(message);
        }
    }
}
