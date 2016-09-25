
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper
{
    public interface IAssignContext
    {
        string DestMemberPrefix { get; }
        string DestMemberName { get; }
        string SrcMemberName { get; }
        string SrcMemberPrefix { get; }
        string DestTypeFullName { get; set; }
    }

    public class PropertyNameContext : IAssignContext
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

            DestTypeFullName = propertyMap.DestType.FullName;
        }

        public PropertyMap PropertyMap { get; }

        public string DestMemberPrefix { get; }

        public string DestMemberName { get; }

        public string SrcFullMemberName { get; }

        public string SrcMemberName { get; }

        public string DestFullMemberName { get; }

        public string SrcMemberPrefix { get; }

        public string DestTypeFullName { get; set; }

        private string Combine(string left, string right)
        {
            return $"{left}.{right}";
        }
    }
}