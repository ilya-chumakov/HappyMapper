using System.Collections.Generic;
using System.Linq;

namespace HappyMapper.Text
{
    internal class FileBuilderRunner
    {
        public List<Rule> RootRules { get; set; } = new List<Rule>();
        public List<Rule> AllRules { get; set; } = new List<Rule>();

        public Rule Add(IFileBuilder builder)
        {
            var rule = new Rule(this, builder);

            RootRules.Add(rule);
            AllRules.Add(rule);

            return rule;
        }

        public void BuildCode(out List<string> sources, out HashSet<string> locations)
        {
            RootRules.ForEach(rule => rule.Build());

            sources = new List<string>();
            locations = new HashSet<string>();

            foreach (TextResult result in AllRules.Select(rule => rule.Result))
            {
                sources.AddRange(result.SelectSourceCode());

                locations.UnionWith(result.Locations);
            }
        }
    }
}