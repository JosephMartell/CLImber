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

        [Fact]
        public void Handle_ShouldFindCommand_RegardlessOfArgumentCase()
        {
            string[] arguments = { "test_command" };
            string[] argumentsWithCapitols = { "Test_Command" };
            string[] argumentsWithRandomCapitols = { "TEst_ComManD" };
            var callCountBefore = DummyCommand.CallCount;

            _sut.Handle(arguments);
            _sut.Handle(argumentsWithCapitols);
            _sut.Handle(argumentsWithRandomCapitols);

            DummyCommand.CallCount.Should().Be(callCountBefore + 3);

        }
    }
}
