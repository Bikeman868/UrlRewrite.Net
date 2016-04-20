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
    public class NormalizeActionTests
    {
        private INormalizeAction _normalizeAction;
        private IRuleResult _ruleResult;

        private IRequestInfo _request1;
        private IRequestInfo _request2;
        private IRequestInfo _request3;
        private IRequestInfo _request4;
        private IRequestInfo _request5;
        private IRequestInfo _request6;
        private IRequestInfo _request7;
        private IRequestInfo _request8;

        [TestInitialize]
        public void Initialize()
        {
            _request1 = new MockRequestInfo("");
            _request2 = new MockRequestInfo("/");
            _request3 = new MockRequestInfo("/path1");
            _request4 = new MockRequestInfo("path1/");
            _request5 = new MockRequestInfo("path1");
            _request6 = new MockRequestInfo("/path1/path2");
            _request7 = new MockRequestInfo("path1/path2/");
            _request8 = new MockRequestInfo("path1/path2");

            IFactory factory = new NinjectFactory();
            _normalizeAction = factory.Create<INormalizeAction>();
            _ruleResult = factory.Create<IRuleResult>();
        }

        [TestMethod]
        public void ShouldDoNothing()
        {
            bool stopProcessing;
            bool endRequest;

            _normalizeAction.Initialize(NormalizeAction.None, NormalizeAction.None);
            _normalizeAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request6, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request7, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request8, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("", _request1.NewPathString);
            Assert.AreEqual("/", _request2.NewPathString);
            Assert.AreEqual("/path1", _request3.NewPathString);
            Assert.AreEqual("path1/", _request4.NewPathString);
            Assert.AreEqual("path1", _request5.NewPathString);
            Assert.AreEqual("/path1/path2", _request6.NewPathString);
            Assert.AreEqual("path1/path2/", _request7.NewPathString);
            Assert.AreEqual("path1/path2", _request8.NewPathString);
        }

        [TestMethod]
        public void ShouldAddLeadingSeparator()
        {
            bool stopProcessing;
            bool endRequest;

            _normalizeAction.Initialize(NormalizeAction.Add, NormalizeAction.None);
            _normalizeAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request6, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request7, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request8, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/", _request1.NewPathString);
            Assert.AreEqual("/", _request2.NewPathString);
            Assert.AreEqual("/path1", _request3.NewPathString);
            Assert.AreEqual("/path1/", _request4.NewPathString);
            Assert.AreEqual("/path1", _request5.NewPathString);
            Assert.AreEqual("/path1/path2", _request6.NewPathString);
            Assert.AreEqual("/path1/path2/", _request7.NewPathString);
            Assert.AreEqual("/path1/path2", _request8.NewPathString);
        }

        [TestMethod]
        public void ShouldAddTrailingSeparator()
        {
            bool stopProcessing;
            bool endRequest;

            _normalizeAction.Initialize(NormalizeAction.None, NormalizeAction.Add);
            _normalizeAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request6, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request7, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request8, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/", _request1.NewPathString);
            Assert.AreEqual("/", _request2.NewPathString);
            Assert.AreEqual("/path1/", _request3.NewPathString);
            Assert.AreEqual("path1/", _request4.NewPathString);
            Assert.AreEqual("path1/", _request5.NewPathString);
            Assert.AreEqual("/path1/path2/", _request6.NewPathString);
            Assert.AreEqual("path1/path2/", _request7.NewPathString);
            Assert.AreEqual("path1/path2/", _request8.NewPathString);
        }

        [TestMethod]
        public void ShouldRemoveLeadingSeparator()
        {
            bool stopProcessing;
            bool endRequest;

            _normalizeAction.Initialize(NormalizeAction.Remove, NormalizeAction.None);
            _normalizeAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request6, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request7, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request8, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("", _request1.NewPathString);
            Assert.AreEqual("", _request2.NewPathString);
            Assert.AreEqual("path1", _request3.NewPathString);
            Assert.AreEqual("path1/", _request4.NewPathString);
            Assert.AreEqual("path1", _request5.NewPathString);
            Assert.AreEqual("path1/path2", _request6.NewPathString);
            Assert.AreEqual("path1/path2/", _request7.NewPathString);
            Assert.AreEqual("path1/path2", _request8.NewPathString);
        }

        [TestMethod]
        public void ShouldRemoveTrailingSeparator()
        {
            bool stopProcessing;
            bool endRequest;

            _normalizeAction.Initialize(NormalizeAction.None, NormalizeAction.Remove);
            _normalizeAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request6, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request7, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request8, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("", _request1.NewPathString);
            Assert.AreEqual("", _request2.NewPathString);
            Assert.AreEqual("/path1", _request3.NewPathString);
            Assert.AreEqual("path1", _request4.NewPathString);
            Assert.AreEqual("path1", _request5.NewPathString);
            Assert.AreEqual("/path1/path2", _request6.NewPathString);
            Assert.AreEqual("path1/path2", _request7.NewPathString);
            Assert.AreEqual("path1/path2", _request8.NewPathString);
        }

        [TestMethod]
        public void ShouldAddBothSeparators()
        {
            bool stopProcessing;
            bool endRequest;

            _normalizeAction.Initialize(NormalizeAction.Add, NormalizeAction.Add);
            _normalizeAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request6, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request7, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request8, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("/", _request1.NewPathString);
            Assert.AreEqual("/", _request2.NewPathString);
            Assert.AreEqual("/path1/", _request3.NewPathString);
            Assert.AreEqual("/path1/", _request4.NewPathString);
            Assert.AreEqual("/path1/", _request5.NewPathString);
            Assert.AreEqual("/path1/path2/", _request6.NewPathString);
            Assert.AreEqual("/path1/path2/", _request7.NewPathString);
            Assert.AreEqual("/path1/path2/", _request8.NewPathString);
        }

        [TestMethod]
        public void ShouldRemoveBothSepararators()
        {
            bool stopProcessing;
            bool endRequest;

            _normalizeAction.Initialize(NormalizeAction.Remove, NormalizeAction.Remove);
            _normalizeAction.PerformAction(_request1, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request2, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request3, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request4, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request5, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request6, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request7, _ruleResult, out stopProcessing, out endRequest);
            _normalizeAction.PerformAction(_request8, _ruleResult, out stopProcessing, out endRequest);

            Assert.AreEqual("", _request1.NewPathString);
            Assert.AreEqual("", _request2.NewPathString);
            Assert.AreEqual("path1", _request3.NewPathString);
            Assert.AreEqual("path1", _request4.NewPathString);
            Assert.AreEqual("path1", _request5.NewPathString);
            Assert.AreEqual("path1/path2", _request6.NewPathString);
            Assert.AreEqual("path1/path2", _request7.NewPathString);
            Assert.AreEqual("path1/path2", _request8.NewPathString);
        }

    }
}
