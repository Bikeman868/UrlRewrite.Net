using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrlRewrite.Interfaces
{
    public interface INumberMatch: ICondition
    {
        INumberMatch Initialize(
            IValueGetter valueGetter,
            CompareOperation compareOperation,
            int match,
            bool inverted = false,
            int defaultValue = 0);
    }
}
