using System.Collections.Generic;
using System.Linq;

namespace OrdinaryMapper.Tests.Tools
{
    public class CompareResult
    {
        public CompareResult(string error)
        {
            Errors = new List<string>(new[] { error });
        }

        public CompareResult(List<string> errors)
        {
            Errors = errors;
        }

        public bool Success => Errors != null && !Errors.Any();

        public List<string> Errors { get; set; }
    }
}