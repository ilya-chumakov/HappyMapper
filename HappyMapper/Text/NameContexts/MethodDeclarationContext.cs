namespace HappyMapper.Text
{
    public class MethodDeclarationContext
    {
        public string DestParam { get; }
        public string DestType { get; }
        public string MethodName { get; }
        public string SrcParam { get; }
        public string SrcType { get; }

        public MethodDeclarationContext(string methodName, string srcType, string destType, string srcParam, string destFieldName)
        {
            MethodName = methodName;
            SrcType = srcType.NormalizeTypeName();
            DestType = destType.NormalizeTypeName();
            SrcParam = srcParam;
            DestParam = destFieldName;
        }
    }
}