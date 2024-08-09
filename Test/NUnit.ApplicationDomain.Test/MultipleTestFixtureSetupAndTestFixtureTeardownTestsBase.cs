using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public abstract class MultipleTestFixtureSetupAndTestFixtureTeardownTestsBase
  {
    public const int StartValue = 9;
    public static int Index = StartValue;

    [OneTimeSetUp]
    public void TestFixtureSetup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base TestFixtureSetup", 1);
    }

    [OneTimeTearDown]
    public void TestFixtureTeardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base TestFixtureTeardown", 5);
    }

    protected void Verify(string text, int expectedIncrement)
    {
      Console.WriteLine(text);

      Index++;
      Assert.That(StartValue + expectedIncrement, Is.EqualTo(Index));
    }
  }
}