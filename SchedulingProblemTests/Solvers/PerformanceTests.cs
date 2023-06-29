using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SchedulingProblemTests.Model.Solvers
{
    /// <summary>
    /// Test class used to measure time (in nanoseconds) in order to optimize performance
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        [DataTestMethod]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(1000000)]
        [DataRow(1000000000)]
        public void ForLoopTest(int size)
        {
            // Arrange
            int[] data = new int[size];

            // Act
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < size; i++)
            {
                data[i] = i;
            }

            sw.Stop();

            // Assert
            Console.Write("ForLoop :- " + sw.ElapsedTicks + "\n");
        }

        [DataTestMethod]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(1000000)]
        [DataRow(1000000000)]
        public void ForeachLoopTest(int size)
        {
            // Arrange
            int[] data = new int[size];

            // Act
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (int i in data)
            {
                data[i] = i;
            }

            sw.Stop();

            // Assert
            Console.Write("ForeachLoop :- " + sw.ElapsedTicks + "\n");
        }

        [DataTestMethod]
        [DataRow(10)]
        [DataRow(100)]
        public void NestedForLoopWithDictionaryTest(int size)
        {
            // Arrange
            var data = new Dictionary<(int, int, int), int>(size);

            // Act
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        data[(i, j, k)] = i;
                    }
                }
            }

            sw.Stop();

            // Assert
            Console.Write("ForLoop :- " + sw.ElapsedTicks + "\n");
        }

        [DataTestMethod]
        [DataRow(10)]
        [DataRow(100)]
        public void NestedForLoopWithMultiDimensionalArrayTest(int size)
        {
            // Arrange
            int[,,] data = new int[size, size, size];

            // Act
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    for (int k = 0; k < data.GetLength(2); k++)
                    {
                        data[i, j, k] = i;
                    }
                }
            }

            sw.Stop();

            // Assert
            Console.Write("ForLoop :- " + sw.ElapsedTicks + "\n");
        }

        [DataTestMethod]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        public void NestedForeachLoopWithMultiDimensionalArrayTest(int size)
        {
            // Arrange
            int[,,] data = new int[size, size, size];

            // Act
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (int i in new int[size])
            {
                foreach (int j in new int[size])
                {
                    foreach (int k in new int[size])
                    {
                        data[i, j, k] = i;
                    }
                }
            }

            sw.Stop();

            // Assert
            Console.Write("ForeachLoop :- " + sw.ElapsedTicks + "\n");
        }

        [DataTestMethod]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        public void NestedForLoopWithJaggedArrayTest(int size)
        {
            // Arrange
            int[][][] data = new int[size][][];
            for (int i = 0; i < size; i++)
            {
                int[][] data2 = new int[size][];
                for (int j = 0; j < size; j++)
                {
                    int[] data3 = new int[size];
                    data2[j] = data3;
                }

                data[i] = data2;
            }

            // Act
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        data[i][j][k] = i;
                    }
                }
            }

            sw.Stop();

            // Assert
            Console.Write("NestedForLoop :- " + sw.ElapsedTicks + "\n");
        }

        [DataTestMethod]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        public void NestedForeachLoopWithJaggedArrayTest(int size)
        {
            // Arrange
            int[][][] data = new int[size][][];
            for (int i = 0; i < size; i++)
            {
                int[][] data2 = new int[size][];
                for (int j = 0; j < size; j++)
                {
                    int[] data3 = new int[size];
                    data2[j] = data3;
                }

                data[i] = data2;
            }

            // Act
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (int i in new int[size])
            {
                foreach (int j in new int[size])
                {
                    foreach (int k in new int[size])
                    {
                        data[i][j][k] = i;
                    }
                }
            }

            sw.Stop();

            // Assert
            Console.Write("NestedForeachLoop :- " + sw.ElapsedTicks + "\n");
        }
    }
}
