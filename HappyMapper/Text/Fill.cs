using System;

namespace HappyMapper.Text
{
    public static class Fill
    {
        public static string GetText(Type type)
        {
            string name = type.FullName.NormalizeTypeName();

            if (type == typeof (string)) return "null";
            if (type.IsValueType) return $"default({name})";
            if (type.IsClass) return $"new {name}()";

            throw new NotSupportedException(name);
        }
    }
}