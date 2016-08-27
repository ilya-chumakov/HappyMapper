using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace OrdinaryMapper
{
    [DebuggerDisplay("{DestinationProperty.Name}")]
    public class PropertyMap
    {
        private readonly List<MemberInfo> _memberChain = new List<MemberInfo>();

        public PropertyMap(MemberInfo destinationProperty, TypeMap typeMap)
        {
            TypeMap = typeMap;
            //UseDestinationValue = true;
            DestinationProperty = destinationProperty;
        }

        public TypeMap TypeMap { get; }
        public MemberInfo DestinationProperty { get; }

        public Type DestinationPropertyType => DestinationProperty.GetMemberType();

        public IEnumerable<MemberInfo> SourceMembers => _memberChain;

        public bool Ignored { get; set; }
        //public bool AllowNull { get; set; }
        //public int? MappingOrder { get; set; }
        //public LambdaExpression CustomResolver { get; set; }
        //public LambdaExpression Condition { get; set; }
        //public LambdaExpression PreCondition { get; set; }
        //public LambdaExpression CustomExpression { get; private set; }
        //public MemberInfo CustomSourceMember { get; set; }
        //public bool UseDestinationValue { get; set; }
        //public bool ExplicitExpansion { get; set; }
        //public object NullSubstitute { get; set; }

        public MemberInfo SourceMember
        {
            get
            {
                return _memberChain.LastOrDefault();
            }
        }

        public Type SourceType
        {
            get
            {
                return SourceMember?.GetMemberType();
            }
        }

        public string CustomSourceMemberName { get; set; }

        public bool IsMapped()
        {
            return Ignored;
        }

        public bool CanResolveValue()
        {
            return !Ignored;
        }
    }
}