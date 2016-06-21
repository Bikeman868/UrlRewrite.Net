using System;
using System.Collections.Generic;
using UrlRewrite.Interfaces.Operations;

namespace UrlRewrite.Interfaces.Conditions
{
    public interface IValueConcatenator: IValueGetter
    {
        IValueGetter Initialize(IList<IValueGetter> values, string separator = null, IOperation operation = null);
        IValueGetter Initialize(IValueGetter value, IOperation operation);
    }
}
