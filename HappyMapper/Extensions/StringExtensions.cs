namespace HappyMapper
{
    internal static class StringExtensions
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

        public static string RemoveDoubleBraces(this string str)
        {
            return str.Replace("{{", "").Replace("}}", "");
        }

        /// <summary>
        /// .Replace("{0}", "{0}." + srcName)
        /// </summary>
        /// <param name="template"></param>
        /// <param name="srcName"></param>
        /// <param name="destName"></param>
        /// <returns></returns>
        public static string AddPropertyNamesToTemplate(this string template, string srcName, string destName)
        {
            return template
                .Replace("{0}", "{0}." + srcName)
                .Replace("{1}", "{1}." + destName);
        }
    }
}