using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Konata.Core.Message.Model;

namespace Kagami.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task OnCommandGithubParser()
    {
        var textChain = TextChain.Create
            ("https://github.com/KonataDev/Kagami");
        {
            // Get result
            var result = await Command
                .OnCommandGithubParser(textChain);

            Console.WriteLine(result.Build());
        }
        Assert.Pass();
    }
}
