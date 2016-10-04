using System;

namespace OrdinaryMapper
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
    }
}