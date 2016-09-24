using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public class TypeNameContext
    {
        public TypeNameContext(TypeMap typeMap, string srcPrefix, string destPrefix)
        {
            TypeMap = typeMap;
            SrcMemberPrefix = srcPrefix;
            DestMemberPrefix = destPrefix;
        }

        public TypeMap TypeMap { get; set; }

        public string DestMemberPrefix { get; }

        public string SrcMemberPrefix { get; }
    }
}