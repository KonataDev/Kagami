using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Kagami.Function;
using Konata.Core.Message;
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
        public async Task OnCommandBvParser()
        {
            var textChain = PlainTextChain
                .Create("BV1Qh411i7ic");
            {
                // Get result
                var result = await Command
                    .OnCommandBvParser(textChain);

                Console.WriteLine(result.Build());
            }
            Assert.Pass();
        }

        [Test]
        public async Task OnCommandGithubParser()
        {
            var textChain = PlainTextChain.Create
                ("https://github.com/KonataDev/Kagami");
            {
                // Get result
                var result = await Command
                    .OnCommandGithubParser(textChain);

                Console.WriteLine(result.Build());
            }
            Assert.Pass();
        }

        [Test]
        public void OnCommandEcho()
        {
            var textChain = PlainTextChain.Create("/echo =w=");
            var messageChain = new MessageBuilder(textChain);
            {
                // Get result
                var result = Command.OnCommandEcho
                    (textChain, messageChain.Build());

                Console.WriteLine(result.Build().ToString());
            }
            Assert.Pass();
        }

        [Test]
        public void OnCommandEval()
        {
            var messageChain = new MessageBuilder
                ("/eval =w=");
            {
                Console.WriteLine(Command.OnCommandEval
                    (messageChain.Build()).Build());
            }
            Assert.Pass();
        }
    }
}
