using System;

namespace HappyMapper.Text
{
    public enum MethodReturnTypeEnum
    {
        Void,
        Dest
    }

    public class MethodDeclarationContext
    {
        public string DestParam { get; }
        public string DestType { get; }
        public string MethodName { get; }
        public string SrcParam { get; }
        public string SrcType { get; }
        public MethodReturnTypeEnum ReturnTypeEnum { get; set; }
        public string ReturnType
        {
            get
            {
                switch (ReturnTypeEnum)
                {
                    case MethodReturnTypeEnum.Void: return "void";
                    case MethodReturnTypeEnum.Dest: return DestType;
                    default:  throw new NotSupportedException(ReturnTypeEnum.ToString());
                }
            }
        }

        public MethodDeclarationContext(
            string methodName,
            string srcType, string destType,
            string srcParam, string destFieldName,
            MethodReturnTypeEnum returnTypeEnum = MethodReturnTypeEnum.Dest)
        {
            MethodName = methodName;
            SrcType = srcType.NormalizeTypeName();
            DestType = destType.NormalizeTypeName();
            SrcParam = srcParam;
            DestParam = destFieldName;
            ReturnTypeEnum = returnTypeEnum;
        }
    }
}