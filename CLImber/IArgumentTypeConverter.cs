namespace CLImber
{
    public interface IArgumentTypeConverter
    {
        object ConvertArgument(string arg);
    }


    //Defining some default Type Converters to include by default.
    public class ArgToInt
    : IArgumentTypeConverter
    {
        public object ConvertArgument(string arg)
        {
            return int.Parse(arg);
        }
    }

}
