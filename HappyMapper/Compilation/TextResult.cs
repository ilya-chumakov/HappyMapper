using System.Collections.Generic;
using System.Collections.Immutable;
using AutoMapper.ConfigurationAPI;
using HappyMapper.Text;

namespace HappyMapper.Compilation
{
    public class TextResult
    {
        public ImmutableDictionary<TypePair, CodeFile> Files { get; set; }
        public HashSet<string> DetectedLocations { get; set; } = new HashSet<string>();
    }
}