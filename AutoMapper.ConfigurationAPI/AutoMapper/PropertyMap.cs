using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Extended.Net4;
using HappyMapper.AutoMapper.ConfigurationAPI.Execution;

namespace HappyMapper.AutoMapper.ConfigurationAPI
{
    [DebuggerDisplay("{DestMember.Name}")]
    public class PropertyMap
    {
        private readonly List<MemberInfo> _memberChain = new List<MemberInfo>();

        public PropertyMap(MemberInfo destMember, TypeMap typeMap)
        {
            TypeMap = typeMap;
            //UseDestinationValue = true;
            DestMember = destMember;
        }

        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestMember, typeMap)
        {
            ApplyInheritedPropertyMap(inheritedMappedProperty);
        }

        public TypeMap TypeMap { get; }
        public MemberInfo DestMember { get; }

        public Type DestType => DestMember.GetMemberType();

        public IEnumerable<MemberInfo> SourceMembers => _memberChain;

        public bool Ignored { get; set; }
        public bool AllowNull { get; set; }
        public int? MappingOrder { get; set; }
        public LambdaExpression CustomResolver { get; set; }
        /// <summary>
        /// AutoMapper-converted expression
        /// </summary>
        public LambdaExpression Condition { get; set; }

        public OriginalStatement OriginalCondition { get; set; }

        public LambdaExpression PreCondition { get; set; }
        public LambdaExpression CustomExpression { get; private set; }
        public MemberInfo CustomSourceMember { get; set; }
        public bool UseDestinationValue { get; set; }
        public bool ExplicitExpansion { get; set; }
        public object NullSubstitute { get; set; }
        public ValueResolverConfiguration ValueResolverConfig { get; set; }

        public TypePair GetTypePair()
        {
            //TODO cache
            return new TypePair(SrcType, DestType);
        }

        public MemberInfo SrcMember
        {
            get
            {
                if (CustomSourceMemberName != null)
                    return TypeMap.SourceType.GetMember(CustomSourceMemberName).FirstOrDefault();

                if (CustomSourceMember != null)
                    return CustomSourceMember;

                if (CustomExpression != null)
                {
                    var finder = new MemberFinderVisitor();
                    finder.Visit(CustomExpression);

                    if (finder.Member != null)
                    {
                        return finder.Member.Member;
                    }
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
                if (CustomResolver != null)
                    return CustomResolver.ReturnType;
                if(ValueResolverConfig != null)
                    return typeof(object);
                return SrcMember?.GetMemberType();
            }
        }

        public string CustomSourceMemberName { get; set; }

        public void ChainMembers(IEnumerable<MemberInfo> members)
        {
            var getters = members as IList<MemberInfo> ?? members.ToList();
            _memberChain.AddRange(getters);
        }

        public void ApplyInheritedPropertyMap(PropertyMap inheritedMappedProperty)
        {
            if (!CanResolveValue() && inheritedMappedProperty.Ignored)
            {
                Ignored = true;
            }
            CustomExpression = CustomExpression ?? inheritedMappedProperty.CustomExpression;
            CustomResolver = CustomResolver ?? inheritedMappedProperty.CustomResolver;
            Condition = Condition ?? inheritedMappedProperty.Condition;
            PreCondition = PreCondition ?? inheritedMappedProperty.PreCondition;
            NullSubstitute = NullSubstitute ?? inheritedMappedProperty.NullSubstitute;
            MappingOrder = MappingOrder ?? inheritedMappedProperty.MappingOrder;
            CustomSourceMember = CustomSourceMember ?? inheritedMappedProperty.CustomSourceMember;
            ValueResolverConfig = ValueResolverConfig ?? inheritedMappedProperty.ValueResolverConfig;
        }

        public bool IsMapped()
        {
            return _memberChain.Count > 0 
                || ValueResolverConfig != null 
                || CustomResolver != null 
                || SrcMember != null
                || CustomExpression != null
                || Ignored;
        }

        public bool CanResolveValue()
        {
            return (_memberChain.Count > 0
                || ValueResolverConfig != null
                || CustomResolver != null
                || SrcMember != null
                || CustomExpression != null) && !Ignored;
        }

        public void SetCustomValueResolverExpression<TSource, TMember>(Expression<Func<TSource, TMember>> sourceMember)
        {
            CustomExpression = sourceMember;

            Ignored = false;
        }

        public override string ToString()
        {
            return $"{SrcMember.Name} -> {DestMember.Name}";
        }

        private class MemberFinderVisitor : ExpressionVisitor
        {
            public MemberExpression Member { get; private set; }

            protected override Expression VisitMember(MemberExpression node)
            {
                Member = node;

                return base.VisitMember(node);
            }
        }
    }
}