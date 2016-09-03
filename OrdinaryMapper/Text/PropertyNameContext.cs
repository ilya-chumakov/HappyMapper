
using AutoMapper;

namespace OrdinaryMapper
{
    public class PropertyNameContext
    {
        public PropertyNameContext(PropertyMap propertyMap, string srcPrefix, string destPrefix)
        {
            PropertyMap = propertyMap;
            SrcMemberPrefix = srcPrefix;
            DestMemberPrefix = destPrefix;
            SrcMemberName = propertyMap.SrcMember.Name;
            DestMemberName = propertyMap.DestMember.Name;
            SrcFullMemberName = Combine(SrcMemberPrefix, SrcMemberName);
            DestFullMemberName = Combine(DestMemberPrefix, DestMemberName);
        }

        public PropertyMap PropertyMap { get; }

        public string DestMemberPrefix { get; }

        public string DestMemberName { get; }

        public string SrcFullMemberName { get; }

        public string SrcMemberName { get; }

        public string DestFullMemberName { get; }

        public string SrcMemberPrefix { get; }

        private string Combine(string left, string right)
        {
            return $"{left}.{right}";
        }
    }
}