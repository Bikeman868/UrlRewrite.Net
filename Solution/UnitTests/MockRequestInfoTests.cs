using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Mocks;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UnitTests
{
    [TestClass]
    public class MockRequestInfoTests
    {
        [TestMethod]
        public void AbsoluteWithNoQueryString()
        {
            var request = new MockRequestInfo("/path1/path2");
            Assert.AreEqual("/path1/path2", request.OriginalPathString);
            Assert.AreEqual("", request.OriginalParametersString);
            Assert.AreEqual("/path1/path2", request.NewPathString);
            Assert.AreEqual("", request.NewParametersString);

            var originalPath = request.OriginalPath;
            Assert.AreEqual(3, originalPath.Count);
            Assert.AreEqual("", originalPath[0]);
            Assert.AreEqual("path1", originalPath[1]);
            Assert.AreEqual("path2", originalPath[2]);

            var originalParameters = request.OriginalParameters;
            Assert.AreEqual(0, originalParameters.Count);
        }

        [TestMethod]
        public void AbsoluteWithQueryString()
        {
            var request = new MockRequestInfo("/path1/path2?param=value");
            Assert.AreEqual("/path1/path2", request.OriginalPathString);
            Assert.AreEqual("param=value", request.OriginalParametersString);
            Assert.AreEqual("/path1/path2", request.NewPathString);
            Assert.AreEqual("param=value", request.NewParametersString);

            var originalPath = request.OriginalPath;
            Assert.AreEqual(3, originalPath.Count);
            Assert.AreEqual("", originalPath[0]);
            Assert.AreEqual("path1", originalPath[1]);
            Assert.AreEqual("path2", originalPath[2]);

            var originalParameters = request.OriginalParameters;
            Assert.AreEqual(1, originalParameters.Count);
            Assert.AreEqual("value", originalParameters["param"][0]);

            var newPath = request.NewPath;
            Assert.AreEqual(3, newPath.Count);
            Assert.AreEqual("", newPath[0]);
            Assert.AreEqual("path1", newPath[1]);
            Assert.AreEqual("path2", newPath[2]);

            var newParameters = request.NewParameters;
            Assert.AreEqual(1, newParameters.Count);
            Assert.AreEqual("value", newParameters["param"][0]);
        }

        [TestMethod]
        public void RelativeWithNoQueryString()
        {
            var request = new MockRequestInfo("path1/path2");
            Assert.AreEqual("path1/path2", request.OriginalPathString);
            Assert.AreEqual("", request.OriginalParametersString);
            Assert.AreEqual("path1/path2", request.NewPathString);
            Assert.AreEqual("", request.NewParametersString);

            var originalPath = request.OriginalPath;
            Assert.AreEqual(2, originalPath.Count);
            Assert.AreEqual("path1", originalPath[0]);
            Assert.AreEqual("path2", originalPath[1]);

            var originalParameters = request.OriginalParameters;
            Assert.AreEqual(0, originalParameters.Count);
        }

        [TestMethod]
        public void RelativeWithQueryString()
        {
            var request = new MockRequestInfo("path1/path2?param=value");
            Assert.AreEqual("path1/path2", request.OriginalPathString);
            Assert.AreEqual("param=value", request.OriginalParametersString);
            Assert.AreEqual("path1/path2", request.NewPathString);
            Assert.AreEqual("param=value", request.NewParametersString);

            var originalPath = request.OriginalPath;
            Assert.AreEqual(2, originalPath.Count);
            Assert.AreEqual("path1", originalPath[0]);
            Assert.AreEqual("path2", originalPath[1]);

            var originalParameters = request.OriginalParameters;
            Assert.AreEqual(1, originalParameters.Count);
            Assert.AreEqual("value", originalParameters["param"][0]);

            var newPath = request.NewPath;
            Assert.AreEqual(2, newPath.Count);
            Assert.AreEqual("path1", newPath[0]);
            Assert.AreEqual("path2", newPath[1]);

            var newParameters = request.NewParameters;
            Assert.AreEqual(1, newParameters.Count);
            Assert.AreEqual("value", newParameters["param"][0]);
        }
    }
}
