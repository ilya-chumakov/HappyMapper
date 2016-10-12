using System;

namespace HappyMapper.Text
{
    public static class CreationTemplates
    {
        public static string NewObject(Type type)
        {
            //string name = type.FullName.NormalizeTypeName();
            string name = type.GetFriendlyName();

            if (type == typeof (string)) return "null";
            if (type.IsValueType) return $"default({name})";
            if (type.IsClass) return $"new {name}()"; //TODO: duplicated CodeTempates.New

            throw new NotSupportedException(name);
        }

        public static string NewCollection(Type type, string countTemplate)
        {
            var argTypeName = type.GenericTypeArguments[0].FullName.NormalizeTypeName();

            if (type.IsArray) return $"new {argTypeName}[{countTemplate}];";

            //suppose generic collection
            return NewObject(type);
        }

        public static string Fill(string collection, string countCode, Type fillerType)
        {
            string filler = CreationTemplates.NewObject(fillerType);
            return $"{collection}.Fill({countCode}, () => {filler});";
        }

        public static string Fill(string collection, string countCode, string filler)
        {
            return $"{collection}.Fill({countCode}, () => {filler});";
        }


    }
}