
using AutoMapper.ConfigurationAPI;

namespace OrdinaryMapper.Text
{
    public interface IAssignContext
    {
        string DestMemberName { get; }
        string SrcMemberName { get; }
        string DestTypeFullName { get; set; }
    }

    public class PropertyNameContext : IAssignContext
    {
        public PropertyNameContext(PropertyMap propertyMap)
        {
            PropertyMap = propertyMap;
            SrcMemberName = propertyMap.SrcMember.Name;
            DestMemberName = propertyMap.DestMember.Name;
            DestTypeFullName = propertyMap.DestType.FullName;
        }

        public PropertyMap PropertyMap { get; }

        public string DestMemberName { get; }

        public string SrcMemberName { get; }

        public string DestTypeFullName { get; set; }
    }
}