using System.IO;
using System.Text;

namespace UrlRewrite.Interfaces.Rules
{
    public interface IRuleParser
    {
        IRuleList Parse(Stream stream, Encoding encoding);
    }
}
