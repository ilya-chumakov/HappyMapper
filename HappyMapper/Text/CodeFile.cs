using AutoMapper.ConfigurationAPI;

namespace HappyMapper.Text
{
    public class CodeFile
    {
        public string Code { get; }
        public TypePair TypePair { get; }
        public string ClassFullName { get; }
        public string MapperMethodName { get; }

        public Assignment InnerMethodAssignment { get; set; }

        public CodeFile(string code, string classFullName, string mapperMethodName, TypePair typePair, Assignment assignment)
        {
            Code = code;
            ClassFullName = classFullName;
            MapperMethodName = mapperMethodName;
            TypePair = typePair;
            InnerMethodAssignment = assignment;
        }
    }
}