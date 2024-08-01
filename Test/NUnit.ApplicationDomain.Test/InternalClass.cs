using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  internal class InternalClass
  {
    [Test, RunInApplicationDomain]
    public void FirstTest()
    {
      // we expect the first test to pass
      Assert.That(13, Is.EqualTo(StaticInformation.Count));
      StaticInformation.Count++;

      Console.WriteLine("In Application domain");
    }

    [Test, RunInApplicationDomain]
    public void SecondTest()
    {
      Assert.That(13, Is.EqualTo(StaticInformation.Count));
      // if we weren't in a separate app domain, the next test would fail:
      StaticInformation.Count++;

      Console.WriteLine("In Application domain");
    }
  }
}