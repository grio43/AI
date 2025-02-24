using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EVESharpCore.Framework;
using SharedComponents.Utility;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class BenchmarkClass
    {

        private static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        [Benchmark]
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
        }

        [Benchmark]
        public void IntersectsRectangle_Intersection_ReturnsTrue()
        {
            // Arrange
            var startPoint = new Vec3(30, 30, 30);  // Inside the box
            var targetPoint = new Vec3(70, 70, 70); // Inside the box
            var rectangle = new DirectMiniBox(40, 40, 40, 60, 60, 60, 50, 50, 50, 40, 40, 40);

            // Act
            var result = DirectRayCasting.IntersectsRectangle(startPoint, targetPoint, rectangle);
        }

        [Benchmark]
        public void IntersectsCapsule_NoIntersection_ReturnsFalse()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(1, 1, 1);
            var capsule = new DirectMiniCapsule(100, 100, 100, 300, 300, 300, 50);

            // Act
            var result = DirectRayCasting.IntersectsCapsule(startPoint, targetPoint, capsule);

            // Assert
        }

        [Benchmark]
        public void IntersectsSphere_Intersection_ReturnsTrue()
        {
            // Arrange
            var startPoint = new Vec3(0, 0, 0);
            var targetPoint = new Vec3(200, 200, 200); // Updated target point to create an intersection
            var sphere = new DirectMiniBall(201, 201, 201, 2); // Increase the sphere radius

            // Act
            var result = DirectRayCasting.IntersectsSphere(startPoint, targetPoint, sphere);

            // Assert
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkClass>();
        }
    }
}