
using System;
using AutoMapper.ConfigurationAPI;

namespace HappyMapper.Text
{
    public interface IAssignContext
    {
        string DestMemberName { get; }
        string SrcMemberName { get; }
        string DestTypeFullName { get; set; }
    }

    public interface IPropertyNameContext
    {
        TypePair TypePair { get; set; }

        Type SrcType { get; set; }
        Type DestType { get; set; }

        string SrcMemberName { get; set; }
        string DestMemberName { get; set; }

        string SrcTypeFullName { get; set; }
        string DestTypeFullName { get; set; }
    }

    public static class PropertyNameContextFactory
    {
        public static IPropertyNameContext CreateWithoutPropertyMap(Type srcType, Type destType, string srcPropertyName, string destPropertyName)
        {
            var ctx = new PropertyNameContext();
            ctx.SrcType = srcType;
            ctx.DestType = destType;
            ctx.SrcMemberName = srcPropertyName;
            ctx.DestMemberName = destPropertyName;
            ctx.SrcTypeFullName = srcType.FullName.NormalizeTypeName();
            ctx.DestTypeFullName = destType.FullName.NormalizeTypeName();

            return ctx;
        }
    }
    internal class PropertyNameContext : IAssignContext, IPropertyNameContext
    {
        internal PropertyNameContext() { }

        public PropertyNameContext(PropertyMap propertyMap)
        {
            PropertyMap = propertyMap;
            TypePair = propertyMap.GetTypePair();

            SrcMemberName = propertyMap.SrcMember.Name.NormalizeTypeName();
            DestMemberName = propertyMap.DestMember.Name.NormalizeTypeName();

            SrcTypeFullName = propertyMap.SrcType.FullName.NormalizeTypeName();
            DestTypeFullName = propertyMap.DestType.FullName.NormalizeTypeName();

            SrcType = propertyMap.SrcType;
            DestType = propertyMap.DestType;
        }

        public PropertyMap PropertyMap { get; }

        public TypePair TypePair { get; set; }
        
        public Type DestType { get; set; }

        public Type SrcType { get; set; }

        public string SrcTypeFullName { get; set; }

        public string DestMemberName { get; set; }

        public string SrcMemberName { get; set; }

        public string DestTypeFullName { get; set; }
    }
}