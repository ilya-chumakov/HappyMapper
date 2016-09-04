using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoMapper.Extended
{
    public static class UnsupportedTypeBlocker
    {
        public static bool IsValid(Type type)
        {
            bool isStruct = type.IsValueType && !type.IsPrimitive;
            bool isClass = type.IsClass && !type.IsAbstract;

            return isStruct || isClass;

        }
    }
}
