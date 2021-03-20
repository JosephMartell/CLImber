using FluentAssertions;
using Xunit;

namespace CLImber.Tests
{

    [CommandClass("test_command")]
    public class DummyCommand
    {
        public static int CallCount { get; private set; } = 0;

        public static bool Flag { get; private set; } = false;

        public static string StringOption { get; private set; } = string.Empty;

        [CommandHandler]
        public void DefaultHandler()
        {
            CallCount++;
        }

        [CommandOption("flag")]
        public bool InstanceFlag
        {
            get
            {
                return Flag;
            }
            set
            {
                Flag = value;
            }
        }

        [CommandOption("stringOption")]
        public string InstanceStringOption
        {
            get
            {
                return StringOption;
            }
            set
            {
                StringOption = value;
            }
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

        [Fact]
        public void Handle_ShouldSetBoolProp_WhenOptionIsInArgs()
        {
            string[] arguments = { "test_command" , "--flag" };
            DummyCommand.Flag.Should().BeFalse();

            _sut.Handle(arguments);

            DummyCommand.Flag.Should().BeTrue();
        }

        [Fact]
        public void Handle_ShouldSetStringProp_WithValueAfterEqualSign()
        {
            string[] arguments = { "test_command", "--stringOption=this is the new value" };
            DummyCommand.StringOption.Should().BeEquivalentTo(string.Empty);

            _sut.Handle(arguments);

            DummyCommand.StringOption.Should().BeEquivalentTo("this is the new value");
        }

    }
}
