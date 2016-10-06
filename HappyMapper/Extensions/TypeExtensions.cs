using System;
using HappyMapper.Text;

namespace HappyMapper
{
    public static class TypeExtensions
    {
        public static bool IsImplicitCastableFrom(this Type dest, Type src)
        {
            return ImplicitCastChecker.CanCast(src, dest);
        }

        public static bool IsExplicitCastableFrom(this Type dest, Type src)
        {
            return ExplicitCastChecker.CanCast(src, dest);
        }

        public static bool HasParameterlessCtor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}