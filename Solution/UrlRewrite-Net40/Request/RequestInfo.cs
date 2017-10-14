using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Utilities;
using UrlRewrite.Utilities;

namespace UrlRewrite.Request
{
    public class RequestInfo : IRequestInfo
    {
        public HttpApplication Application { get; private set; }
        public HttpContext Context { get; private set; }
        public ExecutionMode ExecutionMode { get; set; }
        public bool UrlIsModified { get; private set; }

        private ILog _log;
        private IRequestLog _requestLog;

        public IRequestLog Log
        {
            get
            {
                if (ReferenceEquals(_requestLog, null))
                    _requestLog = _log.GetRequestLog(Application, Context) ?? new RequestLog();
                return _requestLog;
            }
        }

        private IList<System.Action<IRequestInfo>> _deferredActions;

        public IList<System.Action<IRequestInfo>> DeferredActions
        {
            get
            {
                if (ReferenceEquals(_deferredActions, null))
                    _deferredActions = new List<System.Action<IRequestInfo>>();
                return _deferredActions;
            }
        }

        private string _originalUrlString;

        public string OriginalUrlString
        {
            get
            {
                if (ReferenceEquals(_originalPathString, null))
                    _originalUrlString = Context.Request.RawUrl;
                return _originalUrlString;
            }
        }

        private int? _originalQueryPos;

        public int OriginalQueryPos
        {
            get
            {
                if (!_originalQueryPos.HasValue)
                {
                    _originalQueryPos = OriginalUrlString.IndexOf('?');
                }
                return _originalQueryPos.Value;
            }
        }

        private string _originalPathString;

        public string OriginalPathString
        {
            get
            {
                if (ReferenceEquals(_originalPathString, null))
                {
                    _originalPathString = OriginalQueryPos < 0
                        ? OriginalUrlString
                        : OriginalUrlString.Substring(0, OriginalQueryPos);
                }
                return _originalPathString;
            }
        }

        private IList<string> _originalPath;

        public IList<string> OriginalPath
        {
            get
            {
                if (ReferenceEquals(_originalPath, null))
                {
                    _originalPath = OriginalPathString
                        .Split('/')
                        .Where(e => !string.IsNullOrEmpty(e))
                        .ToList();
                    if (OriginalPathString.StartsWith("/"))
                        _originalPath.Insert(0, "");
                }
                return _originalPath;
            }
        }

        private IList<string> _newPath;

        public IList<string> NewPath
        {
            get
            {
                if (ReferenceEquals(_newPath, null))
                    _newPath = OriginalPath.ToList();
                return _newPath;
            }
            set 
            { 
                _newPath = value;
                _newPathString = null;
                UrlIsModified = true;
            }
        }

        private string _originalParametersString;

        public string OriginalParametersString
        {
            get
            {
                if (ReferenceEquals(_originalParametersString, null))
                {
                    _originalParametersString = OriginalQueryPos < 0
                        ? ""
                        : Context.Request.RawUrl.Substring(OriginalQueryPos + 1);
                }
                return _originalParametersString;
            }
        }

        private IDictionary<string, IList<string>> _originalParameters;
        private IDictionary<string, IList<string>> _newParameters;

        private void ParseParameters()
        {
            var parameters = OriginalParametersString
                .Split('&')
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            var originalParameters = new Dictionary<string, IList<string>>();
            var newParameters = new Dictionary<string, IList<string>>();

            foreach (var parameter in parameters)
            {
                string key;
                string value = null;
                var equalsPos = parameter.IndexOf('=');
                if (equalsPos < 0)
                {
                    key = parameter.ToLower();
                }
                else
                {
                    key = parameter.Substring(0, equalsPos).Trim().ToLower();
                    value = parameter.Substring(equalsPos + 1).Trim();
                }

                IList<string> values;
                if (originalParameters.TryGetValue(key, out values))
                {
                    values.Add(value);
                    newParameters[key].Add(value);
                }
                else
                {
                    originalParameters.Add(key, new List<string> { value });
                    newParameters.Add(key, new List<string> { value });
                }
            }

            _originalParameters = originalParameters;
            _newParameters = newParameters;
        }

        public IDictionary<string, IList<string>> OriginalParameters
        {
            get
            {
                if (ReferenceEquals(_originalParameters, null))
                    ParseParameters();
                return _originalParameters;
            }
        }

        public IDictionary<string, IList<string>> NewParameters
        {
            get
            {
                if (ReferenceEquals(_newParameters, null))
                    ParseParameters();
                return _newParameters;
            }
            set 
            { 
                _newParameters = value;
                _newParametersString = null;
                UrlIsModified = true;
            }
        }

        public IRequestInfo Initialize(
            HttpApplication application,
            ILog log)
        {
            Application = application;
            Context = application.Context;
            _log = log;
            ExecutionMode = ExecutionMode.ExecuteOnly;
            return this;
        }

        public void ExecuteDeferredActions()
        {
            if (ReferenceEquals(_deferredActions, null)) return;
            foreach (var action in _deferredActions)
                action(this);
        }

        public string NewUrlString
        {
            get
            {
                var path = NewPathString;
                var query = NewParametersString;
                if (string.IsNullOrEmpty(query))
                    return path;
                return path + "?" + query;
            }
            set 
            { 
                var queryPos = value.IndexOf('?');
                if (queryPos < 0)
                {
                    NewPathString = value;
                    NewParametersString = string.Empty;
                }
                else
                {
                    NewPathString = value.Substring(0, queryPos);
                    NewParametersString = value.Substring(queryPos + 1);
                }
            }
        }

        private string _newPathString;

        public string NewPathString
        {
            get
            {
                if (_newPathString == null)
                {
                    var sb = new StringBuilder(1024);

                    if (NewPath != null && NewPath.Count > 0)
                    {
                        var first = true;
                        foreach (var pathElement in NewPath)
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
                    _newPathString = sb.ToString();
                }
                return _newPathString;
            }
            set 
            { 
                _newPathString = value;
                _newPath = value
                    .Split('/')
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();
                if (value.StartsWith("/"))
                    _newPath.Insert(0, "");
                UrlIsModified = true;
            }
        }

        public void PathChanged()
        {
            _newPathString = null;
            UrlIsModified = true;
        }

        private string _newParametersString;

        public string NewParametersString
        {
            get
            {
                if (_newParametersString == null)
                {
                    if (NewParameters == null || NewParameters.Count == 0)
                    {
                        _newParametersString = string.Empty;
                    }
                    else
                    {
                        var sb = new StringBuilder(1024);
                        var first = true;
                        foreach (var param in NewParameters)
                        {
                            if (param.Value != null && param.Value.Count > 0)
                            {
                                foreach (var value in param.Value)
                                {
                                    if (!first) sb.Append('&');
                                    sb.Append(param.Key);
                                    sb.Append('=');
                                    sb.Append(value);
                                    first = false;
                                }
                            }
                        }
                        _newParametersString = sb.ToString();
                    }
                }
                return _newParametersString;
            }
            set 
            { 
                _newParametersString = value ?? string.Empty;
                var parameters = value
                    .Split('&')
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();
                _newParameters = new Dictionary<string, IList<string>>();
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
                    if (_newParameters.TryGetValue(key, out values))
                    {
                        values.Add(parameterValue);
                    }
                    else
                    {
                        _newParameters.Add(key, new List<string> { parameterValue });
                    }
                }
                UrlIsModified = true;
            }
        }

        public void ParametersChanged()
        {
            _newParametersString = null;
            UrlIsModified = true;
        }

        private IDictionary<string, string> _originalServerVraiables;
        private IDictionary<string, string> _originalHeaders;

        public string GetOriginalServerVariable(string name)
        {
            if (ReferenceEquals(_originalServerVraiables, null))
                return Context.Request.ServerVariables[name];

            string value;
            return _originalServerVraiables.TryGetValue(name, out value) ? value : string.Empty;
        }

        public string GetOriginalHeader(string name)
        {
            if (ReferenceEquals(_originalHeaders, null))
                return Context.Request.Headers[name];

            string value;
            return _originalHeaders.TryGetValue(name, out value) ? value : string.Empty;
        }

        public string GetServerVariable(string name)
        {
            return Context.Request.ServerVariables[name];
        }

        public string GetHeader(string name)
        {
            return Context.Request.Headers[name];
        }

        public void SetServerVariable(string name, string value)
        {
            if (ReferenceEquals(_originalServerVraiables, null))
            {
                _originalServerVraiables = new Dictionary<string, string>();
                foreach (string serverVariable in Context.Request.ServerVariables)
                    _originalServerVraiables[serverVariable] = Context.Request.ServerVariables[serverVariable];
            }
            Context.Request.ServerVariables[name] = value;
        }

        public void SetHeader(string name, string value)
        {
            if (ReferenceEquals(_originalHeaders, null))
            {
                _originalHeaders = new Dictionary<string, string>();
                foreach (string header in Context.Request.Headers)
                    _originalHeaders[header] = Context.Request.Headers[header];
            }
            Context.Request.Headers[name] = value;
        }

        public IEnumerable<string> GetHeaderNames()
        {
            return Context.Request.Headers.AllKeys;
        }
    }
}
