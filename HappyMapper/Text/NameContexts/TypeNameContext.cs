using AutoMapper.ConfigurationAPI;

namespace HappyMapper.Text
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