using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UrlRewrite.Interfaces
{
    public interface IDescribable
    {
        void Describe(TextWriter writer, string indent, string indentText);
    }
}
