using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OrdinaryMapper.Obsolete
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
        public LambdaExpression CustomExpression { get; private set; }
        //public MemberInfo CustomSourceMember { get; set; }
        //public bool UseDestinationValue { get; set; }
        //public bool ExplicitExpansion { get; set; }
        //public object NullSubstitute { get; set; }

        public MemberInfo SrcMember
        {
            get
            {
                if (CustomExpression != null)
                {
                    var member = ExpressionHelper.GetMemberInfo(CustomExpression);

                    if (member != null) return member;
                }

                return _memberChain.LastOrDefault();
            }
        }

        public Type SrcType
        {
            get
            {
                if (CustomExpression != null)
                    return CustomExpression.ReturnType;

                return SrcMember?.GetMemberType();
            }
        }

        public string CustomSourceMemberName { get; set; }

        public bool IsMapped()
        {
            return CustomExpression != null
                || Ignored;
        }

        public bool CanResolveValue()
        {
            return CustomExpression != null && !Ignored;
        }

        public void ChainMembers(IEnumerable<MemberInfo> members)
        {
            var getters = members as IList<MemberInfo> ?? members.ToList();
            _memberChain.AddRange(getters);
        }

        public void SetCustomValueResolverExpression<TSource, TMember>(Expression<Func<TSource, TMember>> sourceMember)
        {
            CustomExpression = sourceMember;

            Ignored = false;
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