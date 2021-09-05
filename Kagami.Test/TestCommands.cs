using System;
using NUnit.Framework;
using Kagami.Function;
using Konata.Core.Message.Model;

namespace Kagami.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestPing()
        {
            Console.WriteLine(Command.OnCommandPing
                (PlainTextChain.Create("/ping")).Build().ToString());
            Assert.Pass();
        }

        [Test]
        public void OnCommandBvParser()
        {
            Console.WriteLine(Command.OnCommandBvParser
                (PlainTextChain.Create("BV1Qh411i7ic")).Build().ToString());
            Assert.Pass();
        }

        [Test]
        public void OnCommandGithubParser()
        {
            Console.WriteLine(Command.OnCommandGithubParser
                (PlainTextChain.Create("https://github.com/KonataDev/Kagami")).Build().ToString());
            Assert.Pass();
        }
    }
}
