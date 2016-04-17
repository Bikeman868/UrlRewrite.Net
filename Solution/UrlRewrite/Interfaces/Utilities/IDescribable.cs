using System.IO;

namespace UrlRewrite.Interfaces.Utilities
{
    public interface IDescribable
    {
        void Describe(TextWriter writer, string indent, string indentText);
    }
}
