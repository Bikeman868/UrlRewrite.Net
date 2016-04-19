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
    public class KeepActionTests
    {
        private IRequestInfo _request1;
        private IRequestInfo _request2;
        private IRequestInfo _request3;
        private IRequestInfo _request4;

        private IKeepAction _keepAction;
        private IRuleResult _ruleResult;

        [TestInitialize]
        public void Initialize()
        {
            _request1 = new MockRequestInfo("/path1/path2/path3?p2=v2");
            _request2 = new MockRequestInfo("/path1/path2", "https", "secure.test.com", 443);
            _request3 = new MockRequestInfo("/path1/path2/?p1=v1&p2=v2&p3=v3");
            _request4 = new MockRequestInfo("/?p3=v3&p2=v2");

            IFactory factory = new NinjectFactory();
            _keepAction = factory.Create<IKeepAction>();
            _ruleResult = factory.Create<IRuleResult>();
        }

        [TestMethod]
        public void ShouldKeepAllPathElements()
        {
            bool stopProcessing;
            bool endRequest;

            _keepAction.Initialize(Scope.PathElement, "1,2,3,4,5");
            _keepAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual(_request1.OriginalPathString, _request1.NewPathString);
            Assert.AreEqual(_request2.OriginalPathString, _request2.NewPathString);
            Assert.AreEqual(_request3.OriginalPathString, _request3.NewPathString);
            Assert.AreEqual(_request4.OriginalPathString, _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldKeepAllParameters()
        {
            bool stopProcessing;
            bool endRequest;

            _keepAction.Initialize(Scope.Parameter, "p1,p2,p3,p4,p5");
            _keepAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("p2=v2", _request1.NewParametersString);
            Assert.AreEqual("", _request2.NewParametersString);
            Assert.AreEqual("p1=v1&p2=v2&p3=v3", _request3.NewParametersString);
            Assert.AreEqual("p2=v2&p3=v3", _request4.NewParametersString);
        }

        [TestMethod]
        public void ShouldKeepNoPathElements()
        {
            bool stopProcessing;
            bool endRequest;

            _keepAction.Initialize(Scope.PathElement, "");
            _keepAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/", _request1.NewPathString);
            Assert.AreEqual("/", _request2.NewPathString);
            Assert.AreEqual("/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldKeepNoParameters()
        {
            bool stopProcessing;
            bool endRequest;

            _keepAction.Initialize(Scope.Parameter, "");
            _keepAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("", _request1.NewParametersString);
            Assert.AreEqual("", _request2.NewParametersString);
            Assert.AreEqual("", _request3.NewParametersString);
            Assert.AreEqual("", _request4.NewParametersString);
        }

        [TestMethod]
        public void ShouldKeepFirstPathElement()
        {
            bool stopProcessing;
            bool endRequest;

            _keepAction.Initialize(Scope.PathElement, "1");
            _keepAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1", _request1.NewPathString);
            Assert.AreEqual("/path1", _request2.NewPathString);
            Assert.AreEqual("/path1", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldKeepOneParameter()
        {
            bool stopProcessing;
            bool endRequest;

            _keepAction.Initialize(Scope.Parameter, "p2");
            _keepAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("p2=v2", _request1.NewParametersString);
            Assert.AreEqual("", _request2.NewParametersString);
            Assert.AreEqual("p2=v2", _request3.NewParametersString);
            Assert.AreEqual("p2=v2", _request4.NewParametersString);
        }

        [TestMethod]
        public void ShouldKeepSecondPathElement()
        {
            bool stopProcessing;
            bool endRequest;

            _keepAction.Initialize(Scope.PathElement, "2");
            _keepAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path2", _request1.NewPathString);
            Assert.AreEqual("/path2", _request2.NewPathString);
            Assert.AreEqual("/path2", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldKeepSeveralPathElements()
        {
            bool stopProcessing;
            bool endRequest;

            _keepAction.Initialize(Scope.PathElement, "1,3");
            _keepAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/path1/path3", _request1.NewPathString);
            Assert.AreEqual("/path1", _request2.NewPathString);
            Assert.AreEqual("/path1/", _request3.NewPathString);
            Assert.AreEqual("/", _request4.NewPathString);
        }

        [TestMethod]
        public void ShouldKeepSeveralParameters()
        {
            bool stopProcessing;
            bool endRequest;

            _keepAction.Initialize(Scope.Parameter, "p2,p3");
            _keepAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _keepAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("p2=v2", _request1.NewParametersString);
            Assert.AreEqual("", _request2.NewParametersString);
            Assert.AreEqual("p2=v2&p3=v3", _request3.NewParametersString);
            Assert.AreEqual("p2=v2&p3=v3", _request4.NewParametersString);
        }

    }
}
