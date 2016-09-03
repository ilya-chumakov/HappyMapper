using System.Linq.Expressions;
using System.Reflection;

namespace OrdinaryMapper.Obsolete
{
    public static class ExpressionHelper
    {
        public static MemberInfo GetMemberInfo(LambdaExpression expression)
        {
            var finder = new MemberFinderVisitor();
            finder.Visit(expression);

            return finder.MemberExpression?.Member;
        }

        private class MemberFinderVisitor : ExpressionVisitor
        {
            public MemberExpression MemberExpression { get; private set; }

            protected override Expression VisitMember(MemberExpression node)
            {
                MemberExpression = node;

                return base.VisitMember(node);
            }
        }
    }
}