using System.IO;

namespace UrlRewrite.Interfaces
{
    public interface IRuleParser
    {
        IRuleList Parse(Stream stream);
    }
}
