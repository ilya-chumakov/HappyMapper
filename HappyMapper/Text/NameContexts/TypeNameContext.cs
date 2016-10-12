using AutoMapper.ConfigurationAPI;

namespace HappyMapper.Text
{
    internal class TypeNameContext
    {
        public TypeNameContext(TypeMap typeMap)
        {
            TypeMap = typeMap;
        }

        public TypeMap TypeMap { get; set; }
    }
}