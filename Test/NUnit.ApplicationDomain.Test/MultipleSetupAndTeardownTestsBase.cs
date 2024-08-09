using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public abstract class MultipleSetupAndTeardownTestsBase
  {
    public const int StartValue = 9;
    public static int Index = StartValue;

    [SetUp]
    public void Setup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base Setup", 1);
    }

    [TearDown]
    public void Teardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base Teardown", 5);
    }

    protected void Verify(string text, int expectedIncrement)
    {
      Console.WriteLine(text);

      Index++;
      Assert.That(StartValue + expectedIncrement, Is.EqualTo(Index));
    }
  }
}