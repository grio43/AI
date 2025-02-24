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

    public class AbyssalDrone
    {
        public long DroneId { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int Bandwidth { get; set; }
        public bool IsAssigned { get; set; }

        public AbyssalDrone(long droneId, int typeId, string typeName, int bandwidth)
        {
            DroneId = droneId;
            TypeId = typeId;
            TypeName = typeName;
            Bandwidth = bandwidth;
            IsAssigned = false;
        }
    }
    [TestClass]
    public class DroneSelector
    {
        public List<AbyssalDrone> GetHighestBandwidthAndAmountDrones(List<AbyssalDrone> drones, int maxDrones, int maxBandwidth)
        {
            if (drones.Count <= maxDrones)
                maxDrones = drones.Count;

            if (drones.Sum(d => d.Bandwidth) <= maxBandwidth)
                maxBandwidth = drones.Sum(d => d.Bandwidth);

            // Sort the original list in descending order by bandwidth
            drones = drones.OrderByDescending(d => d.Bandwidth).ToList();

            // Create a 2D array to store the dynamic programming results
            int[,] dp = new int[maxDrones + 1, maxBandwidth + 1];

            // Iterate through each drone in the sorted list
            for (int i = 0; i < drones.Count; i++)
            {
                AbyssalDrone currentDrone = drones[i];

                // Iterate from the maximum number of drones down to 1
                for (int j = maxDrones; j >= 1; j--)
                {
                    // Iterate from the maximum bandwidth down to the current drone's bandwidth
                    for (int k = maxBandwidth; k >= (int)currentDrone.Bandwidth; k--)
                    {
                        // Calculate the new bandwidth value if the current drone is selected
                        int newBandwidth = dp[j - 1, k - (int)currentDrone.Bandwidth] + (int)currentDrone.Bandwidth;

                        // Update the maximum bandwidth value in the DP array
                        if (newBandwidth > dp[j, k])
                        {
                            dp[j, k] = newBandwidth;
                        }
                    }
                }
            }

            List<AbyssalDrone> returnedDrones = new List<AbyssalDrone>();
            List<List<AbyssalDrone>> dronesCache = new List<List<AbyssalDrone>>();

            for (int j = drones.Count - 1; j >= 0; j--)
            {
                // Initialize variables to track remaining bandwidth and number of drones
                int remainingBandwidth = maxBandwidth;
                int remainingDrones = maxDrones;
                // Make a copy of the original list to choose drones from
                var dronesToChooseFrom = drones.ToList();
                // Create a list to store the selected drones
                List<AbyssalDrone> selectedDrones = new List<AbyssalDrone>();
                // Iterate through the sorted list in reverse order
                for (int i = j; i >= 0; i--)
                {
                    AbyssalDrone currentDrone = drones[i % drones.Count];

                    // Check if the current drone can be selected based on remaining bandwidth, remaining drones,
                    // and if selecting it leads to the maximum bandwidth value
                    if (currentDrone.Bandwidth <= remainingBandwidth && remainingDrones > 0 &&
                dp[remainingDrones, remainingBandwidth] == dp[remainingDrones - 1, remainingBandwidth - (int)currentDrone.Bandwidth] + (int)currentDrone.Bandwidth)
                    {
                        // Find the corresponding drone from the original list using the typeId. By that we will ensure that we always pick the lowest index from the initial list.
                        // But since we are picking items out of order the resulting list order is not stable. Which should not matter however in context of drones.
                        // TODO: While launching drones we should ensure that the order is at least the same as in the drone bay (Might be a detection vector)
                        // Edit: Eve ensures the order already for us :)
                        var d = dronesToChooseFrom.FirstOrDefault(x => x.TypeId == currentDrone.TypeId);
                        if (d != null)
                        {
                            // Add the selected drone to the list
                            selectedDrones.Add(d);

                            // Remove the selected drone from the drones to choose from
                            dronesToChooseFrom.Remove(d);

                            // Update remaining bandwidth and number of drones
                            remainingBandwidth -= (int)currentDrone.Bandwidth;
                            remainingDrones--;
                        }
                    }
                }
                if (selectedDrones.Any())
                {
                    dronesCache.Add(selectedDrones);
                }
            }

            if (dronesCache.Count > 0)
            {
                var highestBW = dronesCache.Where(e => e.Sum(d => d.Bandwidth) <= maxBandwidth).OrderByDescending(e => e.Sum(d => d.Bandwidth)).FirstOrDefault().Sum(d => d.Bandwidth);
                // Select the highest drone amount from the group with highest bandwidth
                //Debug.WriteLine($"Highest BW {highestBW}");
                returnedDrones = dronesCache.Where(e => e.Sum(d => d.Bandwidth) == highestBW).OrderByDescending(e => e.Count).FirstOrDefault();
            }

            // Return the list of selected drones
            return returnedDrones;
        }

    }
    [TestClass]
    public class MaxDroneMaxDroneBandwidthSelectionTests
    {


        [TestMethod]
        public void Test1()
        {
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
        {
            new AbyssalDrone(1, 1005, "Drone A", 5),
            new AbyssalDrone(2, 1005, "Drone B", 5),
            new AbyssalDrone(3, 1005, "Drone C", 5),
            new AbyssalDrone(4, 1005, "Drone D", 5),
            new AbyssalDrone(5, 1005, "Drone E", 5),
            new AbyssalDrone(6, 1005, "Drone F", 5),
            new AbyssalDrone(7, 1005, "Drone G", 5),
            new AbyssalDrone(8, 1005, "Drone H", 5),
            new AbyssalDrone(9, 1005, "Drone I", 5),
            new AbyssalDrone(10, 1025, "Drone J", 25),
            new AbyssalDrone(11, 1010, "Drone K", 10),
            new AbyssalDrone(12, 1025, "Drone L", 25),
            new AbyssalDrone(13, 1010, "Drone M", 10),
        };

            int maxDrones = 5;
            int maxBandwidth = 50;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            // the ids which should be in the list
            var ids = new List<int>() { 1, 2, 3, 10, 11 };

            Assert.IsTrue(highestBandwidthAndAmountDrones.Any() && ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id) && ids.Count == highestBandwidthAndAmountDrones.Count));
        }

        [TestMethod]
        public void Test2()
        {
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
        {
            new AbyssalDrone(1, 1050, "Drone A", 50),
            new AbyssalDrone(2, 1025, "Drone B", 25),
            new AbyssalDrone(3, 1010, "Drone C", 10),
            new AbyssalDrone(4, 1005, "Drone D", 5),
            new AbyssalDrone(5, 1050, "Drone E", 50),
            new AbyssalDrone(6, 1025, "Drone F", 25),
            new AbyssalDrone(7, 1010, "Drone G", 10),
            new AbyssalDrone(8, 1005, "Drone H", 5),
            new AbyssalDrone(9, 1050, "Drone I", 50),
            new AbyssalDrone(10, 1025, "Drone J", 25),
            new AbyssalDrone(11, 1010, "Drone K", 10),
            new AbyssalDrone(12, 1005, "Drone L", 5),
            new AbyssalDrone(13, 1050, "Drone M", 50),
            new AbyssalDrone(14, 1025, "Drone N", 25),
            new AbyssalDrone(15, 1010, "Drone O", 10),
            new AbyssalDrone(16, 1005, "Drone P", 5),
            new AbyssalDrone(17, 1050, "Drone Q", 50),
            new AbyssalDrone(18, 1025, "Drone R", 25),
            new AbyssalDrone(19, 1010, "Drone S", 10),
            new AbyssalDrone(20, 1005, "Drone T", 5),
            new AbyssalDrone(21, 1010, "Drone U", 10),
            new AbyssalDrone(22, 1025, "Drone V", 25),
            new AbyssalDrone(23, 1010, "Drone W", 10),
            new AbyssalDrone(24, 1005, "Drone X", 5),
            new AbyssalDrone(25, 1025, "Drone Y", 25),
            new AbyssalDrone(26, 1005, "Drone Z", 5)
        };

            int maxDrones = 5;
            int maxBandwidth = 125;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            var p = 0;
            for (int i = 0; i < 100; i++)
            {
                var k = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);
                var f = k.Count;
                p += f;
            }
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds / 100} milliseconds Count [{p}]");

            // the ids which should be in the list
            var ids = new List<int>() { 1, 4, 3, 5, 7 };

            Assert.IsTrue(highestBandwidthAndAmountDrones.Any() && ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id) && ids.Count == highestBandwidthAndAmountDrones.Count));
        }

        [TestMethod]
        public void Test3()
        {
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
            {

            };

            int maxDrones = 5;
            int maxBandwidth = 125;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                var k = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);
                var f = k.Count;
            }
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            // the ids which should be in the list
            var ids = new List<int>() { };

            Assert.IsTrue(ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id) && ids.Count == highestBandwidthAndAmountDrones.Count));
        }
        [TestMethod]
        public void Test4()
        {
            //Create a list of drones
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
        {
            new AbyssalDrone(1, 1025, "Drone A", 25),
            new AbyssalDrone(2, 1025, "Drone B", 25),
            new AbyssalDrone(3, 1025, "Drone C", 25),
            new AbyssalDrone(4, 1025, "Drone D", 25),
            new AbyssalDrone(5, 1025, "Drone E", 25),
            new AbyssalDrone(6, 1025, "Drone F", 25),
            new AbyssalDrone(7, 1007, "Drone G", 10),
            new AbyssalDrone(8, 1005, "Drone H", 5),
            new AbyssalDrone(9, 1050, "Drone I", 50),
            new AbyssalDrone(10, 1025, "Drone J", 25),
            new AbyssalDrone(11, 1010, "Drone K", 10),
            new AbyssalDrone(12, 1005, "Drone L", 5),
            new AbyssalDrone(13, 1050, "Drone M", 50),
            new AbyssalDrone(14, 1025, "Drone N", 25),
            new AbyssalDrone(15, 1010, "Drone O", 10),
            new AbyssalDrone(16, 1005, "Drone P", 5),
            new AbyssalDrone(17, 1050, "Drone Q", 50),
            new AbyssalDrone(18, 1025, "Drone R", 25),
            new AbyssalDrone(19, 1010, "Drone S", 10),
            new AbyssalDrone(20, 1005, "Drone T", 5),
            new AbyssalDrone(21, 1010, "Drone U", 10),
            new AbyssalDrone(22, 1025, "Drone V", 25),
            new AbyssalDrone(23, 1010, "Drone W", 10),
            new AbyssalDrone(24, 1005, "Drone X", 5),
            new AbyssalDrone(25, 1025, "Drone Y", 25),
            new AbyssalDrone(26, 1005, "Drone Z", 5)
        };

            int maxDrones = 5;
            int maxBandwidth = 125;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                var k = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);
                var f = k.Count;
            }
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            // the ids which should be in the list
            var ids = new List<int>() { };

            Assert.IsTrue(highestBandwidthAndAmountDrones.Any() && ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id) && ids.Count == highestBandwidthAndAmountDrones.Count));
        }

        [TestMethod]
        public void Test5()
        {
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
        {
            new AbyssalDrone(1, 1025, "Drone A", 25),
            new AbyssalDrone(2, 1025, "Drone B", 25),
            new AbyssalDrone(3, 1025, "Drone C", 25),
            new AbyssalDrone(4, 1025, "Drone D", 25),
            new AbyssalDrone(5, 1025, "Drone E", 25),
            new AbyssalDrone(6, 1025, "Drone F", 25),
            new AbyssalDrone(7, 1010, "Drone G", 10),
            new AbyssalDrone(8, 1005, "Drone H", 5),
            new AbyssalDrone(9, 1050, "Drone I", 50),
            new AbyssalDrone(10, 1025, "Drone J", 25),
            new AbyssalDrone(11, 1010, "Drone K", 10),
            new AbyssalDrone(12, 1005, "Drone L", 5),
            new AbyssalDrone(13, 1050, "Drone M", 50),
            new AbyssalDrone(14, 1025, "Drone N", 25),
            new AbyssalDrone(15, 1010, "Drone O", 10),
            new AbyssalDrone(16, 1005, "Drone P", 5),
            new AbyssalDrone(17, 1050, "Drone Q", 50),
            new AbyssalDrone(18, 1025, "Drone R", 25),
            new AbyssalDrone(19, 1010, "Drone S", 10),
            new AbyssalDrone(20, 1005, "Drone T", 5),
            new AbyssalDrone(21, 1010, "Drone U", 10),
            new AbyssalDrone(22, 1025, "Drone V", 25),
            new AbyssalDrone(23, 1010, "Drone W", 10),
            new AbyssalDrone(24, 1005, "Drone X", 5),
            new AbyssalDrone(25, 1025, "Drone Y", 25),
            new AbyssalDrone(26, 1005, "Drone Z", 5)
        };

            int maxDrones = 5;
            int maxBandwidth = 20;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                var k = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);
                var f = k.Count;
            }
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            // the ids which should be in the list
            var ids = new List<int>() { 8, 12, 16, 20 };

            Assert.IsTrue(highestBandwidthAndAmountDrones.Any() && ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id) && ids.Count == highestBandwidthAndAmountDrones.Count));
        }

        [TestMethod]
        public void Test6()
        {
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
        {
            new AbyssalDrone(1, 1010, "Drone A", 10),
            new AbyssalDrone(2, 1010, "Drone B", 10),
            new AbyssalDrone(3, 1010, "Drone C", 10),
            new AbyssalDrone(4, 1010, "Drone D", 10),
            new AbyssalDrone(5, 1010, "Drone E", 10),
            new AbyssalDrone(6, 1010, "Drone F", 10),
            new AbyssalDrone(7, 1010, "Drone G", 10),
            new AbyssalDrone(8, 1010, "Drone H", 10),
            new AbyssalDrone(9, 1010, "Drone I", 10),
            new AbyssalDrone(10, 1010, "Drone J", 10),
            new AbyssalDrone(11, 1010, "Drone K", 10),
            new AbyssalDrone(12, 1010, "Drone L", 10),
            new AbyssalDrone(13, 1010, "Drone M", 10),
            new AbyssalDrone(14, 1010, "Drone N", 10),
            new AbyssalDrone(15, 1010, "Drone O", 10),
            new AbyssalDrone(16, 1010, "Drone P", 10),
            new AbyssalDrone(17, 1010, "Drone Q", 10),
            new AbyssalDrone(18, 1010, "Drone R", 10),
            new AbyssalDrone(19, 1010, "Drone S", 10),
            new AbyssalDrone(20, 1010, "Drone T", 10),
            new AbyssalDrone(21, 1010, "Drone U", 10),
            new AbyssalDrone(22, 1010, "Drone V", 10),
            new AbyssalDrone(23, 1010, "Drone W", 10),
            new AbyssalDrone(24, 1010, "Drone X", 10),
            new AbyssalDrone(25, 1010, "Drone Y", 10),
            new AbyssalDrone(26, 1005, "Drone Z", 5)
        };

            int maxDrones = 5;
            int maxBandwidth = 20;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                var k = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);
                var f = k.Count;
            }
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            // the ids which should be in the list
            var ids = new List<int>() { 1, 2 };

            Assert.IsTrue(highestBandwidthAndAmountDrones.Any() && ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id) && ids.Count == highestBandwidthAndAmountDrones.Count));
        }
        [TestMethod]
        public void Test7()
        {
            //Create a list of drones
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
        {
            new AbyssalDrone(1, 1025, "Drone A", 25),
            new AbyssalDrone(2, 1025, "Drone B", 25),
            new AbyssalDrone(3, 1025, "Drone C", 25),
            new AbyssalDrone(4, 1025, "Drone D", 25),
            new AbyssalDrone(5, 1025, "Drone E", 25),
            new AbyssalDrone(6, 1025, "Drone F", 25),
            new AbyssalDrone(7, 1010, "Drone G", 10),
            new AbyssalDrone(8, 1005, "Drone H", 5),
            new AbyssalDrone(9, 1025, "Drone I", 25),
            new AbyssalDrone(10, 1025, "Drone J", 25),
            new AbyssalDrone(11, 1010, "Drone K", 10),
            new AbyssalDrone(12, 1005, "Drone L", 5),
            new AbyssalDrone(13, 1025, "Drone M", 25),
            new AbyssalDrone(14, 1025, "Drone N", 25),
            new AbyssalDrone(15, 1010, "Drone O", 10),
            new AbyssalDrone(16, 1005, "Drone P", 5),
            new AbyssalDrone(17, 1025, "Drone Q", 25),
            new AbyssalDrone(18, 1025, "Drone R", 25),
            new AbyssalDrone(19, 1010, "Drone S", 10),
            new AbyssalDrone(20, 1005, "Drone T", 5),
            new AbyssalDrone(21, 1010, "Drone U", 10),
            new AbyssalDrone(22, 1025, "Drone V", 25),
            new AbyssalDrone(23, 1010, "Drone W", 10),
            new AbyssalDrone(24, 1005, "Drone X", 5),
            new AbyssalDrone(25, 1025, "Drone Y", 25),
            new AbyssalDrone(26, 1005, "Drone Z", 5)
        };

            int maxDrones = 5;
            int maxBandwidth = 125;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                var k = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);
                var f = k.Count;
            }
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            // the ids which should be in the list
            var ids = new List<int>() { 1, 2, 3, 4, 5 };

            Assert.IsTrue(highestBandwidthAndAmountDrones.Any() && ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id)));
        }

        [TestMethod]
        public void Test8()
        {
            //Create a list of drones
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
        {
            new AbyssalDrone(1, 1005, "Drone A", 5),
            new AbyssalDrone(2, 1005, "Drone B", 5),
            new AbyssalDrone(3, 1005, "Drone C", 5),
            new AbyssalDrone(4, 1005, "Drone D", 5),
        };

            int maxDrones = 5;
            int maxBandwidth = 15;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                var k = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);
                var f = k.Count;
            }
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            // the ids which should be in the list
            var ids = new List<int>() { 1, 2, 3 };

            Assert.IsTrue(highestBandwidthAndAmountDrones.Any() && ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id) && ids.Count == highestBandwidthAndAmountDrones.Count));
        }

        [TestMethod]
        public void Test9()
        {
            //Create a list of drones
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
        {
            new AbyssalDrone(1, 1005, "Drone A", 5),
            new AbyssalDrone(2, 1005, "Drone A", 5),
        };

            int maxDrones = 5;
            int maxBandwidth = 15;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                var k = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);
                var f = k.Count;
            }
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            // the ids which should be in the list
            var ids = new List<int>() { 1, 2 };

            Assert.IsTrue(highestBandwidthAndAmountDrones.Any() && ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id) && ids.Count == highestBandwidthAndAmountDrones.Count));
        }

        [TestMethod]
        public void Test10()
        {
            //Create a list of drones
            //Create a list of drones
            List<AbyssalDrone> drones = new List<AbyssalDrone>
        {

        };

            int maxDrones = 5;
            int maxBandwidth = 15;

            DroneSelector droneSelector = new DroneSelector();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            List<AbyssalDrone> highestBandwidthAndAmountDrones = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);

            stopwatch.Stop();
            TimeSpan executionTime = stopwatch.Elapsed;

            Console.WriteLine("Selected Drones:");
            foreach (AbyssalDrone drone in highestBandwidthAndAmountDrones)
            {
                Console.WriteLine($"Drone ID: {drone.DroneId}, Bandwidth: {drone.Bandwidth}");
            }
            Console.WriteLine($"Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            for (int i = 0; i < 100; i++)
            {
                var k = droneSelector.GetHighestBandwidthAndAmountDrones(drones, maxDrones, maxBandwidth);
                var f = k.Count;
            }
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            Console.WriteLine($"TotalDrones [{highestBandwidthAndAmountDrones.Count}] TotalBandwidth [{highestBandwidthAndAmountDrones.Sum(d => d.Bandwidth)}] Execution Time: {executionTime.TotalMilliseconds} milliseconds");

            // the ids which should be in the list
            var ids = new List<int>() { };

            Assert.IsTrue(ids.All(id => highestBandwidthAndAmountDrones.Any(d => d.DroneId == id) && ids.Count == highestBandwidthAndAmountDrones.Count));
        }
    }
}