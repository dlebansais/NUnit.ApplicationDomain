using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public abstract class TestFixtureSetupAndSetupTestsBase
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

    [OneTimeSetUp]
    public void TestFixtureSetup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base TestFixtureSetup", 3);
    }

    [OneTimeTearDown]
    public void TestFixtureTeardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base TestFixtureTeardown", 7);
    }

    [TearDown]
    public void Teardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base Teardown", 9);
    }

    protected void Verify(string text, int expectedIncrement)
    {
      Console.WriteLine(text);

      Index++;
      Assert.That(StartValue + expectedIncrement, Is.EqualTo(Index));
    }
  }
}