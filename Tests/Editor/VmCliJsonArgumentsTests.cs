using System;
using System.Collections.Generic;
using NUnit.Framework;
using VMUnityPipeline.Editor.Commands;

namespace VMUnityPipeline.Editor.Tests
{
    internal sealed class VmCliJsonArgumentsTests
    {
        [TestCase("2026-09-08T20:05:52.800Z")]
        [TestCase("2026-09-09T04:05:52.800+08:00")]
        [TestCase("2026-09-09")]
        public void JsonStringsRetainTheirExactTextAtEveryDepth(string value)
        {
            string json = "{\"since\":\"" + value +
                "\",\"nested\":{\"value\":\"" + value +
                "\"},\"list\":[\"" + value + "\"]}";

            Assert.That(VmCliJsonArguments.TryParseObject(json,
                out Dictionary<string, object> arguments, out string error), Is.True, error);
            Assert.That(arguments["since"], Is.TypeOf<string>().And.EqualTo(value));
            Assert.That(((Dictionary<string, object>)arguments["nested"])["value"],
                Is.TypeOf<string>().And.EqualTo(value));
            Assert.That(((List<object>)arguments["list"])[0],
                Is.TypeOf<string>().And.EqualTo(value));
        }

        [TestCase("{\"since\":1,\"since\":2}")]
        [TestCase("{\"nested\":{\"key\":1,\"key\":2}}")]
        [TestCase("{} {}")]
        [TestCase("{} trailing")]
        [TestCase("[]")]
        [TestCase("null")]
        public void InvalidObjectDocumentsRemainRejected(string json)
        {
            Assert.That(VmCliJsonArguments.TryParseObject(json,
                out Dictionary<string, object> arguments, out string error), Is.False);
            Assert.That(arguments, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void JsonPrimitiveTypesRemainUnchanged()
        {
            Assert.That(VmCliJsonArguments.TryParseObject(
                "{\"integer\":42,\"fraction\":1.25,\"boolean\":true,\"empty\":null}",
                out Dictionary<string, object> arguments, out string error), Is.True, error);
            Assert.That(arguments["integer"], Is.TypeOf<long>().And.EqualTo(42L));
            Assert.That(arguments["fraction"], Is.TypeOf<double>().And.EqualTo(1.25d));
            Assert.That(arguments["boolean"], Is.True);
            Assert.That(arguments["empty"], Is.Null);
        }

        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase("\r")]
        public void NumericLexemesRetainTheirExactBitsAcrossNestedSourceLines(string newline)
        {
            string json = "{" + newline + "\"pivot\":57.21696670694649," + newline +
                "\"nested\":{\"small\":5e-324,\"values\":[1,57.21696670694649,-1.0000000000000002]}," +
                newline + "\"text\":\"57.21696670694649\"}";
            Assert.That(VmCliJsonArguments.TryParseObject(json, out Dictionary<string, object> arguments,
                out string error), Is.True, error);
            Assert.That(BitConverter.DoubleToInt64Bits((double)arguments["pivot"]), Is.EqualTo(0x404c9bc590a753f7L));
            var nested = (Dictionary<string, object>)arguments["nested"];
            Assert.That(BitConverter.DoubleToInt64Bits((double)nested["small"]), Is.EqualTo(1L));
            var values = (List<object>)nested["values"];
            Assert.That(values[0], Is.TypeOf<long>().And.EqualTo(1L));
            Assert.That(BitConverter.DoubleToInt64Bits((double)values[1]), Is.EqualTo(0x404c9bc590a753f7L));
            Assert.That(BitConverter.DoubleToInt64Bits((double)values[2]),
                Is.EqualTo(unchecked((long)0xbff0000000000001UL)));
            Assert.That(arguments["text"], Is.TypeOf<string>().And.EqualTo("57.21696670694649"));
        }

        [TestCase("{\"value\":NaN}")]
        [TestCase("{\"value\":Infinity}")]
        [TestCase("{\"value\":1e309}")]
        [TestCase("{\"value\":010}")]
        public void NonFiniteNumericArgumentsRemainRejected(string json)
        {
            Assert.That(VmCliJsonArguments.TryParseObject(json, out Dictionary<string, object> arguments,
                out string error), Is.False);
            Assert.That(arguments, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }
    }
}
