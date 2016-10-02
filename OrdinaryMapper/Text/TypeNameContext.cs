using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public class TypeNameContext
    {
        public TypeNameContext(TypeMap typeMap)
        {
            TypeMap = typeMap;
        }

        public TypeMap TypeMap { get; set; }
    }
}