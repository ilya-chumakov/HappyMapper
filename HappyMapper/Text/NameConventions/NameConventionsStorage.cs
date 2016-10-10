namespace HappyMapper.Text
{
    public static class NameConventionsStorage
    {
        public static ActionNameConvention BeforeMap { get; set; } = new ActionNameConvention();
        public static ActionNameConvention Condition { get; set; } = new ActionNameConvention();
        public static MapperNameConvention Mapper { get; set; } = new MapperNameConvention();
        public static MapNameConvention Map { get; set; } = new MapNameConvention();
        public static MapCollectionNameConvention MapCollection { get; set; } = new MapCollectionNameConvention();


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

            Map.SrcParam = "src";
            Map.DestParam = "dest";
            Map.Method = "Map";

            MapCollection.SrcParam = "src";
            MapCollection.DestParam = "dest";
            MapCollection.SrcCollection = "srcList";
            MapCollection.DestCollection = "destList";
            MapCollection.Method = "MapCollection";
            MapCollection.CollectionTypeTemplate = "ICollection<{0}>";
        }
    }
}