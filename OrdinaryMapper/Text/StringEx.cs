namespace OrdinaryMapper
{
    public static class StringEx
    {
        /// <summary>
        /// Remove +
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static string NormalizeTypeName(this string typeName)
        {
            return typeName.Replace('+', '.');
        }

        public static string TemplateToCode(this string template, string src, string dest)
        {
            return template.Replace("{0}", src).Replace("{1}", dest);
        }


    }
}