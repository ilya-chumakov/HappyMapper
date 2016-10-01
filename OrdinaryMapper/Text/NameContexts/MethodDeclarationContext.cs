namespace OrdinaryMapper
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
            this.MethodName = methodName;
            this.SrcType = srcType.NormalizeTypeName();
            this.DestType = destType.NormalizeTypeName();
            this.SrcParam = srcParam;
            this.DestParam = destFieldName;
        }
    }
}