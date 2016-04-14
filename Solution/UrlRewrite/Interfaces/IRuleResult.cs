using System.Collections.Generic;

namespace UrlRewrite.Interfaces
{
    public interface IRuleResult
    {
        /// <summary>
        /// When set to true, no more rules within this rule list should be eveluated
        /// </summary>
        bool StopProcessing { get; }

        /// <summary>
        /// When set to true, a response has already been sent back to the client and 
        /// this request must not be passed on to the web site for processing
        /// </summary>
        bool EndRequest { get; }

        /// <summary>
        /// Indicates that if you execute this rule again with the same input URL that
        /// the results could be different therefore the results can not be cached
        /// and reused. Rules should be flagged as dynamic if they use other information
        /// such as cookies and database lookups to rewrite the request.
        /// </summary>
        bool IsDynamic { get; }

        /// <summary>
        /// A list of actions to execute. Only populated if this rule matched the
        /// incomming request.
        /// </summary>
        List<IAction> Actions { get; }

        /// <summary>
        /// This is used to pass information between rule elements. For example
        /// one action might set a value and another might act upon it. One way
        /// that this is used is to support backreferences in curly braces, in
        /// this case the Regex conditions store the match groups into the
        /// request properties so that the rewrite action can reference them
        /// </summary>
        IPropertyBag Properties { get; }
    }
}
