using FluentAssertions;
using Xunit;

namespace CLImber.Tests
{
    [CommandClass("decimal_command")]
    public class DecimalDummyCmd
    {
        public static decimal[] _args;

        public static void ResetIndicators()
        {
            _args = null;
        }

        [CommandHandler]
        public void ConvertToDecimalArray(decimal[] args)
        {
            _args = args;
        }

    }

    [CommandClass("test_command")]
    public class DummyCommand
    {
        public static int CallCount { get; private set; } = 0;

        public static bool Flag { get; private set; } = false;

        public static bool OtherFlag { get; private set; } = false;

        public static string StringOption { get; private set; } = string.Empty;

        public static int IntOption { get; private set; } = 0;

        public static string CommandArg { get; private set; } = string.Empty;

        public static int OverloadCmdArg { get; private set; } = 0;

        public static string[] CommandArgs { get; set; } = new string[0];

        public static decimal[] OverloadedArgs { get; private set; } = new decimal[0];

        public static void ResetIndicators()
        {
            CallCount = 0;
            Flag = false;
            OtherFlag = false;
            StringOption = string.Empty;
            IntOption = 0;
            CommandArg = string.Empty;
            OverloadCmdArg = 0;
            CommandArgs = new string[0];
            OverloadedArgs = new decimal[0];
        }

        public DummyCommand()
        {
            CallCount = 0;
            Flag = false;
            StringOption = string.Empty;
            IntOption = 0;
            OtherFlag = false;
            CommandArg = string.Empty;
        }

        [CommandHandler]
        public void DefaultHandler()
        {
            CallCount++;
        }

        [CommandHandler]
        public void CommandHandlerWithArgs(string arg1)
        {
            CallCount++;
            CommandArg = arg1;
        }

        [CommandHandler]
        public void OverloadHandler(int arg1)
        {
            OverloadCmdArg = arg1;
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

        [CommandOption("stringOption", Abbreviation = 's')]
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

        [CommandOption("int", Abbreviation = 'i')]
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

        [CommandHandler]
        public void StringArrayHandler(string[] args)
        {
            CommandArgs = args;
        }

        [CommandHandler]
        public void OverloadedArrayHanlder(decimal[] args)
        {
            OverloadedArgs = args;
        }

        [CommandHandler]
        public void ThrowsException(int a, int b)
        {
            throw new System.Exception("Test exception");
        }
    }

    public class CLIHandlerTests
    {
        private readonly CLIHandler _sut = new CLIHandler();

        [Fact]
        public void Handle_ShouldFindClassesInAssembly_WhenDecoratedWithCommandClass()
        {
            string[] arguments = { "test_command" };

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

            DummyCommand.ResetIndicators();

            _sut.Handle(argumentsWithCapitols);
            DummyCommand.CallCount.Should().Be(1);

            DummyCommand.ResetIndicators();

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

        [Fact]
        public void Handle_SetsOptionsAndParsesArgs_WhenBothArePresent()
        {
            string[] arguments = { "test_command", "-fo", "--stringOption=someValue", "this is the argument"};

            _sut.Handle(arguments);

            DummyCommand.Flag.Should().BeTrue();
            DummyCommand.OtherFlag.Should().BeTrue();
            DummyCommand.StringOption.Should().Be("someValue");
            DummyCommand.CallCount.Should().Be(1);
            DummyCommand.CommandArg.Should().Be("this is the argument");
        }

        [Fact]
        public void Handle_IgnoresTextCase_WhenRequested()
        {
            string[] argumentsWithCase = { "test_Command" };
            string[] argumentsNoCase = { "test_command" };
            _sut.IgnoreCommandCase = true;
            
            _sut.Handle(argumentsNoCase);
            DummyCommand.CallCount.Should().Be(1);

            DummyCommand.ResetIndicators();

            _sut.Handle(argumentsWithCase);
            DummyCommand.CallCount.Should().Be(1);

        }

        [Fact]
        public void Handle_UsesTextCase_WhenRequested()
        {
            string[] argumentsWithCase = { "tEst_ComMand" };
            string[] argumentsNoCase = { "test_command" };
            _sut.IgnoreCommandCase = false;
            
            _sut.Handle(argumentsNoCase);
            DummyCommand.CallCount.Should().Be(1);

            DummyCommand.ResetIndicators();

            _sut.Handle(argumentsWithCase);
            DummyCommand.CallCount.Should().Be(0);
        }

        [Fact]
        public void Handle_SetsOptionValue_ForAbbreviatedOptions()
        {
            string[] arguments = { "test_command", "-s=someValue" };
            _sut.Handle(arguments);
            DummyCommand.StringOption.Should().BeEquivalentTo("someValue");

        }

        [Fact]
        public void Handle_SetsOptionValue_WhenValueIsNextArgument()
        {
            string[] arguments = { "test_command", "-s", "someValue" };
            _sut.Handle(arguments);
            DummyCommand.StringOption.Should().BeEquivalentTo("someValue");

        }
        [Fact]
        public void Handle_SetsOptionValue_WhenValueIsPartOfAggregateGroup()
        {
            string[] arguments = { "test_command", "-fs", "someValue" };
            _sut.Handle(arguments);
            DummyCommand.Flag.Should().BeTrue();
            DummyCommand.StringOption.Should().BeEquivalentTo("someValue");
        }

        [Fact]
        public void Handle_ShouldNotRun_WhenMultipleAggregatesNeedValues()
        {
            string[] arguments = { "test_command", "-fsi", "someValue", "5" };
            _sut.Handle(arguments);
            DummyCommand.CallCount.Should().Be(0);
        }

        [Fact]
        public void Handle_PassesNArguments_WhenHandlerAcceptsArray()
        {
            string[] arguments = { "test_command", "arg1", "arg2", "arg3", "arg4" };

            _sut.Handle(arguments);
            DummyCommand.CommandArgs.Should().HaveCount(4);
        }

        [Fact]
        public void Handle_PropagatesExceptions_WhenThrownInInvokedMethod()
        {
            string[] arguments = { "test_command", "5", "7" };

            _sut.Invoking(y => y.Handle(arguments))
                .Should().Throw<System.Exception>();
        }

        [Fact]
        public void Handle_ConvertsToDecimalArray_WhenCalled()
        {
            string[] arguments = { "decimal_command", "5", "7", "87.6" };

            _sut.Invoking(y => y.Handle(arguments))
                .Should().NotThrow();
            DecimalDummyCmd._args.Length.Should().Be(3);

        }
    
        [Fact]
        public void Handle_ChoosesOverloadedHandlers_BasedOnSuppliedType()
        {
            string[] arguments = { "test_command", "this is the argument" };
            _sut.Handle(arguments);
            DummyCommand.CommandArg.Should().Be("this is the argument");

            string[] olArguments = { "test_command", "5" };
            _sut.Handle(olArguments);
            DummyCommand.OverloadCmdArg.Should().Be(5);
        }

        [Fact]
        public void Handle_WorksWithOverloadedArrayMethods()
        {
            string[] arguments = { "test_command", "5", "7", "87.6" };

            _sut.Invoking(y => y.Handle(arguments))
                .Should().NotThrow();
            DummyCommand.OverloadedArgs.Length.Should().Be(3);

        }

    }
}
