using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper.Text
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