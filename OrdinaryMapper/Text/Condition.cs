using System;

namespace OrdinaryMapper
{
    public class Condition : IDisposable
    {
        public Condition(PropertyNameContext context, Coder coder)
        {
            Context = context;
            Coder = coder;

            var condition = Context.PropertyMap.Condition;

            IsExist = condition != null;

            if (IsExist)
            {
                Coder.AttachRawCode("{{");
            }
        }

        public bool IsExist { get; set; } = false;

        protected PropertyNameContext Context { get; set; }
        protected Coder Coder { get; set; }

        public void Dispose()
        {
            if (IsExist)
            {
                Coder.AttachRawCode("}}");
            }
        }
    }
}