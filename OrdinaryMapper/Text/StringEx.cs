namespace OrdinaryMapper
{
    public static class StringEx
    {
        public static string NormalizeTypeName(this string typeName)
        {
            return typeName.Replace('+', '.');
        }
    }
}