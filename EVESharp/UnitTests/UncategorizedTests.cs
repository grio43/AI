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
using System.Net.Mime;
using System.Windows.Forms;

namespace UnitTests
{
    [TestClass]
    public class UncategorizedTests
    {
        private Random _rnd = new Random();
        [TestMethod]
        public void T1()
        {
            var x = 1000.0d;
            double minPerc = 0.01d;
            double maxPerc = 0.03d;
            x = x + (_rnd.NextDouble() * (x * maxPerc - x * minPerc) + x * minPerc) * (_rnd.NextDouble() >= 0.5 ? 1 : -1);
            Debug.WriteLine(x);
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void T2()
        {
            var x = 0.0049072396941483f;
            var y = 0.999934792518616f;
            var z = -0.0103144673630595f;
            Vec3 test = new Vec3(x,y,z);
            Debug.WriteLine(test.Magnitude);
            Assert.IsTrue(test.IsUnitVector(0.000001d));
        }
        
        
        
        private bool IsPointOnLine(Vec3 p1, Vec3 p2, Vec3 randomPoint, double errorRate = 0.0)
        {
            // Calculate the vector between the two line endpoints
            double lineX = p2.X - p1.X;
            double lineY = p2.Y - p1.Y;
            double lineZ = p2.Z - p1.Z;

            // Calculate the vector between the first endpoint and the random point
            double pointX = randomPoint.X - p1.X;
            double pointY = randomPoint.Y - p1.Y;
            double pointZ = randomPoint.Z - p1.Z;

            // Calculate the cross product of the two vectors
            double crossProductX = lineY * pointZ - lineZ * pointY;
            double crossProductY = lineZ * pointX - lineX * pointZ;
            double crossProductZ = lineX * pointY - lineY * pointX;

            // Calculate the magnitude of the cross product
            double crossProductMagnitude = Math.Sqrt(crossProductX * crossProductX + crossProductY * crossProductY + crossProductZ * crossProductZ);

            // Calculate the length of the line segment
            double lineLength = Math.Sqrt(lineX * lineX + lineY * lineY + lineZ * lineZ);

            // Calculate the distance between the random point and the line segment
            double distance = crossProductMagnitude / lineLength;

            // Check if the distance is within the error rate
            return distance <= errorRate;
        }
        [TestMethod]
        public void T3()
        {
           var p1 = new Vec3(300, 10000, 50000);
           var p2 = new Vec3(-300, -10000, -50000);
           var direction = (p1 - p2);
           var mag = direction.Magnitude;
           var testPointTrue = (p1 + direction.Normalize() * (mag / Math.PI));
           var testPointFalse = (p1 + direction.Normalize() * (mag / Math.PI)) + new Vec3(1,1,1);
           Assert.IsTrue(IsPointOnLine(p1, p2, testPointTrue, 0.200));
           Assert.IsFalse(IsPointOnLine(p1, p2, testPointFalse, 0.200));
        }

        [TestMethod]
        public void T4()
        {
            foreach (var (id, name) in VirtualDesktopHelper.GetVirtualDesktops())
            {
                Console.WriteLine(id + " - " + name);
            }
        }

        [TestMethod]
        public void T5_TestVirtualDesktopWindowMoving()
        {

            // Create a form as the process-owned window
            Form testForm = new Form { Text = "Test" };
            testForm.Show();
            IntPtr hWnd = testForm.Handle;

            var isOnCurrent = VirtualDesktopHelper.VirtualDesktopManager.IsWindowOnCurrentVirtualDesktop(hWnd);
            var currentGuid = VirtualDesktopHelper.VirtualDesktopManager.GetWindowDesktopId(hWnd);

            var vDesktops = VirtualDesktopHelper.GetVirtualDesktops();
            if (vDesktops.Count > 1)
            {
                VirtualDesktopHelper.VirtualDesktopManager.MoveWindowToDesktop(hWnd, vDesktops.FirstOrDefault(e => e.DesktopId != currentGuid).DesktopId);
                var afterMovetGuid = VirtualDesktopHelper.VirtualDesktopManager.GetWindowDesktopId(hWnd);

                Assert.IsTrue(currentGuid != afterMovetGuid);
            }
            else
            {
                Assert.IsTrue(true);
            }
        }
    }
}
