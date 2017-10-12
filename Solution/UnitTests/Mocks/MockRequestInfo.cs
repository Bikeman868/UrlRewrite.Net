using System;
using System.Collections.Generic;
using System.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Utilities;

namespace UnitTests.Mocks
{
    public class MockRequestInfo: IRequestInfo
    {
        private System.Web.HttpContext _context;
        private System.Web.HttpApplication _application;
        private ILog _log;

        private IDictionary<string, string> _originalServerVariables;
        private IDictionary<string, string> _serverVariables;
        private IDictionary<string, string> _originalHeaders;
        private IDictionary<string, string> _headers;

        private readonly string _url;

        public MockRequestInfo(
            string url, 
            string scheme = "http", 
            string host = "test.com",
            int port = 80,
            IDictionary<string, string> serverVariables = null,
            IDictionary<string, string> headers = null)
        {
            _url = url;
            NewUrlString = url;

            _serverVariables = serverVariables ?? new Dictionary<string, string>();

            _serverVariables["URL"] = url;
            _serverVariables["PATH_INFO"] = NewPathString;
            _serverVariables["QUERY_STRING"] = NewParametersString;
            _serverVariables["SERVER_PORT"] = port.ToString();
            _serverVariables["SERVER_PORT_SECURE"] = scheme == "https" ? "1" : "0";

            _headers = headers ?? new Dictionary<string, string>();

            _headers["HOST"] = host;
            _headers["USER_AGENT"] = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US)";
            _headers["CONNECTION"] = "Keep-Alive";

            _originalServerVariables = _serverVariables.ToDictionary(e => e.Key, e => e.Value);
            _originalHeaders = _headers.ToDictionary(e => e.Key, e => e.Value);

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
        public bool UrlIsModified { get; private set; }

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
                UrlIsModified = true;
            }
        }

        public string NewPathString 
        { 
            get { return string.Join("/", NewPath); }
            set 
            { 
                NewPath = value.Split('/').ToList();
                UrlIsModified = true;
            }
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
                UrlIsModified = true;
            } 
        }

        private IList<string> _newPath;
        public IList<string> NewPath 
        {
            get { return _newPath; }
            set
            {
                _newPath = value;
                UrlIsModified = true;
            }
        }

        private IDictionary<string, IList<string>> _newParameters;

        public IDictionary<string, IList<string>> NewParameters
        {
            get { return _newParameters; }
            set
            {
                _newParameters = value;
                UrlIsModified = true;
            }
        }

        public void PathChanged()
        {
            UrlIsModified = true;
        }

        public void ParametersChanged()
        {
            UrlIsModified = true;
        }

        public void ExecuteDeferredActions()
        {
        }

        public string GetOriginalServerVariable(string name)
        {
            string value;
            return _originalServerVariables.TryGetValue(name, out value) ? value : string.Empty;
        }

        public string GetOriginalHeader(string name)
        {
            string value;
            return _originalHeaders.TryGetValue(name, out value) ? value : string.Empty;
        }

        public string GetServerVariable(string name)
        {
            string value;
            return _serverVariables.TryGetValue(name, out value) ? value : string.Empty;
        }

        public string GetHeader(string name)
        {
            string value;
            return _headers.TryGetValue(name, out value) ? value : string.Empty;
        }

        public void SetServerVariable(string name, string value)
        {
            _serverVariables[name] = value;
        }

        public void SetHeader(string name, string value)
        {
            _headers[name] = value;
        }

        public IEnumerable<string> GetHeaderNames()
        {
            return _headers.Keys;
        }
    }
}
