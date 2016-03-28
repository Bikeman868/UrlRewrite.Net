using System.Text;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class Action
    {
        protected bool _stopProcessing;
        protected bool _endRequest;

        protected string BuildNewUrl(IRequestInfo requestInfo)
        {
            var sb = new StringBuilder(1024);

            if (requestInfo.NewPath != null && requestInfo.NewPath.Count > 0)
            {
                var first = true;
                foreach (var pathElement in requestInfo.NewPath)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append('/');
                    sb.Append(pathElement);
                }
            }
            else
            {
                sb.Append('/');
            }

            if (requestInfo.NewParameters != null && requestInfo.NewParameters.Count > 0)
            {
                var first = true;
                foreach (var param in requestInfo.NewParameters)
                {
                    if (param.Value != null && param.Value.Count > 0)
                    {
                        foreach (var value in param.Value)
                        {
                            sb.Append(first ? '?' : '&');
                            sb.Append(param.Key);
                            sb.Append('=');
                            sb.Append(value);
                            first = false;
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}
