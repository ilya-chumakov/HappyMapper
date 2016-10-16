
using AutoMapper.Extended.Net4;

namespace HappyMapper.Text
{
    internal class MapCollectionNameConvention
    {
        public string SrcParam { get; set; }
        public string DestParam { get; set; }
        public string SrcCollection { get; set; }
        public string DestCollection { get; set; }
        public string Method { get; set; }
        public string CollectionTypeTemplate { get; set; }

        public string GetCollectionType(string genericArgumentFullName)
        {
            return string.Format(CollectionTypeTemplate, genericArgumentFullName.NormalizeTypeName());
        }
    }
}