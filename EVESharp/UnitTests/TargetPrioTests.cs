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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;


    public class DirectEntity
    {
        public int Id { get; set; }
        public int AbyssalTargetPriority { get; set; }
        public double Distance { get; set; }

        public double DistanceTo(IEnumerable<DirectEntity> otherEntities)
        {
            if (!otherEntities.Any())
                return 0;

            var totalDist = 0d;
            var entCount = otherEntities.Count();

            foreach (var entity in otherEntities)
            {
                totalDist += this.DistanceTo(entity);
            }
            var avg = totalDist / entCount;
            //Console.WriteLine($"avg [{avg}] totalDist [{totalDist}] entCount [{entCount}]");
            return avg;
        }
        public double DistanceTo(DirectEntity otherEnt)
        {
            // ye, this does not work in real 3d space, but it should be sufficient for the test
            return Math.Abs(otherEnt.Distance - Distance);
        }
        public override string ToString()
        {
            return $"Id: {Id}, AbyssalTargetPriority: {AbyssalTargetPriority}, Distance: {Distance}";
        }
    }

    public class TestClass
    {
        public List<DirectEntity> GetSortedTargetList(IEnumerable<DirectEntity> list,
         IEnumerable<DirectEntity> averageDistEntities = null, long _group1OrSingleTargetId = 0, long _group2TargetId = 0)
        {
            return list.OrderBy(e => e.Id)
                //.When(false, enumerable => enumerable.ForEachInLine(e => Console.WriteLine(e)))

                .OrderBy(e => e.AbyssalTargetPriority)
                .ThenByDescending(e => e.Id == _group1OrSingleTargetId || e.Id == _group2TargetId)
                .ThenBy(e =>
                averageDistEntities == null || !averageDistEntities.Any()
                    ? (int)(e.Distance / 5000)
                    : (int)(e.DistanceTo(averageDistEntities) / 5000)).ToList();
        }
    }

    [TestClass]
    public class TargetPrioTests
    {
        [TestMethod]
        public void GetSortedTargetList_WithEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var targetList = new List<DirectEntity>();
            var testInstance = new TestClass();

            // Act
            var sortedList = testInstance.GetSortedTargetList(targetList);

            foreach (var entry in sortedList)
            {
                Console.WriteLine(entry);
            }

            // Assert
            CollectionAssert.AreEqual(targetList, sortedList);
        }

        [TestMethod]
        public void GetSortedTargetList_WithAverageDistEntitiesNull_ShouldSortByDistance()
        {
            // Arrange
            var targetList = new List<DirectEntity>
        {
            new DirectEntity { Id = 3, AbyssalTargetPriority = 10, Distance = 20000 },
            new DirectEntity { Id = 2, AbyssalTargetPriority = 8, Distance = 15000 },
            new DirectEntity { Id = 1, AbyssalTargetPriority = 5, Distance = 10000 },
        };
            var testInstance = new TestClass();

            // Act
            var sortedList = testInstance.GetSortedTargetList(targetList);

            foreach (var entry in sortedList)
            {
                Console.WriteLine(entry);
            }

            // Assert
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, sortedList.Select(e => e.Id).ToArray());
        }

        [TestMethod]
        public void GetSortedTargetList_WithAverageDistEntities_ShouldSortByDistanceToAverageDistEntities()
        {
            var averageDistEntities = new List<DirectEntity>
        {
            new DirectEntity { Id = 6, AbyssalTargetPriority = 3, Distance = 100_000 },
            new DirectEntity { Id = 5, AbyssalTargetPriority = 2, Distance = 100_000 },
            new DirectEntity { Id = 4, AbyssalTargetPriority = 1, Distance = 100_000 },
        };

            var targetList = new List<DirectEntity>
        {
            new DirectEntity { Id = 3, AbyssalTargetPriority = 8, Distance = 20000 },
            new DirectEntity { Id = 2, AbyssalTargetPriority = 8, Distance = 15000 },
            new DirectEntity { Id = 1, AbyssalTargetPriority = 8, Distance = 10000 },
        };
            var testInstance = new TestClass();

            var sortedList = testInstance.GetSortedTargetList(targetList, averageDistEntities);

            foreach (var entry in sortedList)
            {
                Console.WriteLine(entry);
            }

            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, sortedList.Select(e => e.Id).ToArray());
        }

        [TestMethod]
        public void GetSortedTargetList1_WithIds()
        {
            var averageDistEntities = new List<DirectEntity>
        {
            new DirectEntity { Id = 6, AbyssalTargetPriority = 3, Distance = 100_000 },
            new DirectEntity { Id = 5, AbyssalTargetPriority = 2, Distance = 100_000 },
            new DirectEntity { Id = 4, AbyssalTargetPriority = 1, Distance = 100_000 },
        };

            var targetList = new List<DirectEntity>
        {
            new DirectEntity { Id = 3, AbyssalTargetPriority = 8, Distance = 20000 },
            new DirectEntity { Id = 2, AbyssalTargetPriority = 8, Distance = 15000 },
            new DirectEntity { Id = 1, AbyssalTargetPriority = 8, Distance = 10000 },
        };

            long _group1OrSingleTargetId = 1;
            long _group2TargetId = 3;

            var testInstance = new TestClass();

            var sortedList = testInstance.GetSortedTargetList(targetList, averageDistEntities, _group1OrSingleTargetId, _group2TargetId);

            foreach (var entry in sortedList)
            {
                Console.WriteLine(entry);
            }

            CollectionAssert.AreEqual(new[] { 3, 1, 2 }, sortedList.Select(e => e.Id).ToArray());
        }
    }
}
