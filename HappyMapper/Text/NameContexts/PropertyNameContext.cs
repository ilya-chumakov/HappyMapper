
using AutoMapper.ConfigurationAPI;

namespace HappyMapper.Text
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
            SrcMemberName = propertyMap.SrcMember.Name.NormalizeTypeName();
            DestMemberName = propertyMap.DestMember.Name.NormalizeTypeName();
            DestTypeFullName = propertyMap.DestType.FullName.NormalizeTypeName();
        }

        public PropertyMap PropertyMap { get; }

        public string DestMemberName { get; }

        public string SrcMemberName { get; }

        public string DestTypeFullName { get; set; }
    }
}