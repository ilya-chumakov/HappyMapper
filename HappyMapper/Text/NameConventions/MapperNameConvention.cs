
using AutoMapper.Extended.Net4;
using HappyMapper.AutoMapper.ConfigurationAPI;

namespace HappyMapper.Text
{
    internal class MapperNameConvention
    {
        public string Namespace { get; set; }
        public string ClassShortName { get; set; }

        public string ClassFullName => $"{Namespace}.{ClassShortName}";

        public string CreateUniqueMapperMethodNameWithGuid(TypePair typePair)
        {
            string guid = NamingTools.NewGuid(MaxGuidLength);

            string normSrcName = NamingTools.ToAlphanumericOnly(typePair.SourceType.Name);
            string normDestName = NamingTools.ToAlphanumericOnly(typePair.DestinationType.Name);

            return $"Mapper_{normSrcName}_{normDestName}_{guid}";
        }

        public int? MaxGuidLength
        {
            get
            {
#if DEBUG
                return 4;
#else
                return null;
#endif
            }
        }
    }
}