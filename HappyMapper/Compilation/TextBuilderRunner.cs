using System.Collections.Generic;
using System.Linq;
using HappyMapper.Text;

namespace HappyMapper.Compilation
{
    public class TextBuilderRunner
    {
        public Rule Add(ITextBuilder builder)
        {
            var rule = new Rule(this, builder);

            RootRules.Add(rule);
            AllRules.Add(rule);

            return rule;
        }

        public List<Rule> RootRules { get; set; } = new List<Rule>();
        public List<Rule> AllRules { get; set; } = new List<Rule>();

        public void GetCode(out List<string> sources, out HashSet<string> locations)
        {
            RootRules.ForEach(rule => rule.Build());

            sources = new List<string>();
            locations = new HashSet<string>();

            foreach (TextResult result in AllRules.Select(rule => rule.Result))
            {
                sources.AddRange(result.SelectSourceCode());

                locations.UnionWith(result.DetectedLocations);
            }
        }
    }
}