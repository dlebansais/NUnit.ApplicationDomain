﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  [TestFixture]
  public class ActualTestClass : TestsBase
  {
    [Test, RunInApplicationDomain]
    [Description("Each test is in different appdomain, even works in abstract classes")]
    public void Test1()
    {
      Assert.That(13, Is.EqualTo(StaticInformation.Count));
      // if we weren't in a separate app domain, the next test would fail:
      StaticInformation.Count++;

      Console.WriteLine("In Application domain");
    }

    [Test, RunInApplicationDomain]
    [Description("Each test is in different appdomain, even works in abstract classes")]
    public void Test2()
    {
      Assert.That(13, Is.EqualTo(StaticInformation.Count));
      // if we weren't in a separate app domain, the next test would fail:
      StaticInformation.Count++;

      Console.WriteLine("In Application domain");
    }

    protected override void SomeChildSetup()
    {
    }
  }
}