using System;

namespace HappyMapper.Text
{
    internal static class CreationTemplates
    {
        /// <summary>
        /// TODO: it won't work for array and tricky collection
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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

        
        public static string Add(string collection, string countCode, Type fillerType)
        {
            string filler = CreationTemplates.NewObject(fillerType);
            return $"{collection}.Add({countCode}, () => {filler});";
        }

        public static string Add(string collection, string countCode, string filler)
        {
            return $"{collection}.Add({countCode}, () => {filler});";
        }


    }
}