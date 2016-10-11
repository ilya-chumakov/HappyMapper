using System;

namespace HappyMapper.Text
{
    public static class CreationTemplates
    {
        public static string NewObject(Type type)
        {
            string name = type.FullName.NormalizeTypeName();

            if (type == typeof (string)) return "null";
            if (type.IsValueType) return $"default({name})";
            if (type.IsClass) return $"new {name}()"; //TODO: duplicated CodeTempates.New

            throw new NotSupportedException(name);
        }
    }
}