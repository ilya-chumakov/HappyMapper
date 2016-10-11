using AutoMapper.ConfigurationAPI;

namespace HappyMapper.Text
{
    public class TextBuilderHelper
    {
        public static CodeFile CreateFile(
            TypePair typePair, string methodCode, string methodName, Assignment assignment = default(Assignment))
        {
            var Convention = NameConventionsStorage.Mapper;

            string shortClassName = Convention.CreateUniqueMapperMethodNameWithGuid(typePair);
            string fullClassName = $"{Convention.Namespace}.{shortClassName}";

            string classCode = StatementTemplates.Class(methodCode, Convention.Namespace, shortClassName);

            return new CodeFile(classCode, fullClassName, methodName, typePair, assignment);
        }
    }
}