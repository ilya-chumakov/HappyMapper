using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace OrdinaryMapper
{
    [DebuggerDisplay("{SrcMember.Name} -> {DestMember.Name}")]
    public class PropertyMap
    {
        private readonly List<MemberInfo> _memberChain = new List<MemberInfo>();

        public PropertyMap(MemberInfo destMember, TypeMap typeMap)
        {
            TypeMap = typeMap;
            //UseDestinationValue = true;
            DestMember = destMember;
        }

        public TypeMap TypeMap { get; }
        public MemberInfo DestMember { get; }

        public Type DestType => DestMember.GetMemberType();

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

        public MemberInfo SrcMember
        {
            get
            {
                return _memberChain.LastOrDefault();
            }
        }

        public Type SrcType
        {
            get
            {
                return SrcMember?.GetMemberType();
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

        public void ChainMembers(IEnumerable<MemberInfo> members)
        {
            var getters = members as IList<MemberInfo> ?? members.ToList();
            _memberChain.AddRange(getters);
        }

        public TypePair GetTypePair()
        {
            //TODO cache
            return new TypePair(SrcType, DestType);
        }

        public override string ToString()
        {
            return $"{SrcMember.Name} -> {DestMember.Name}";
        }
    }
}