namespace OrdinaryMapper
{
    public static class NameConventions
    {
        public static ActionNameConvention BeforeMap { get; set; } = new ActionNameConvention();
        public static ActionNameConvention Condition { get; set; } = new ActionNameConvention();
        public static MapperNameConvention Mapper { get; set; } = new MapperNameConvention();
        

        static NameConventions()
        {
            string nms = "OrdinaryMapper";

            Mapper.Namespace = nms;
            Mapper.ClassShortName = "Mapper";

            BeforeMap.Namespace = nms;
            BeforeMap.ClassShortName = "BeforeMapActionStore";
            BeforeMap.MemberPrefix = "BeforeMapAction_";

            Condition.Namespace = nms;
            Condition.ClassShortName = "ConditionStore";
            Condition.MemberPrefix = "Condition_";
        }
    }
}