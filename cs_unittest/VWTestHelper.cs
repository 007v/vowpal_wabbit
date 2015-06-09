﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.MachineLearning;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Atn;

namespace cs_unittest
{
    internal static class VWTestHelper
    {
        internal static void ParseInput(Stream stream, IParseTreeListener listener)
        {
            try
            {
                // optimized for memory consumption
                var antlrStream = new UnbufferedCharStream(stream);
                var lexer = new VowpalWabbitLexer(antlrStream)
                {
                    TokenFactory = new CommonTokenFactory(copyText: true)
                };

                var tokens = new UnbufferedTokenStream(lexer);
                var parser = new VowpalWabbitParser(tokens)
                {
                    BuildParseTree = false,
                };
                // fast than LL(*)
                parser.Interpreter.PredictionMode = PredictionMode.Sll;

                parser.AddParseListener(listener);
                parser.AddErrorListener(new TestErrorListener());
                parser.start();
            }
            finally
            {
                stream.Dispose();
            }
        }
        internal static void AssertEqual(string expectedFile, VowpalWabbitPerformanceStatistics actual)
        {
            var expectedPerformanceStatistics = VWTestHelper.ReadPerformanceStatistics(expectedFile);
            AssertEqual(expectedPerformanceStatistics, actual);
        }

        internal static void AssertEqual(VowpalWabbitPerformanceStatistics expected, VowpalWabbitPerformanceStatistics actual)
        {
            if (expected.TotalNumberOfFeatures != actual.TotalNumberOfFeatures)
            {
                Console.Error.WriteLine(
                    "Warning: total number of features differs. Expected: {0} vs. actual: {1}",
                    expected.TotalNumberOfFeatures,
                    actual.TotalNumberOfFeatures);
            }

            Assert.AreEqual(expected.NumberOfExamplesPerPass, actual.NumberOfExamplesPerPass);
            Assert.AreEqual(expected.AverageLoss, actual.AverageLoss, 1e-5);
            Assert.AreEqual(expected.BestConstant, actual.BestConstant, 1e-5);
            Assert.AreEqual(expected.BestConstantLoss, actual.BestConstantLoss, 1e-5);
            Assert.AreEqual(expected.WeightedExampleSum, actual.WeightedExampleSum, 1e-5);
            Assert.AreEqual(expected.WeightedLabelSum, actual.WeightedLabelSum, 1e-5);
        }

        internal static VowpalWabbitPerformanceStatistics ReadPerformanceStatistics(string filename)
        {
            var lines = File.ReadAllLines(filename);
            var stats = new VowpalWabbitPerformanceStatistics()
            {
                NumberOfExamplesPerPass = FindULongEntry(lines, "number of examples per pass = "),
                TotalNumberOfFeatures = FindULongEntry(lines, "total feature number = "),
                AverageLoss = FindDoubleEntry(lines, "average loss = "),
                BestConstant = FindDoubleEntry(lines, "best constant = "),
                BestConstantLoss = FindDoubleEntry(lines, "best constant's loss = "),
                WeightedExampleSum = FindDoubleEntry(lines, "weighted example sum = "),
                WeightedLabelSum = FindDoubleEntry(lines, "weighted label sum = ")
            };

            return stats;
        }

        private static double FindDoubleEntry(string[] lines, string label)
        {
            var candidate = lines.FirstOrDefault(l => l.StartsWith(label));

            if (candidate == null)
            {
                return 0.0;
            }

            var ret = 0.0;
            if (double.TryParse(candidate.Substring(label.Length), NumberStyles.Float, CultureInfo.InvariantCulture, out ret))
            {
                return ret;   
            }

            return 0.0;
        }

        private static ulong FindULongEntry(string[] lines, string label)
        {
            var candidate = lines.FirstOrDefault(l => l.StartsWith(label));

            if (candidate == null)
            {
                return 0L;
            }

            ulong ret = 0L;
            if (ulong.TryParse(candidate.Substring(label.Length), NumberStyles.Float, CultureInfo.InvariantCulture, out ret))
            {
                return ret;
            }

            return 0L;
        }
    }
}
