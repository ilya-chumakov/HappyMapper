namespace HappyMapper.Text
{
    public static class NameConventionsStorage
    {
        public static ActionNameConvention BeforeMap { get; set; } = new ActionNameConvention();
        public static ActionNameConvention Condition { get; set; } = new ActionNameConvention();
        public static MapperNameConvention Mapper { get; set; } = new MapperNameConvention();
        

        static NameConventionsStorage()
        {
            string nms = "HappyMapper";

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