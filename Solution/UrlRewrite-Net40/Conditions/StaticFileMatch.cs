using System;
using System.IO;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Conditions
{
    internal class StaticFileMatch : IStaticFileMatch
    {
        private IValueGetter _valueGetter;
        private bool _inverted;
        private bool _isDirectory;
        private Func<string, bool> _testFunc;

        public IStaticFileMatch Initialize(
            IValueGetter valueGetter,
            bool isDirectory,
            bool inverted = false)
        {
            _valueGetter = valueGetter;
            _inverted = inverted;
            _isDirectory = isDirectory;
            _testFunc = isDirectory ? (Func<string, bool>)IsDirectory : IsStaticFile;
            return this;
        }

        private bool IsDirectory(string filePath)
        {
            return Directory.Exists(filePath);
        }

        private bool IsStaticFile(string filePath)
        {
            return File.Exists(filePath);
        }

        public bool Test(IRequestInfo request, IRuleResult ruleResult)
        {
            var path = _valueGetter.GetString(request, ruleResult);
            try
            {
                if (!Path.IsPathRooted(path))
                    path = request.Application.Server.MapPath(path);

                return _inverted ? !_testFunc(path) : _testFunc(path);
            }
            catch
            {
                // When there is a permissions issue we can't tell if it exists or not
                return false;
            }
        }

        public override string ToString()
        {
            var description = "request " + _valueGetter + (_isDirectory ? " directory" : " file");
            description += (_inverted ? " does not exist" : "exists") + " on disk";
            return description;
        }

        public ICondition Initialize(XElement configuration, IValueGetter valueGetter)
        {
            return this;
        }

        public string ToString(IRequestInfo request)
        {
            return ToString();
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.WriteLine(indent + "If " + ToString());
        }
    }
}
