using System;
using System.Linq;

namespace AutoMapper.Extended.Net4
{
    public static class UnsupportedTypeBlocker
    {
        public static bool IsValid(Type type)
        {
            //bool isStruct = type.IsValueType && !type.IsPrimitive;

            return type.IsClass && !type.IsAbstract && !type.IsGenericType;
        }

        public static NotSupportedException CreateException(Type sourceType)
        {
            string message = $@"The type {sourceType.FullName} is not supported. 
                HappyMapper is in 'proof-of-concept' state, so only simplest object-to-object maps are implemented.";

            return new NotSupportedException(message);
        }
    }
}
