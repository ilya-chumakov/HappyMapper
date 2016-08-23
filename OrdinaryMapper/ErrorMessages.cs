using System;

namespace OrdinaryMapper
{
    public static class ErrorMessages
    {
        public static string MissingMapping(Type srcType, Type destType)
        {
            return $"Missing mapping: {srcType.FullName} -> {destType.FullName}. Did you forget to call CreateMap method?";
        }
    }
}