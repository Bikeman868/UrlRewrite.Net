using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite.Request
{
    internal class RequestInfo : IRequestInfo
    {
        public HttpApplication Application { get; private set; }
        public HttpContext Context { get; private set; }
        public ExecutionMode ExecutionMode { get; set; }

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

        private int? _queryPos;

        public int QueryPos
        {
            get
            {
                if (!_queryPos.HasValue)
                {
                    var request = Context.Request;
                    _queryPos = request.RawUrl.IndexOf('?');
                }
                return _queryPos.Value;
            }
        }

        private string _originalPathString;

        public string OriginalPathString
        {
            get
            {
                if (ReferenceEquals(_originalPathString, null))
                {
                    _originalPathString = QueryPos < 0
                        ? Context.Request.RawUrl
                        : Context.Request.RawUrl.Substring(0, QueryPos);
                }
                return _originalPathString;
            }
        }

        private List<string> _originalPath;

        public List<string> OriginalPath
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

        private List<string> _newPath;

        public List<string> NewPath
        {
            get
            {
                if (ReferenceEquals(_newPath, null))
                    _newPath = OriginalPath.ToList();
                return _newPath;
            }
            set { _newPath = value; }
        }

        private string _originalParametersString;

        public string OriginalParametersString
        {
            get
            {
                if (ReferenceEquals(_originalParametersString, null))
                {
                    _originalParametersString = QueryPos < 0
                        ? ""
                        : Context.Request.RawUrl.Substring(QueryPos + 1);
                }
                return _originalParametersString;
            }
        }

        private Dictionary<string, List<string>> _originalParameters;
        private Dictionary<string, List<string>> _newParameters;

        private void ParseParameters()
        {
            var parameters = OriginalParametersString
                .Split('&')
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            var originalParameters = new Dictionary<string, List<string>>();
            var newParameters = new Dictionary<string, List<string>>();

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
                    key = parameter.Substring(0, equalsPos).ToLower();
                    value = parameter.Substring(equalsPos + 1);
                }

                List<string> values;
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

        public Dictionary<string, List<string>> OriginalParameters
        {
            get
            {
                if (ReferenceEquals(_originalParameters, null))
                    ParseParameters();
                return _originalParameters;
            }
        }

        public Dictionary<string, List<string>> NewParameters
        {
            get
            {
                if (ReferenceEquals(_newParameters, null))
                    ParseParameters();
                return _newParameters;
            }
            set { _newParameters = value; }
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
        }

        public void PathChanged()
        {
            _newPathString = null;
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
        }

        public void ParametersChanged()
        {
            _newParametersString = null;
        }

    }
}
