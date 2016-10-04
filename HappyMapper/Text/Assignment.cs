namespace HappyMapper.Text
{
    public struct Assignment
    {
        public string RelativeTemplate { get; set; }

        public string GetCode(string src, string dest)
        {
            return RelativeTemplate.TemplateToCode(src, dest).RemoveDoubleBraces();
        }
    }
}