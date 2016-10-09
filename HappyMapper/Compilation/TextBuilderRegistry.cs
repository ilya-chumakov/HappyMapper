using System.Collections.Generic;
using System.Linq;
using HappyMapper.Text;

namespace HappyMapper.Compilation
{
    public class TextBuilderRegistry
    {
        public Rule Add(ITextBuilder builder)
        {
            var node = new Rule(this, builder);

            RootNodes.Add(node);
            AllNodes.Add(node);

            return node;
        }

        public List<Rule> RootNodes { get; set; } = new List<Rule>();
        public List<Rule> AllNodes { get; set; } = new List<Rule>();

        public void GetCode(out List<string> sources, out HashSet<string> locations)
        {
            var list = RootNodes[0].Build();

            sources = new List<string>();

            foreach (var result in list)
            {
                List<string> codes = result.Files.Values.Select(x => x.Code).ToList();

                sources.AddRange(codes);
            }

            locations = list.First(l => l.DetectedLocations!= null && l.DetectedLocations.Any()).DetectedLocations;
        }
    }
}