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
    public class InsertActionTests
    {
        private IInsertAction _insertAction;
        private IValueGetter _valueGetter;
        private IRuleResult _ruleResult;

        private IRequestInfo _request1;
        private IRequestInfo _request2;
        private IRequestInfo _request3;
        private IRequestInfo _request4;

        [TestInitialize]
        public void Initialize()
        {
            _request1 = new MockRequestInfo("/path1/path2/path3?param=value");
            _request2 = new MockRequestInfo("/path1/path2", "https", "secure.test.com", 443);
            _request3 = new MockRequestInfo("/path1/path2/");
            _request4 = new MockRequestInfo("/");

            IFactory factory = new NinjectFactory();
            _insertAction = factory.Create<IInsertAction>();
            _valueGetter = factory.Create<IValueGetter>();
            _valueGetter.Initialize(Scope.Literal, "NewValue");
            _ruleResult = factory.Create<IRuleResult>();
        }

        [TestMethod]
        public void ShouldInsertIntoPathMinus3()
        {
            bool stopProcessing;
            bool endRequest;

            _insertAction.Initialize(Scope.PathElement, "-3", _valueGetter);
            _insertAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/NewValue/path1/path2/path3", _request1.NewPathString);
            Assert.AreEqual("/path1/path2", _request2.NewPathString);
            Assert.AreEqual("/path1/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldInsertIntoPathMinus2()
        {
            bool stopProcessing;
            bool endRequest;

            _insertAction.Initialize(Scope.PathElement, "-2", _valueGetter);
            _insertAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/NewValue/path2/path3", _request1.NewPathString);
            Assert.AreEqual("/NewValue/path1/path2", _request2.NewPathString);
            Assert.AreEqual("/NewValue/path1/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldInsertIntoPathMinus1()
        {
            bool stopProcessing;
            bool endRequest;

            _insertAction.Initialize(Scope.PathElement, "-1", _valueGetter);
            _insertAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path2/NewValue/path3", _request1.NewPathString);
            Assert.AreEqual("/path1/NewValue/path2", _request2.NewPathString);
            Assert.AreEqual("/path1/NewValue/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldInsertIntoPathZero()
        {
            bool stopProcessing;
            bool endRequest;

            _insertAction.Initialize(Scope.PathElement, "0", _valueGetter);
            _insertAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/NewValue/path1/path2/path3", _request1.NewPathString);
            Assert.AreEqual("/NewValue/path1/path2", _request2.NewPathString);
            Assert.AreEqual("/NewValue/path1/path2/", _request3.NewPathString);
            Assert.AreEqual("/NewValue", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldInsertIntoPathPlus1()
        {
            bool stopProcessing;
            bool endRequest;

            _insertAction.Initialize(Scope.PathElement, "1", _valueGetter);
            _insertAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/NewValue/path1/path2/path3", _request1.NewPathString);
            Assert.AreEqual("/NewValue/path1/path2", _request2.NewPathString);
            Assert.AreEqual("/NewValue/path1/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldInsertIntoPathPlus2()
        {
            bool stopProcessing;
            bool endRequest;

            _insertAction.Initialize(Scope.PathElement, "2", _valueGetter);
            _insertAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/NewValue/path2/path3", _request1.NewPathString);
            Assert.AreEqual("/path1/NewValue/path2", _request2.NewPathString);
            Assert.AreEqual("/path1/NewValue/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldInsertIntoPathPlus3()
        {
            bool stopProcessing;
            bool endRequest;

            _insertAction.Initialize(Scope.PathElement, "3", _valueGetter);
            _insertAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _insertAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path2/NewValue/path3", _request1.NewPathString);
            Assert.AreEqual("/path1/path2", _request2.NewPathString);
            Assert.AreEqual("/path1/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

    }
}
