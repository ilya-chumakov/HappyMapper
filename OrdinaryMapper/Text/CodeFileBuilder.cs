using System.Text;
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public class CodeFileBuilder
    {
        public TypePair TypePair { get; set; }
        public MapperNameConvention Convention { get; set; } = NameConventions.Mapper;

        public string DestFullName { get; }
        public string SrcFullName { get; }

        public CodeFileBuilder(TypePair typePair)
        {
            TypePair = typePair;
            SrcFullName = typePair.SourceType.FullName;
            DestFullName = typePair.DestinationType.FullName;
        }
    }
}