namespace CLImber.Example
{

    class Program
    {
        static readonly CLIHandler _handler = new CLIHandler();
        static void CLImberConfig()
        {
            _handler.RegisterTypeConverter<decimal>(s => decimal.Parse(s));
        }

        static void Main(string[] args)
        {
            CLImberConfig();
            _handler.Handle(args);
        }
    }
}
