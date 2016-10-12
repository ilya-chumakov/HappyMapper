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

        public static string GetFriendlyName(this Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
                friendlyName = type.Namespace + "." + friendlyName;
            }
            else
            {
                friendlyName = type.FullName;
            }

            return friendlyName.Replace('+', '.');
        }
    }
}