using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public class CodeFile
    {
        public string Code { get; }
        public TypePair TypePair { get; }
        public string ClassFullName { get; }
        public string MapperMethodName { get; }

        public CodeFile(string code, string classFullName, string mapperMethodName, TypePair typePair)
        {
            Code = code;
            ClassFullName = classFullName;
            MapperMethodName = mapperMethodName;
            TypePair = typePair;
        }
    }
}