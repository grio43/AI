using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedComponents.EVE;
using SharedComponents.EveMarshal;
using SharedComponents.Utility;
using SharedComponents.Utility.AsyncLogQueue;
using System.Security.Cryptography;
using System.Text;
using HookManager.Win32Hooks;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class IPUtilTests
    {

        [TestMethod]
        public void TestMethod1()
        {
            var ip1 = "1.1.1.1";
            var ip2 = "10.1.10.1";
            var ip3 = "255.255.255.0";
            var ip4 = "255.255.255.255";

            var ip1U = IPUtil.IpToUint(ip1);
            var ip2U = IPUtil.IpToUint(ip2);
            var ip3U = IPUtil.IpToUint(ip3);
            var ip4U = IPUtil.IpToUint(ip4);

            var ip1R = IPUtil.UintToIp(ip1U);
            var ip2R = IPUtil.UintToIp(ip2U);

            var ip3R = IPUtil.UintToIp(ip3U - 1);
            var ip4R = IPUtil.UintToIp(ip4U + 1);

            Debug.WriteLine(ip4R);

            Assert.IsTrue(ip1 == ip1R);
            Assert.IsTrue(ip2 == ip2R);
            Assert.IsTrue(ip1U != ip2U);
            Assert.IsTrue(ip3R == "255.255.254.255");
            Assert.IsTrue(ip4R == "0.0.0.0");

        }
        [TestMethod]
        public void TestMethod2()
        {
            List<string> list = new List<string>() { "192.0.2.55" };
            var result = IPUtil.GenerateIPMask(list);
            Debug.WriteLine(result);
            Assert.IsTrue(result == "1.1.1.1-192.0.2.54,192.0.2.56-255.255.255.255");
        }

        [TestMethod]
        public void TestMethod3()
        {
            List<string> list = new List<string>() { "192.0.2.55", "192.0.5.33" };
            var result = IPUtil.GenerateIPMask(list);
            Debug.WriteLine(result);
            Assert.IsTrue(result == "1.1.1.1-192.0.2.54,192.0.2.56-192.0.5.32,192.0.5.34-255.255.255.255");
        }


        [TestMethod]
        public void TestMethod4()
        {
            List<string> list = new List<string>() {  "192.0.2.55", "192.0.5.33","192.0.5.34" };
            var result = IPUtil.GenerateIPMask(list);
            Debug.WriteLine(result);
            Assert.IsTrue(result == "1.1.1.1-192.0.2.54,192.0.2.56-192.0.5.32,192.0.5.35-255.255.255.255");

        }

        [TestMethod]
        public void TestMethod5()
        {
            List<string> list = new List<string>() { "192.0.2.55", "192.0.5.33", "192.0.5.34", "192.0.5.35" };
            var result = IPUtil.GenerateIPMask(list);
            Debug.WriteLine(result);
            Assert.IsTrue(result == "1.1.1.1-192.0.2.54,192.0.2.56-192.0.5.32,192.0.5.36-255.255.255.255");
        }

        [TestMethod]
        public void TestMethod6()
        {
            List<string> list = new List<string>() { "192.0.2.55", "192.0.5.33", "192.0.5.35" };
            var result = IPUtil.GenerateIPMask(list);
            Debug.WriteLine(result);
            Assert.IsTrue(result == "1.1.1.1-192.0.2.54,192.0.2.56-192.0.5.32,192.0.5.34,192.0.5.36-255.255.255.255");
        }


        [TestMethod]
        public void TestMethod7()
        {
            List<string> list = new List<string>() { "192.0.2.55", "192.0.5.33", "192.0.5.35", "192.0.10.02", "192.0.10.04" };
            var result = IPUtil.GenerateIPMask(list);
            Debug.WriteLine(result);
            Assert.IsTrue(result == "1.1.1.1-192.0.2.54,192.0.2.56-192.0.5.32,192.0.5.34,192.0.5.36-192.0.10.1,192.0.10.3,192.0.10.5-255.255.255.255");
        }



        [TestMethod]
        public void TestMethod8()
        {
            List<string> list = new List<string>() { "192.0.2.55", "192.0.5.33", "192.0.5.35", "192.0.10.02", "192.0.10.04", "200.0.0.1", "200.0.0.2", "200.0.0.3" };
            var result = IPUtil.GenerateIPMask(list);
            Debug.WriteLine(result);
            Assert.IsTrue(result == "1.1.1.1-192.0.2.54,192.0.2.56-192.0.5.32,192.0.5.34,192.0.5.36-192.0.10.1,192.0.10.3,192.0.10.5-200.0.0.0,200.0.0.4-255.255.255.255");
        }


        [TestMethod]
        public void TestMethod9()
        {
            List<string> list = new List<string>() { "192.0.2.55", "192.0.5.33", "192.0.5.35", "192.0.10.02", "192.0.10.04", "200.0.0.0", "200.0.0.1", "200.0.0.2" };
            var result = IPUtil.GenerateIPMask(list);
            Debug.WriteLine(result);
            Assert.IsTrue(result == "1.1.1.1-192.0.2.54,192.0.2.56-192.0.5.32,192.0.5.34,192.0.5.36-192.0.10.1,192.0.10.3,192.0.10.5-199.255.255.255,200.0.0.3-255.255.255.255");
        }

    }
}
