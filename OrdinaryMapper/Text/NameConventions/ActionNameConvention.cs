namespace OrdinaryMapper
{
    public class ActionNameConvention
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
}