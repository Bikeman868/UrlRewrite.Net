using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Mocks;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UnitTests
{
    [TestClass]
    public class ValueGetterTests
    {
        private IValueGetter _valueGetter;
        private IRuleResult _ruleResult;
        private IRequestInfo _request1;
        private IRequestInfo _request2;
        private IRequestInfo _request3;

        [TestInitialize]
        public void Initialize()
        {
            _request1 = new MockRequestInfo("/path1/path2?param=value", "http", "test.com", 80);
            _request2 = new MockRequestInfo("/path1/path2", "https", "secure.test.com", 443);
            _request3 = new MockRequestInfo("/path1/path2/", "http", "test.com", 80);

            _request1.NewPath[1] = "changed1";
            _request1.NewParameters["param"] = new List<string> {"changed"};
            _request1.PathChanged();
            _request1.SetServerVariable("PATH_INFO", "/company/news/");
            _request1.SetHeader("HOST", "www.mysite.com");

            _request2.NewPath[2] = "changed2";
            _request2.PathChanged();
            _request2.SetServerVariable("SERVER_PORT", "443");
            _request2.SetHeader("USER_AGENT", "blah blah blah");

            _request3.NewUrlString = "/changed1/changed2/";
            _request3.NewParameters["param"] = new List<string> { "added" };
            _request3.ParametersChanged();

            IFactory factory = new DefaultFactory();
            _valueGetter = factory.Create<IValueGetter>();
            _ruleResult = factory.Create<IRuleResult>();
        }

        [TestMethod]
        public void ShouldGetVariousScopesAsStrings()
        {
            _valueGetter.Initialize(Scope.OriginalUrl);
            Assert.AreEqual(_request1.OriginalUrlString, _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.OriginalUrlString, _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.OriginalUrlString, _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.Url);
            Assert.AreEqual(_request1.NewUrlString, _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.NewUrlString, _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.NewUrlString, _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.OriginalPath);
            Assert.AreEqual(_request1.OriginalPathString, _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.OriginalPathString, _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.OriginalPathString, _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.Path);
            Assert.AreEqual(_request1.NewPathString, _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.NewPathString, _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.NewPathString, _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.OriginalQueryString);
            Assert.AreEqual(_request1.OriginalParametersString, _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.OriginalParametersString, _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.OriginalParametersString, _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.QueryString);
            Assert.AreEqual(_request1.NewParametersString, _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.NewParametersString, _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.NewParametersString, _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.OriginalPathElement, 1);
            Assert.AreEqual(_request1.OriginalPath[1], _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.OriginalPath[1], _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.OriginalPath[1], _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.OriginalPathElement, -1);
            Assert.AreEqual("path2", _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual("path2", _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual("path2", _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.PathElement, 1);
            Assert.AreEqual(_request1.NewPath[1], _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.NewPath[1], _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.NewPath[1], _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.PathElement, -1);
            Assert.AreEqual("path2", _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual("changed2", _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual("changed2", _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.OriginalParameter, "param");
            Assert.AreEqual("value", _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual("", _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual("", _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.Parameter, "param");
            Assert.AreEqual("changed", _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual("", _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual("added", _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.Header, "HOST");
            Assert.AreEqual(_request1.GetHeader("HOST"), _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.GetHeader("HOST"), _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.GetHeader("HOST"), _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.OriginalHeader, "USER_AGENT");
            Assert.AreEqual(_request1.GetOriginalHeader("USER_AGENT"), _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.GetOriginalHeader("USER_AGENT"), _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.GetOriginalHeader("USER_AGENT"), _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.ServerVariable, "URL");
            Assert.AreEqual(_request1.GetServerVariable("URL"), _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.GetServerVariable("URL"), _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.GetServerVariable("URL"), _valueGetter.GetString(_request3, _ruleResult));

            _valueGetter.Initialize(Scope.OriginalServerVariable, "SERVER_PORT");
            Assert.AreEqual(_request1.GetOriginalServerVariable("SERVER_PORT"), _valueGetter.GetString(_request1, _ruleResult));
            Assert.AreEqual(_request2.GetOriginalServerVariable("SERVER_PORT"), _valueGetter.GetString(_request2, _ruleResult));
            Assert.AreEqual(_request3.GetOriginalServerVariable("SERVER_PORT"), _valueGetter.GetString(_request3, _ruleResult));

            _ruleResult.Properties.Set<IList<string>>(new List<string> { "match0", "match1", "match2" }, "R");
            _ruleResult.Properties.Set<IList<string>>(new List<string> { "condition0", "condition1" }, "C");

            _valueGetter.Initialize(Scope.MatchGroup, "0");
            Assert.AreEqual("match0", _valueGetter.GetString(_request1, _ruleResult));

            _valueGetter.Initialize(Scope.MatchGroup, "1");
            Assert.AreEqual("match1", _valueGetter.GetString(_request1, _ruleResult));

            _valueGetter.Initialize(Scope.MatchGroup, "2");
            Assert.AreEqual("match2", _valueGetter.GetString(_request1, _ruleResult));

            _valueGetter.Initialize(Scope.ConditionGroup, "0");
            Assert.AreEqual("condition0", _valueGetter.GetString(_request1, _ruleResult));

            _valueGetter.Initialize(Scope.ConditionGroup, "1");
            Assert.AreEqual("condition1", _valueGetter.GetString(_request1, _ruleResult));

            _valueGetter.Initialize(Scope.ConditionGroup, "2");
            Assert.AreEqual("", _valueGetter.GetString(_request1, _ruleResult));
        }
       
        [TestMethod]
        public void ShouldGetVariousScopesAsNumbers()
        {
            var request = new MockRequestInfo(
                "/1/2?param1=3&param2=4&param1=5",
                "http",
                "www.test.com",
                80);

            request.NewPath[1] = "6";
            request.PathChanged();

            request.NewParameters.Add("param3", new List<string>{"7"});
            request.ParametersChanged();

            _valueGetter.Initialize(Scope.OriginalPathElement, 1);
            Assert.AreEqual(1, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.OriginalPathElement, 2);
            Assert.AreEqual(2, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.PathElement, 1);
            Assert.AreEqual(6, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.PathElement, 2);
            Assert.AreEqual(2, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.PathElement, -1);
            Assert.AreEqual(2, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.PathElement, -2);
            Assert.AreEqual(6, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.OriginalParameter, "param1");
            Assert.AreEqual(3, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.OriginalParameter, "param2");
            Assert.AreEqual(4, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.OriginalParameter, "param3");
            Assert.AreEqual(0, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.Parameter, "param1");
            Assert.AreEqual(3, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.Parameter, "param2");
            Assert.AreEqual(4, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.Parameter, "param3");
            Assert.AreEqual(7, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.Parameter, "param4");
            Assert.AreEqual(0, _valueGetter.GetInt(request, _ruleResult, 0));

            _valueGetter.Initialize(Scope.ServerVariable, "SERVER_PORT");
            Assert.AreEqual(80, _valueGetter.GetInt(request, _ruleResult, 0));
        }

        [TestMethod]
        public void ShouldGetDefaultNumberValues()
        {
            var request = new MockRequestInfo("/1/2?param1=3&param2=4&param1=5");
            request.NewPath[1] = "6";
            request.NewParameters.Add("param3", new List<string> { "7" });
            request.ParametersChanged();

            _valueGetter.Initialize(Scope.OriginalPathElement, 3);
            Assert.AreEqual(999, _valueGetter.GetInt(request, _ruleResult, 999));

            _valueGetter.Initialize(Scope.OriginalPathElement, -4);
            Assert.AreEqual(888, _valueGetter.GetInt(request, _ruleResult, 888));

            _valueGetter.Initialize(Scope.PathElement, 3);
            Assert.AreEqual(777, _valueGetter.GetInt(request, _ruleResult, 777));

            _valueGetter.Initialize(Scope.PathElement, -5);
            Assert.AreEqual(666, _valueGetter.GetInt(request, _ruleResult, 666));

            _valueGetter.Initialize(Scope.OriginalParameter, "nonexistant");
            Assert.AreEqual(555, _valueGetter.GetInt(request, _ruleResult, 555));
        }

        [TestMethod]
        public void ShouldGetDefaultStringValues()
        {
            var request = new MockRequestInfo("/1/2?param1=3&param2=4&param1=5");
            request.NewPath[1] = "6";
            request.NewParameters.Add("param3", new List<string> { "7" });
            request.ParametersChanged();

            _valueGetter.Initialize(Scope.OriginalPathElement, 3);
            Assert.AreEqual("", _valueGetter.GetString(request, _ruleResult));

            _valueGetter.Initialize(Scope.OriginalPathElement, -4);
            Assert.AreEqual("", _valueGetter.GetString(request, _ruleResult));

            _valueGetter.Initialize(Scope.PathElement, 3);
            Assert.AreEqual("", _valueGetter.GetString(request, _ruleResult));

            _valueGetter.Initialize(Scope.PathElement, -5);
            Assert.AreEqual("", _valueGetter.GetString(request, _ruleResult));

            _valueGetter.Initialize(Scope.OriginalParameter, "nonexistant");
            Assert.AreEqual("", _valueGetter.GetString(request, _ruleResult));
        }
    }
}
