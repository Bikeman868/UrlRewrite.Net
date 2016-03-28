using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrlRewrite.Interfaces
{
    internal enum MatchPattern
    {
        StartsWith,
        EndsWith,
        Contains,
        Equals,
        MatchWildcard,
        MatchRegex
    }
}
