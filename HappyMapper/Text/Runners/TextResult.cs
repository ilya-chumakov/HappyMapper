using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMapper.ConfigurationAPI;

namespace HappyMapper.Text
{
    internal class TextResult
    {
        public TextResult(ImmutableDictionary<TypePair, CodeFile> files, HashSet<string> locations)
        {
            Files = files;
            Locations = locations;
        }

        public ImmutableDictionary<TypePair, CodeFile> Files { get; set; }
        public HashSet<string> Locations { get; set; } = new HashSet<string>();

        public List<string> SelectSourceCode()
        {
            return Files.Values.Select(x => x.Code).ToList();
        }
    }
}