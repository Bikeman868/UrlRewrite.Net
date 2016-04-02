using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrlRewrite.Interfaces
{
    public interface IStringMatch: ICondition
    {
        IStringMatch Initialize(
            IValueGetter valueGetter,
            CompareOperation compareOperation,
            string match,
            bool inverted = false,
            bool ignoreCase = true);
    }
}
