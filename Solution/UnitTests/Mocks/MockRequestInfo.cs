using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrlRewrite.Interfaces;

namespace UnitTests.Mocks
{
    internal class MockRequestInfo: IRequestInfo
    {
        private System.Web.HttpContext _context;
        private System.Web.HttpApplication _application;
        private ILog _log;

        private readonly string _url;
        private readonly string _scheme;
        private readonly int _port;

        public MockRequestInfo(string url, string scheme = "http", int port = 80)
        {
            _url = url;
            _scheme = scheme;
            _port = port;

            NewUrlString = url;
        }

        public IRequestInfo Initialize(System.Web.HttpApplication application, ILog log)
        {
            _application = application;
            _context = application == null ? null : application.Context;
            _log = log;
            return this;
        }

        public System.Web.HttpApplication Application
        {
            get { return _application; }
        }

        public System.Web.HttpContext Context
        {
            get { return _context; }
        }

        public IRequestLog Log
        {
            get { return _log.GetRequestLog(_application, _context); }
        }

        public ExecutionMode ExecutionMode { get; set; }
        public IList<Action<IRequestInfo>> DeferredActions { get; set; }

        public string OriginalUrlString { get { return _url; } }

        public string OriginalPathString
        {
            get
            {
                var originalUrlString = OriginalUrlString;
                var q = originalUrlString.IndexOf('?');
                if (q < 0) return originalUrlString;
                return originalUrlString.Substring(0, q);
            }
        }

        public IList<string> OriginalPath
        {
            get
            {
                return OriginalPathString.Split('/').ToList();
            }
        }

        public string OriginalParametersString
        {
            get
            {
                var q = _url.IndexOf('?');
                if (q < 0) return String.Empty;
                return _url.Substring(q + 1);
            }
        }

        public IDictionary<string, IList<string>> OriginalParameters
        {
            get
            {
                var originalParameters = new Dictionary<string, IList<string>>();

                var originalParametersString = OriginalParametersString;
                if (string.IsNullOrEmpty(originalParametersString))
                    return originalParameters;

                var parameters = originalParametersString
                    .Split('&')
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                foreach (var parameter in parameters)
                {
                    string key;
                    string parameterValue = null;
                    var equalsPos = parameter.IndexOf('=');
                    if (equalsPos < 0)
                    {
                        key = parameter.ToLower();
                    }
                    else
                    {
                        key = parameter.Substring(0, equalsPos).Trim().ToLower();
                        parameterValue = parameter.Substring(equalsPos + 1).Trim();
                    }

                    IList<string> values;
                    if (originalParameters.TryGetValue(key, out values))
                    {
                        values.Add(parameterValue);
                    }
                    else
                    {
                        originalParameters.Add(key, new List<string> { parameterValue });
                    }
                }
                return originalParameters;
            }
        }

        public string NewUrlString
        {
            get 
            { 
                var result = NewPathString;
                var query = NewParametersString;
                if (!string.IsNullOrEmpty(query))
                    result += "?" + query;
                return result;
            }
            set 
            { 
                var q = value.IndexOf('?');
                if (q < 0)
                {
                    NewPathString = value;
                    NewParametersString = string.Empty;
                }
                else
                {
                    NewPathString = value.Substring(0, q);
                    NewParametersString = value.Substring(q + 1);
                }
            }
        }

        public string NewPathString 
        { 
            get { return string.Join("/", NewPath); }
            set { NewPath = value.Split('/').ToList(); }
        }

        public string NewParametersString 
        { 
            get
            {
                var result = string.Empty;
                if (NewParameters != null)
                {
                    foreach (var key in NewParameters.Keys)
                    {
                        var parameterList = NewParameters[key];
                        if (parameterList == null || parameterList.Count == 0)
                        {
                            if (result.Length > 0) result += '&';
                            result += key + "=";
                        }
                        else
                        {
                            foreach (var parameterValue in parameterList)
                            {
                                if (result.Length > 0) result += '&';
                                result += key + "=" + parameterValue;
                            }
                        }
                    }
                }
                return result;
            }
            set 
            { 
                NewParameters = new Dictionary<string, IList<string>>();
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (var parameter in value.Split('&'))
                    {
                        var equals = parameter.IndexOf('=');
                        string key;
                        string parameterValue;
                        if (equals < 0)
                        {
                            key = parameter;
                            parameterValue = string.Empty;
                        }
                        else
                        {
                            key = parameter.Substring(0, equals);
                            parameterValue = parameter.Substring(equals + 1);
                        }

                        IList<string> valueList;
                        if (NewParameters.TryGetValue(key, out valueList))
                        {
                            valueList.Add(parameterValue);
                        }
                        else
                        {
                            NewParameters.Add(key, new List<string> { parameterValue });
                        }
                    }
                }
            } 
        }

        public IList<string> NewPath { get; set; }

        public IDictionary<string, IList<string>> NewParameters { get; set; }

        public void PathChanged()
        {
        }

        public void ParametersChanged()
        {
        }

        public void ExecuteDeferredActions()
        {
        }
    }
}
