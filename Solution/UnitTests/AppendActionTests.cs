using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Mocks;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Utilities;

namespace UnitTests
{
    [TestClass]
    public class AppendActionTests
    {
        private IAppendAction _appendAction;
        private IValueGetter _valueGetter;
        private IRuleResult _ruleResult;

        private IRequestInfo _request1;
        private IRequestInfo _request2;
        private IRequestInfo _request3;
        private IRequestInfo _request4;
        private IRequestInfo _request5;

        [TestInitialize]
        public void Initialize()
        {
            _request1 = new MockRequestInfo("/path1/path2/path3?param=value");
            _request2 = new MockRequestInfo("/path1/path2", "https", "secure.test.com", 443);
            _request3 = new MockRequestInfo("/path1/path2/");
            _request4 = new MockRequestInfo("/path1/");
            _request5 = new MockRequestInfo("/");

            IFactory factory = new NinjectFactory();
            _appendAction = factory.Create<IAppendAction>();
            _valueGetter = factory.Create<IValueGetter>();
            _valueGetter.Initialize(Scope.Literal, "NewValue");
            _ruleResult = factory.Create<IRuleResult>();
        }

        [TestMethod]
        public void ShouldAppendToPathMinus3()
        {
            bool stopProcessing;
            bool endRequest;

            _appendAction.Initialize(Scope.PathElement, "-3", _valueGetter);
            _appendAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1NewValue/path2/path3", _request1.NewPathString);
            Assert.AreEqual("/path1/path2", _request2.NewPathString);
            Assert.AreEqual("/path1/path2/", _request3.NewPathString);
            Assert.AreEqual("/path1/", _request4.NewPathString);
            Assert.AreEqual("/", _request5.NewPathString);
        }

        [TestMethod]
        public void ShouldAppendToPathMinus2()
        {
            bool stopProcessing;
            bool endRequest;

            _appendAction.Initialize(Scope.PathElement, "-2", _valueGetter);
            _appendAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path2NewValue/path3", _request1.NewPathString);
            Assert.AreEqual("/path1NewValue/path2", _request2.NewPathString);
            Assert.AreEqual("/path1NewValue/path2/", _request3.NewPathString);
            Assert.AreEqual("/path1/", _request4.NewPathString);
            Assert.AreEqual("/", _request5.NewPathString);
        }

        [TestMethod]
        public void ShouldAppendToPathMinus1()
        {
            bool stopProcessing;
            bool endRequest;

            _appendAction.Initialize(Scope.PathElement, "-1", _valueGetter);
            _appendAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path2/path3NewValue", _request1.NewPathString);
            Assert.AreEqual("/path1/path2NewValue", _request2.NewPathString);
            Assert.AreEqual("/path1/path2NewValue/", _request3.NewPathString);
            Assert.AreEqual("/path1NewValue/", _request4.NewPathString);
            Assert.AreEqual("/", _request5.NewPathString);
        }

        [TestMethod]
        public void ShouldAppendToPathZero()
        {
            bool stopProcessing;
            bool endRequest;

            _appendAction.Initialize(Scope.PathElement, "0", _valueGetter);
            _appendAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path2/path3/NewValue", _request1.NewPathString);
            Assert.AreEqual("/path1/path2/NewValue", _request2.NewPathString);
            Assert.AreEqual("/path1/path2/NewValue", _request3.NewPathString);
            Assert.AreEqual("/path1/NewValue", _request4.NewPathString);
            Assert.AreEqual("/NewValue", _request5.NewPathString);
        }

        [TestMethod]
        public void ShouldAppendToPathPlus1()
        {
            bool stopProcessing;
            bool endRequest;

            _appendAction.Initialize(Scope.PathElement, "1", _valueGetter);
            _appendAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1NewValue/path2/path3", _request1.NewPathString);
            Assert.AreEqual("/path1NewValue/path2", _request2.NewPathString);
            Assert.AreEqual("/path1NewValue/path2/", _request3.NewPathString);
            Assert.AreEqual("/path1NewValue/", _request4.NewPathString);
            Assert.AreEqual("/", _request5.NewPathString);
        }

        [TestMethod]
        public void ShouldAppendToPathPlus2()
        {
            bool stopProcessing;
            bool endRequest;

            _appendAction.Initialize(Scope.PathElement, "2", _valueGetter);
            _appendAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path2NewValue/path3", _request1.NewPathString);
            Assert.AreEqual("/path1/path2NewValue", _request2.NewPathString);
            Assert.AreEqual("/path1/path2NewValue/", _request3.NewPathString);
            Assert.AreEqual("/path1/", _request4.NewPathString);
            Assert.AreEqual("/", _request5.NewPathString);
        }

        [TestMethod]
        public void ShouldAppendToPathPlus3()
        {
            bool stopProcessing;
            bool endRequest;

            _appendAction.Initialize(Scope.PathElement, "3", _valueGetter);
            _appendAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _appendAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path2/path3NewValue", _request1.NewPathString);
            Assert.AreEqual("/path1/path2", _request2.NewPathString);
            Assert.AreEqual("/path1/path2/", _request3.NewPathString);
            Assert.AreEqual("/path1/", _request4.NewPathString);
            Assert.AreEqual("/", _request5.NewPathString);
        }

    }
}
