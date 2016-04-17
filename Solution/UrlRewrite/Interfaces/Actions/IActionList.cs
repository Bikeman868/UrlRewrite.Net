using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IActionList: IAction
    {
        IActionList Initialize(bool stopProcessing = false, bool endRequest = false);
        IActionList Add(IAction action);
    }
}
