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
    public class DeleteActionTests
    {
        private IDeleteAction _deleteAction;
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
            _deleteAction = factory.Create<IDeleteAction>();
            _ruleResult = factory.Create<IRuleResult>();
        }

        [TestMethod]
        public void ShouldDeleteFromPathMinus3()
        {
            bool stopProcessing;
            bool endRequest;

            _deleteAction.Initialize(Scope.PathElement, "-3");
            _deleteAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path2/path3", _request1.NewPathString);
            Assert.AreEqual("/path1/path2", _request2.NewPathString);
            Assert.AreEqual("/path1/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldDeleteFromPathMinus2()
        {
            bool stopProcessing;
            bool endRequest;

            _deleteAction.Initialize(Scope.PathElement, "-2");
            _deleteAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path3", _request1.NewPathString);
            Assert.AreEqual("/path2", _request2.NewPathString);
            Assert.AreEqual("/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldDeleteFromPathMinus1()
        {
            bool stopProcessing;
            bool endRequest;

            _deleteAction.Initialize(Scope.PathElement, "-1");
            _deleteAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path2", _request1.NewPathString);
            Assert.AreEqual("/path1", _request2.NewPathString);
            Assert.AreEqual("/path1/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldDeleteFromPathZero()
        {
            bool stopProcessing;
            bool endRequest;

            _deleteAction.Initialize(Scope.PathElement, "0");
            _deleteAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/", _request1.NewPathString);
            Assert.AreEqual("/", _request2.NewPathString);
            Assert.AreEqual("/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldDeleteFromPathPlus1()
        {
            bool stopProcessing;
            bool endRequest;

            _deleteAction.Initialize(Scope.PathElement, "1");
            _deleteAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path2/path3", _request1.NewPathString);
            Assert.AreEqual("/path2", _request2.NewPathString);
            Assert.AreEqual("/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldDeleteFromPathPlus2()
        {
            bool stopProcessing;
            bool endRequest;

            _deleteAction.Initialize(Scope.PathElement, "2");
            _deleteAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path3", _request1.NewPathString);
            Assert.AreEqual("/path1", _request2.NewPathString);
            Assert.AreEqual("/path1/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldDeleteFromPathPlus3()
        {
            bool stopProcessing;
            bool endRequest;

            _deleteAction.Initialize(Scope.PathElement, "3");
            _deleteAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _deleteAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path2", _request1.NewPathString);
            Assert.AreEqual("/path1/path2", _request2.NewPathString);
            Assert.AreEqual("/path1/path2/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

    }
}
