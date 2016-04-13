using System.IO;

namespace UrlRewrite.Interfaces
{
    public interface IDescribable
    {
        void Describe(TextWriter writer, string indent, string indentText);
    }
}
