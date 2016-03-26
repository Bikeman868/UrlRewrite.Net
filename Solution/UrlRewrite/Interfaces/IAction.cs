using System.Web;

namespace UrlRewrite.Interfaces
{
    public interface IAction
    {
        /// <summary>
        /// Performs the redirection, rewrite or whatever action is required
        /// </summary>
        /// <returns>True if stop processing any further actions</returns>
        bool PerformAction(IRequestInfo request);

        /// <summary>
        /// If this returns true then the request receives no more processing
        /// and a response is sent back to the caller.
        /// </summary>
        bool EndRequest { get; }

        /// <summary>
        /// If this returns true then no further actions will be performed for 
        /// this request.
        /// </summary>
        bool StopProcessing { get; }
    }
}
