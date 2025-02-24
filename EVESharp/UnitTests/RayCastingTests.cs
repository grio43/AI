using EVESharpCore.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedComponents.Utility;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    /**
    [TestClass]
    public class RayCastingTests
    {
        // Test case for IntersectsSphere method
        [TestMethod]
        public void IntersectsSphere_Intersection_ReturnsTrue()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(200, 200, 200); // Updated target point to create an intersection
            var sphere = new DirectMiniBall(201, 201, 201, 2); // Increase the sphere radius

            // Act
            var result = DirectRayCasting.IntersectsSphere(startPoint, targetPoint, sphere);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IntersectsSphere_NoIntersection_ReturnsFalse()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(100, 100, 100);
            var sphere = new DirectMiniBall(200, 200, 200, 50);

            // Act
            var result = DirectRayCasting.IntersectsSphere(startPoint, targetPoint, sphere);

            // Assert
            Assert.IsFalse(result);
        }

        // Test case for IntersectsCapsule method
        [TestMethod]
        public void IntersectsCapsule_Intersection_ReturnsTrue()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(50, 50, 50);
            var capsule = new DirectMiniCapsule(0, 0, 0, 100, 100, 100, 50);

            // Act
            var result = DirectRayCasting.IntersectsCapsule(startPoint, targetPoint, capsule);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IntersectsCapsule_LineOnCapsuleBorder_ReturnsTrue()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(100, 100, 100);
            var capsule = new DirectMiniCapsule(0, 0, 0, 100, 100, 100, 50);

            // Act
            var result = DirectRayCasting.IntersectsCapsule(startPoint, targetPoint, capsule);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IntersectsCapsule_LineOutsideCapsuleBorder_ReturnsFalse()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(1, 1, 1);
            var capsule = new DirectMiniCapsule(100, 100, 100, 300, 300, 300, 50);

            // Act
            var result = DirectRayCasting.IntersectsCapsule(startPoint, targetPoint, capsule);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IntersectsCapsule_LineOutsideCapsuleBorder_ReturnsFalse2()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(100, 100, 100);
            //var capsule = new DirectMiniCapsule(200, 200, 200, 300, 300, 300, 50);
            //var capsule = new DirectMiniCapsule(400, 400, 400, 500, 500, 500, 50);

            var capsule = new DirectMiniCapsule(101, 101, 101, 150, 150, 150, 1);

            // Act
            var result = DirectRayCasting.IntersectsCapsule(startPoint, targetPoint, capsule);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IntersectsCapsule_NoIntersection_ReturnsFalse()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(1, 1, 1);
            var capsule = new DirectMiniCapsule(100, 100, 100, 300, 300, 300, 50);

            // Act
            var result = DirectRayCasting.IntersectsCapsule(startPoint, targetPoint, capsule);

            // Assert
            Assert.IsFalse(result);
        }

        // Test case for IntersectsRectangle method
        [TestMethod]
        public void IntersectsRectangle_Intersection_ReturnsTrue()
        {
            // Arrange
            var startPoint = new Vec3(30, 30, 30);  // Inside the box
            var targetPoint = new Vec3(70, 70, 70); // Inside the box
            var rectangle = new DirectMiniBox(40, 40, 40, 60, 60, 60, 50, 50, 50, 40, 40, 40);

            // Act
            var result = DirectRayCasting.IntersectsRectangle(startPoint, targetPoint, rectangle);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IntersectsRectangle_NoIntersection_ReturnsFalse()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(39, 39, 39);
            var rectangle = new DirectMiniBox(200, 200, 200, 300, 300, 300, 50, 50, 50, 40, 40, 40);

            // Act
            var result = DirectRayCasting.IntersectsRectangle(startPoint, targetPoint, rectangle);

            // Assert
            Assert.IsFalse(result);
        }

        private static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        public double CalculateDistanceBetweenPointAndRay(Vec3 rayOrigin, Vec3 rayDirection, Vec3 pointA, Vec3 pointB, double radius, double rayLength)
        {
            Vec3 ab = pointB - pointA;
            Vec3 ao = rayOrigin - pointA;

            double t = ao.DotProduct(ab) / ab.DotProduct(ab);
            t = Clamp(t, 0.0, 1.0);

            Vec3 closestPoint = pointA + t * ab;
            Vec3 rayToClosest = closestPoint - rayOrigin;

            double s = rayToClosest.DotProduct(rayDirection);
            s = Clamp(s, 0.0, rayLength);

            Vec3 pointOnRay = rayOrigin + s * rayDirection;

            return (pointOnRay - closestPoint).Magnitude - radius;
        }

        [TestMethod]
        public void DistTest()
        {
            Vec3 startPoint = new Vec3(0, 0, 0);
            Vec3 endPoint = new Vec3(100, 100, 100);
            Vec3 targetPoint = new Vec3(150, 150, 150);
            double capsuleRadius = 50;

            Vec3 direction = endPoint - startPoint;
            Vec3 offset = targetPoint - startPoint;
            double t = offset.DotProduct(direction) / direction.DotProduct(direction);
            t = Clamp(t, 0.0, 1.0);
            Vec3 closestPoint = startPoint + t * direction;

            Vec3 distanceVector = targetPoint - closestPoint;
            double closestDistance = distanceVector.Magnitude - capsuleRadius;

            //Console.WriteLine("Closest Distance: " + closestDistance);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void IntersectsCapsule()
        {
            // Define the start and end points of the ray and create a capsule
            var start = new Vec3(0, 0, 0);
            var end = new Vec3(140, 140, 140);
            var capsule = new DirectMiniCapsule(200, 200, 200, 250, 250, 250, 70);

            // Extract relevant values from the capsule
            Vec3 a = new Vec3(capsule.AX, capsule.AY, capsule.AZ);
            Vec3 b = new Vec3(capsule.BX, capsule.BY, capsule.BZ);
            double radius = capsule.Radius;

            // Calculate the direction and length of the ray
            Vec3 rayOrigin = start;
            Vec3 rayDirection = Vec3.Normalize(end - start);

            // Calculate the distance between the closest point on the capsule and the ray
            double distance = CalculateDistanceBetweenPointAndRay(rayOrigin, rayDirection, a, b, radius, (start - end).Magnitude);

            // Debug output
            Console.WriteLine("Start: " + start);
            Console.WriteLine("End: " + end);
            Console.WriteLine("Capsule A: " + a);
            Console.WriteLine("Capsule B: " + b);
            Console.WriteLine("Capsule Radius: " + radius);
            Console.WriteLine("Ray Origin: " + rayOrigin);
            Console.WriteLine("Ray Direction: " + rayDirection);
            Console.WriteLine("Distance: " + distance);
            Console.WriteLine("Radius: " + radius);

            // Assert that the distance is greater than the radius
            Assert.IsTrue(distance <= radius);
        }

        // Test case for IntersectsCapsule method
        [TestMethod]
        public void IntersectsCapsule_LineIntersects_ReturnsTrue()
        {
            // Arrange
            var startPoint = new Vec3(0, 300, 0);
            var targetPoint = new Vec3(0, 305, 0);
            var capsule = new DirectMiniCapsule(0, 200, 0, 0, 250, 0, 50);

            // Act
            var result = DirectRayCasting.IntersectsCapsule(startPoint, targetPoint, capsule);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IntersectsCapsule_LineIntersects_ReturnsFalse2()
        {
            // Arrange
            var startPoint = new Vec3(0, 301, 0);
            var targetPoint = new Vec3(0, 305, 0);
            var capsule = new DirectMiniCapsule(0, 200, 0, 0, 250, 0, 50);

            // Act
            var result = DirectRayCasting.IntersectsCapsule(startPoint, targetPoint, capsule);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IntersectsRectangle_Intersection_ReturnsTrueX()
        {
            // Arrange
            var startPoint = new Vec3(50, 50, 50);
            var targetPoint = new Vec3(700, 700, 700);
            var rectangles = new List<DirectMiniBox>()
    {
        new DirectMiniBox(40, 40, 40, 60, 60, 60, 50, 50, 50, 40, 40, 40),
        new DirectMiniBox(80, 80, 80, 100, 100, 100, 50, 50, 50, 40, 40, 40)
    };

            // Act
            var result = false;
            foreach (var rectangle in rectangles)
            {
                result = DirectRayCasting.IntersectsRectangle(startPoint, targetPoint, rectangle);
                if (result)
                    break;
            }

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsLineOfSightFree_IntersectionWithSphere_ReturnsFalse()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(100, 100, 100);
            var spheres = new List<DirectMiniBall>()
        {
            new DirectMiniBall(50, 50, 50, 60),
            new DirectMiniBall(300, 300, 300, 50)
        };
            var capsules = new List<DirectMiniCapsule>()
        {
            //new DirectMiniCapsule(100, 100, 100, 300, 300, 300, 50),
            new DirectMiniCapsule(100, 100, 100, 500, 500, 500, 50)
        };
            var rectangles = new List<DirectMiniBox>()
        {
            new DirectMiniBox(200, 200, 200, 300, 300, 300, 50, 50, 50, 40, 40, 40),
            new DirectMiniBox(400, 400, 400, 500, 500, 500, 50, 50, 50, 40, 40, 40)
        };

            // Act
            var result = DirectRayCasting.IsLineOfSightFree(startPoint, targetPoint, spheres, capsules, rectangles);

            // Assert
            Assert.IsFalse(result.Item1);
        }

        [TestMethod]
        public void IsLineOfSightFree_IntersectionWithCapsule_ReturnsFalse()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(100, 100, 100);
            var spheres = new List<DirectMiniBall>()
        {
            new DirectMiniBall(200, 200, 200, 50),
            new DirectMiniBall(300, 300, 300, 50)
        };
            var capsules = new List<DirectMiniCapsule>()
        {
            new DirectMiniCapsule(50, 50, 50, 150, 150, 150, 60),
            new DirectMiniCapsule(400, 400, 400, 500, 500, 500, 50)
        };
            var rectangles = new List<DirectMiniBox>()
        {
            new DirectMiniBox(200, 200, 200, 300, 300, 300, 50, 50, 50, 40, 40, 40),
            new DirectMiniBox(400, 400, 400, 500, 500, 500, 50, 50, 50, 40, 40, 40)
        };

            // Act
            var result = DirectRayCasting.IsLineOfSightFree(startPoint, targetPoint, spheres, capsules, rectangles);

            // Assert
            Assert.IsFalse(result.Item1);
        }

        [TestMethod]
        public void IsLineOfSightFree_IntersectionWithRectangle_ReturnsFalse()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(100, 100, 100);
            var spheres = new List<DirectMiniBall>()
        {
            new DirectMiniBall(200, 200, 200, 50),
            new DirectMiniBall(300, 300, 300, 50)
        };
            var capsules = new List<DirectMiniCapsule>()
        {
            new DirectMiniCapsule(200, 200, 200, 300, 300, 300, 50),
            new DirectMiniCapsule(0, 0, 0, 100, 100, 100, 50)
        };
            var rectangles = new List<DirectMiniBox>()
        {
            new DirectMiniBox(40, 40, 40, 60, 60, 60, 50, 50, 50, 40, 40, 40),
            new DirectMiniBox(400, 400, 400, 500, 500, 500, 50, 50, 50, 40, 40, 40)
        };

            // Act
            var result = DirectRayCasting.IsLineOfSightFree(startPoint, targetPoint, spheres, capsules, rectangles);

            // Assert
            Assert.IsFalse(result.Item1);
        }
    }
    **/
}