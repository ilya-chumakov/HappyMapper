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
        public static BeforeMapActionNameConvention BeforeMap { get; set; } = new BeforeMapActionNameConvention();
        public static BeforeMapActionNameConvention Condition { get; set; } = new BeforeMapActionNameConvention();
        

        static NameConventionConfig()
        {
            string nms = "OrdinaryMapper";
            BeforeMap.Namespace = nms;
            BeforeMap.ClassShortName = "BeforeMapActionStore";
            BeforeMap.MemberPrefix = "BeforeMapAction_";

            Condition.Namespace = nms;
            Condition.ClassShortName = "ConditionStore";
            Condition.MemberPrefix = "Condition_";
        }
    }
}