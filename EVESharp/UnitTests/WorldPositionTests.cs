using System;
using EVESharpCore.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class WorldPositionTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var a = new DirectWorldPosition(0.0, 0.0, 0.0);
            var b = new DirectWorldPosition(1.0, 1.0, 1.0);
            var c = new DirectWorldPosition(0.0, 0.0, 0.0);
            Assert.IsTrue(a == c);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var a = new DirectWorldPosition(0.0, 0.0, 0.0);
            var b = new DirectWorldPosition(1.0, 1.0, 1.0);
            var c = new DirectWorldPosition(0.0, 0.0, 0.0);
            Assert.IsTrue(a.Equals(c));
        }

        [TestMethod]
        public void TestMethod2()
        {
            var a = new DirectWorldPosition(0.0, 0.0, 0.0);
            var b = new DirectWorldPosition(1.0, 1.0, 1.0);
            var c = new DirectWorldPosition(0.0, 0.0, 0.0);
            Assert.IsFalse(a == b);
        }


        [TestMethod]
        public void TestMethod4()
        {
            var a = new DirectWorldPosition(0.0, 0.0, 0.0);
            var b = new DirectWorldPosition(1.0, 1.0, 1.0);
            var c = new DirectWorldPosition(0.0, 0.0, 0.0);
            Assert.IsTrue(a.GetHashCode() == c.GetHashCode());
        }

        [TestMethod]
        public void TestMethod5()
        {
            var a = new DirectWorldPosition(0.0, 0.0, 0.0);
            var b = new DirectWorldPosition(1.0, 1.0, 1.0);
            var c = new DirectWorldPosition(0.0, 0.0, 0.0);
            Assert.IsTrue(a.GetHashCode() != b.GetHashCode());
        }

        [TestMethod]
        public void TestMethod6()
        {
            DirectWorldPosition aN = null;
            var a = new DirectWorldPosition(0.0, 0.0, 0.0);
            DirectWorldPosition bN = null;
            Assert.IsTrue(aN == bN);
        }

        [TestMethod]
        public void TestMethod7()
        {
            DirectWorldPosition aN = null;
            var a = new DirectWorldPosition(0.0, 0.0, 0.0);
            DirectWorldPosition bN = null;
            Assert.IsTrue(aN == null);
        }

        [TestMethod]
        public void TestMethod8()
        {
            DirectWorldPosition aN = null;
            var a = new DirectWorldPosition(0.0, 0.0, 0.0);
            DirectWorldPosition bN = null;
            Assert.IsTrue(a != null);
        }

        [TestMethod]
        public void TestMethod9()
        {
            var a = new DirectWorldPosition(0.0, 0.0, 0.4);
            var b = new DirectWorldPosition(0.0, 0.0, 0.0);
            Assert.AreEqual(a, b);
        }


        [TestMethod]
        public void TestMethod10()
        {
            var a = new DirectWorldPosition(0.0, 0.0, 0.5);
            var b = new DirectWorldPosition(0.0, 0.0, 0.0);
            Assert.AreNotEqual(a, b);
        }
    }
}
