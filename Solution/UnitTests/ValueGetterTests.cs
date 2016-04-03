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
        private IRequestInfo _request1;
        private IRequestInfo _request2;
        private IRequestInfo _request3;

        [TestInitialize]
        public void Initialize()
        {
            _request1 = new MockRequestInfo("/path1/path2?param=value");
            _request2 = new MockRequestInfo("/path1/path2");
            _request3 = new MockRequestInfo("/path1/path2/");

            _request1.NewPath[1] = "changed1";
            _request1.NewParameters["param"] = new List<string> {"changed"};
            _request1.PathChanged();

            _request2.NewPath[2] = "changed2";
            _request2.PathChanged();

            _request3.NewUrlString = "/changed1/changed2/";
            _request3.NewParameters["param"] = new List<string> { "added" };
            _request3.ParametersChanged();

            IFactory factory = new DefaultFactory();
            _valueGetter = factory.Create<IValueGetter>();
        }

        [TestMethod]
        public void ShouldGetVariousScopesAsStrings()
        {
            _valueGetter.Initialize(Scope.OriginalUrl);
            Assert.AreEqual(_request1.OriginalUrlString, _valueGetter.GetString(_request1));
            Assert.AreEqual(_request2.OriginalUrlString, _valueGetter.GetString(_request2));
            Assert.AreEqual(_request3.OriginalUrlString, _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.NewUrl);
            Assert.AreEqual(_request1.NewUrlString, _valueGetter.GetString(_request1));
            Assert.AreEqual(_request2.NewUrlString, _valueGetter.GetString(_request2));
            Assert.AreEqual(_request3.NewUrlString, _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.OriginalPath);
            Assert.AreEqual(_request1.OriginalPathString, _valueGetter.GetString(_request1));
            Assert.AreEqual(_request2.OriginalPathString, _valueGetter.GetString(_request2));
            Assert.AreEqual(_request3.OriginalPathString, _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.NewPath);
            Assert.AreEqual(_request1.NewPathString, _valueGetter.GetString(_request1));
            Assert.AreEqual(_request2.NewPathString, _valueGetter.GetString(_request2));
            Assert.AreEqual(_request3.NewPathString, _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.OriginalQueryString);
            Assert.AreEqual(_request1.OriginalParametersString, _valueGetter.GetString(_request1));
            Assert.AreEqual(_request2.OriginalParametersString, _valueGetter.GetString(_request2));
            Assert.AreEqual(_request3.OriginalParametersString, _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.NewQueryString);
            Assert.AreEqual(_request1.NewParametersString, _valueGetter.GetString(_request1));
            Assert.AreEqual(_request2.NewParametersString, _valueGetter.GetString(_request2));
            Assert.AreEqual(_request3.NewParametersString, _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.OriginalPathElement, 1);
            Assert.AreEqual(_request1.OriginalPath[1], _valueGetter.GetString(_request1));
            Assert.AreEqual(_request2.OriginalPath[1], _valueGetter.GetString(_request2));
            Assert.AreEqual(_request3.OriginalPath[1], _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.OriginalPathElement, -1);
            Assert.AreEqual("path2", _valueGetter.GetString(_request1));
            Assert.AreEqual("path2", _valueGetter.GetString(_request2));
            Assert.AreEqual("path2", _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.NewPathElement, 1);
            Assert.AreEqual(_request1.NewPath[1], _valueGetter.GetString(_request1));
            Assert.AreEqual(_request2.NewPath[1], _valueGetter.GetString(_request2));
            Assert.AreEqual(_request3.NewPath[1], _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.NewPathElement, -1);
            Assert.AreEqual("path2", _valueGetter.GetString(_request1));
            Assert.AreEqual("changed2", _valueGetter.GetString(_request2));
            Assert.AreEqual("changed2", _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.OriginalParameter, "param");
            Assert.AreEqual("value", _valueGetter.GetString(_request1));
            Assert.AreEqual("", _valueGetter.GetString(_request2));
            Assert.AreEqual("", _valueGetter.GetString(_request3));

            _valueGetter.Initialize(Scope.NewParameter, "param");
            Assert.AreEqual("changed", _valueGetter.GetString(_request1));
            Assert.AreEqual("", _valueGetter.GetString(_request2));
            Assert.AreEqual("added", _valueGetter.GetString(_request3));
        }
       
        [TestMethod]
        public void ShouldGetVariousScopesAsNumbers()
        {
            var request = new MockRequestInfo("/1/2?param1=3&param2=4&param1=5");
            request.NewPath[1] = "6";
            request.NewParameters.Add("param3", new List<string>{"7"});
            request.ParametersChanged();

            _valueGetter.Initialize(Scope.OriginalPathElement, 1);
            Assert.AreEqual(1, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.OriginalPathElement, 2);
            Assert.AreEqual(2, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.NewPathElement, 1);
            Assert.AreEqual(6, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.NewPathElement, 2);
            Assert.AreEqual(2, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.NewPathElement, -1);
            Assert.AreEqual(2, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.NewPathElement, -2);
            Assert.AreEqual(6, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.OriginalParameter, "param1");
            Assert.AreEqual(3, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.OriginalParameter, "param2");
            Assert.AreEqual(4, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.OriginalParameter, "param3");
            Assert.AreEqual(0, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.NewParameter, "param1");
            Assert.AreEqual(3, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.NewParameter, "param2");
            Assert.AreEqual(4, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.NewParameter, "param3");
            Assert.AreEqual(7, _valueGetter.GetInt(request, 0));

            _valueGetter.Initialize(Scope.NewParameter, "param4");
            Assert.AreEqual(0, _valueGetter.GetInt(request, 0));
        }

        [TestMethod]
        public void ShouldGetDefaultNumberValues()
        {
            var request = new MockRequestInfo("/1/2?param1=3&param2=4&param1=5");
            request.NewPath[1] = "6";
            request.NewParameters.Add("param3", new List<string> { "7" });
            request.ParametersChanged();

            _valueGetter.Initialize(Scope.OriginalPathElement, 3);
            Assert.AreEqual(999, _valueGetter.GetInt(request, 999));

            _valueGetter.Initialize(Scope.OriginalPathElement, -4);
            Assert.AreEqual(888, _valueGetter.GetInt(request, 888));

            _valueGetter.Initialize(Scope.NewPathElement, 3);
            Assert.AreEqual(777, _valueGetter.GetInt(request, 777));

            _valueGetter.Initialize(Scope.NewPathElement, -5);
            Assert.AreEqual(666, _valueGetter.GetInt(request, 666));

            _valueGetter.Initialize(Scope.OriginalParameter, "nonexistant");
            Assert.AreEqual(555, _valueGetter.GetInt(request, 555));
        }

        [TestMethod]
        public void ShouldGetDefaultStringValues()
        {
            var request = new MockRequestInfo("/1/2?param1=3&param2=4&param1=5");
            request.NewPath[1] = "6";
            request.NewParameters.Add("param3", new List<string> { "7" });
            request.ParametersChanged();

            _valueGetter.Initialize(Scope.OriginalPathElement, 3);
            Assert.AreEqual("", _valueGetter.GetString(request));

            _valueGetter.Initialize(Scope.OriginalPathElement, -4);
            Assert.AreEqual("", _valueGetter.GetString(request));

            _valueGetter.Initialize(Scope.NewPathElement, 3);
            Assert.AreEqual("", _valueGetter.GetString(request));

            _valueGetter.Initialize(Scope.NewPathElement, -5);
            Assert.AreEqual("", _valueGetter.GetString(request));

            _valueGetter.Initialize(Scope.OriginalParameter, "nonexistant");
            Assert.AreEqual("", _valueGetter.GetString(request));
        }
    }
}
