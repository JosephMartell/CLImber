using System;

namespace CLImber
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DesignatedConverterAttribute
        : Attribute
    {
        public DesignatedConverterAttribute(int argumentNumber, string converterName)
        {
            ArgumentNumber = argumentNumber;
            ConverterName = converterName;
        }

        public int ArgumentNumber { get; }
        public string ConverterName { get; }
    }
}
