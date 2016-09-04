using System.Linq.Expressions;

namespace AutoMapper.ConfigurationAPI.QueryableExtensions.Impl
{
    public class ParameterReplacementVisitor : ExpressionVisitor
    {
        private readonly Expression _memberExpression;

        public ParameterReplacementVisitor(Expression memberExpression)
        {
            _memberExpression = memberExpression;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _memberExpression;
        }
    }
}