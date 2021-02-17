using System;
using System.Collections.Generic;
using System.Linq;

namespace CLImber
{
    public class CLIHandler
    {

        private Type _handler;
        private List<IArgumentTypeConverter> _converters;

        public void RegisterCmdHandler<T>(string cmd, List<IArgumentTypeConverter> converters) where T : CommandBase
        {
            _handler = typeof(T);
            _converters = converters;
        }

        public string Handle(IEnumerable<string> args)
        {
            var convertedParms = new List<object>();

            for (int i = 0; i < args.Count(); i++)
            {
                convertedParms.Add(_converters[i].ConvertArgument(args.ElementAt(i)));
            }
            var ctors = _handler.GetConstructors().Where((info) => info.IsPublic);

            var chosenCtor = from c in ctors
                             where (c.GetParameters().Count() == args.Count())
                                && (c.GetParameters().Select(p => p.ParameterType).Except(convertedParms.Select(a => a.GetType()))).Count() == 0
                             select c;

            if (chosenCtor.Count() > 1)
            { 
                return "Could not determine constructor based on passed parameters";
            }

            chosenCtor.First().Invoke(args.ToArray());
            return "constructor called";
        }

        //protected IEnumerable<object> TranslateParameters(IEnumerable<string> args)
        //{

        //}
    }

    public class CommandBase
    {

    }

    public interface IArgumentTypeConverter
    {
        object ConvertArgument(string arg);
    }
}
