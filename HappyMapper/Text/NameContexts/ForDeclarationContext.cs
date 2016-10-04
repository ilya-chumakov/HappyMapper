namespace HappyMapper.Text
{
    public class ForDeclarationContext
    {
        public string SrcVariable { get; set; }
        public string SrcCollection { get; set; }
        public string DestVariable { get; set; }
        public string DestCollection { get; set; }

        public ForDeclarationContext(string srcCollection, string destCollection, string srcVariable, string destVariable)
        {
            SrcVariable = srcVariable;
            SrcCollection = srcCollection;
            DestVariable = destVariable;
            DestCollection = destCollection;
        }
    }
}