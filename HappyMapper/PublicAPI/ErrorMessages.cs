using System;

namespace HappyMapper
{
    public static class ErrorMessages
    {
        public static string MissingMapping(Type srcType, Type destType)
        {
            return $"Missing mapping: {srcType.FullName} -> {destType.FullName}. Did you forget to call CreateMap method?";
        }

        public static string NoParameterlessCtor(string srcProperty, string destProperty, Type destType)
        {
            return 
                $"{srcProperty} -> {destProperty} cann't be mapped because destination is null and destination type {destType.FullName} has no parameterless ctor";
        }

        public static string NoParameterlessCtor(Type destType)
        {
            return 
                $"Destination is null and destination type {destType.FullName} has no parameterless ctor";
        }
    }
}