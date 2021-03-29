using FluentAssertions;
using Xunit;

namespace CLImber.Tests
{

    [CommandClass("test_command")]
    public class DummyCommand
    {
        public static int CallCount { get; private set; } = 0;

        public static bool Flag { get; private set; } = false;

        public static bool OtherFlag { get; private set; } = false;

        public static string StringOption { get; private set; } = string.Empty;

        public static int IntOption { get; private set; } = 0;

        public DummyCommand()
        {
            CallCount = 0;
            Flag = false;
            StringOption = string.Empty;
            IntOption = 0;
            OtherFlag = false;
        }

        [CommandHandler]
        public void DefaultHandler()
        {
            CallCount++;
        }

        [CommandOption("flag", Abbreviation = 'f')]
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

        [CommandOption("other", Abbreviation = 'o')]
        public bool InstanceOtherFlag
        {
            get
            {
                return OtherFlag;
            }
            set
            {
                OtherFlag = value;
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

        [CommandOption("int")]
        public int InstanceIntOption
        {
            get
            {
                return IntOption;
            }
            set
            {
                IntOption = value;
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

            DummyCommand.CallCount.Should().Be(1);

        }

        [Fact]
        public void Handle_ShouldFindCommand_RegardlessOfArgumentCase()
        {
            string[] arguments = { "test_command" };
            string[] argumentsWithCapitols = { "Test_Command" };
            string[] argumentsWithRandomCapitols = { "TEst_ComManD" };

            _sut.Handle(arguments);
            DummyCommand.CallCount.Should().Be(1);

            _sut.Handle(argumentsWithCapitols);
            DummyCommand.CallCount.Should().Be(1);

            _sut.Handle(argumentsWithRandomCapitols);
            DummyCommand.CallCount.Should().Be(1);
        }

        [Fact]
        public void Handle_ShouldSetBoolProp_WhenOptionIsInArgs()
        {
            string[] arguments = { "test_command" , "--flag" };

            _sut.Handle(arguments);

            DummyCommand.Flag.Should().BeTrue();
        }

        [Fact]
        public void Handle_ShouldIgnoreCase_WhenSettingOptions()
        {
            string[] arguments = { "tEst_ComMand", "--fLaG" };

            _sut.Handle(arguments);

            DummyCommand.Flag.Should().BeTrue();
        }

        [Fact]
        public void Handle_ShouldSetStringProp_WithValueAfterEqualSign()
        {
            string[] arguments = { "test_command", "--stringOption=this is the new value" };

            _sut.Handle(arguments);

            DummyCommand.StringOption.Should().BeEquivalentTo("this is the new value");
        }

        [Fact]
        public void Handle_ShouldSetIntegerOptions_WhenValidValueIsProvided()
        {
            string[] arguments = { "test_command", "--int=17" };

            _sut.Handle(arguments);

            DummyCommand.IntOption.Should().Be(17);
        }

        [Fact]
        public void Handle_ShouldSetFlag_WhenAbbreviationsIsUsed()
        {
            string[] arguments = { "test_command", "-f" };

            _sut.Handle(arguments);

            DummyCommand.Flag.Should().BeTrue();
        }

        [Fact]
        public void Handle_ShouldSetAllShortOptions_WhenAggregated()
        {
            string[] arguments = { "test_command", "-fo" };

            _sut.Handle(arguments);

            DummyCommand.Flag.Should().BeTrue();
            DummyCommand.OtherFlag.Should().BeTrue();
        }

        [Fact]
        public void Handle_SetsOptions_WhenSetWithNormalCommand()
        {
            string[] arguments = { "test_command", "-fo" };

            _sut.Handle(arguments);

            DummyCommand.Flag.Should().BeTrue();
            DummyCommand.OtherFlag.Should().BeTrue();
            DummyCommand.CallCount.Should().Be(1);
        }
    }
}
