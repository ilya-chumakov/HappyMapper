namespace OrdinaryMapper
{
    public class BeforeMapActionNameConvention
    {
        public string Namespace { get; set; }
        public string ClassShortName { get; set; }
        public string MemberPrefix { get; set; }

        public string ClassFullName => $"{Namespace}.{ClassShortName}";

        public string GetMemberShortName(string id)
        {
            return $"{MemberPrefix}{id}";
        }

        public string GetMemberFullName(string id)
        {
            return $"{ClassFullName}.{MemberPrefix}{id}";
        }
    }

    public static class NameConventionConfig
    {
        public static BeforeMapActionNameConvention BeforeMapActionNameConvention { get; set; } = new BeforeMapActionNameConvention();
        

        static NameConventionConfig()
        {
            string nms = "OrdinaryMapper";
            BeforeMapActionNameConvention.Namespace = nms;
            BeforeMapActionNameConvention.ClassShortName = "BeforeMapActionStore";
            BeforeMapActionNameConvention.MemberPrefix = "BeforeMapAction_";
        }
    }
}