using FluentAssertions;
using Xunit;

namespace CLImber.Tests
{

    [CommandClass("test_command")]
    public class DummyCommand
    {
        public static int CallCount { get; private set; } = 0;

        [CommandHandler]
        public void DefaultHandler()
        {
            CallCount++;
        }
    }

    public class CLIHandlerTests
    {
        private readonly CLIHandler _sut = new CLIHandler();

        [Fact]
        public void Handle_ShouldFindClassesInAssembly_WhenDecoratedWithCommandClass()
        {
            string[] arguments = { "test_command" };
            var callCountBefore = DummyCommand.CallCount;

            _sut.Handle(arguments);

            DummyCommand.CallCount.Should().Be(callCountBefore + 1);

        }
    }
}
