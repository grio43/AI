
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedComponents.EVE;
using SharedComponents.EveMarshal;
using SharedComponents.Utility;
using SharedComponents.Utility.AsyncLogQueue;
using p = SharedComponents.EVE.PatternEval;
using System.Security.Cryptography;
using System.Text;
using HookManager.Win32Hooks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace UnitTests
{
    /**
    [TestClass]
    public class PatternEvalTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var s =
                @"{mo[04:XX][09:X3]},{tu[05:XX][16:X3]},{we[05:XX][16:XX]},{th[05:4X][16:X5]},{fr[05:XX][16:XX]},{sa[05:XX][16:XX]},{su[05:XX][16:XX]}";
            var res = p.GenerateOutput(s);
            Assert.AreEqual(s.Length, res.Length);
            Assert.IsFalse(res.ToLower().Contains("x") && res.ToLower().Contains("y"));
        }

        [TestMethod]
        public void TestMethod2()
        {
            DateTime a = new DateTime(2020, 7, 9, 9, 0, 35);
            DateTime b = new DateTime(2020, 7, 9, 17, 59, 35);

            var s =
                @"{mo[04:XX][09:X3]},{tu[05:XX][16:X3]},{we[05:XX][16:XX]},{th[05:4X][16:X5]},{fr[05:XX][16:XX]},{sa[05:XX][16:XX]},{su[05:XX][16:XX]}";
            var res = p.GenerateOutput(s);

            Console.WriteLine(res);
            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, a));
            Assert.IsFalse(p.IsAnyPatternMatchingDatetime(res, b));
        }

        [TestMethod]
        public void TestMethod3()
        {
            DateTime a = new DateTime(2020, 7, 6, 17, 59, 35);
            DateTime b = new DateTime(2020, 7, 7, 17, 59, 35);
            DateTime c = new DateTime(2020, 7, 8, 17, 59, 35);
            DateTime d = new DateTime(2020, 7, 9, 17, 59, 35);
            DateTime e = new DateTime(2020, 7, 10, 17, 59, 35);
            DateTime f = new DateTime(2020, 7, 11, 17, 59, 35);
            DateTime g = new DateTime(2020, 7, 12, 17, 59, 35);

            DateTime h = new DateTime(2020, 7, 12, 19, 59, 35);

            DateTime i = new DateTime(2020, 7, 12, 21, 59, 35);

            var s = @"{?[15:XX][18:X3]},{?[20:XX][22:X3]}";
            var res = p.GenerateOutput(s);

            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, a));
            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, b));
            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, c));
            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, d));
            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, e));
            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, f));
            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, g));

            Assert.IsFalse(p.IsAnyPatternMatchingDatetime(res, h));

            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, i));

        }

        [TestMethod]
        public void TestMethod4()
        {
            DateTime a = new DateTime(2020, 7, 9, 10, 0, 35);
            var s =
                @"{?[04:XX][09:X3]}";
            var res = p.GenerateOutput(s);

            Debug.WriteLine(res);
            Assert.IsTrue(p.IsAnyPatternMatchingDatetime(res, a, 60));
            Assert.IsFalse(p.IsAnyPatternMatchingDatetime(res, a, 0));

        }

        [TestMethod]
        public void TestMethod5()
        {
            var s = "[{\"buy\":{\"forQuery\":{\"bid\":true,\"types\":[215],\"regions\":[10000002],\"systems\":[30000142],\"hours\":24,\"minq\":1},\"volume\":4250000,\"wavg\":2.59,\"avg\":2.53,\"variance\":0.08,\"stdDev\":0.29,\"median\":2.70,\"fivePercent\":2.72,\"max\":2.72,\"min\":2.02,\"highToLow\":true,\"generated\":1665961006249},\"sell\":{\"forQuery\":{\"bid\":false,\"types\":[215],\"regions\":[10000002],\"systems\":[30000142],\"hours\":24,\"minq\":1},\"volume\":61861951,\"wavg\":5.45,\"avg\":5.13,\"variance\":0.30,\"stdDev\":0.55,\"median\":5.63,\"fivePercent\":4.70,\"max\":6.00,\"min\":4.45,\"highToLow\":false,\"generated\":1665961006249}}]";
            var regex = new Regex(@"(?:\""sell\"".*\""wavg\"":(?<Wavg>[\d\.]+))", RegexOptions.Compiled);
            var result = regex.Match(s).Groups["Wavg"].Value;
            var d = double.Parse(result, CultureInfo.InvariantCulture);

            Assert.AreEqual("5.45", result);
            Assert.AreEqual(5.45d, d);
        }
    }
    **/
}
