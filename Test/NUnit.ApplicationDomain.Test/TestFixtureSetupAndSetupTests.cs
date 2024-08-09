using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  [RunInApplicationDomain]
  public class TestFixtureSetupAndSetupTests : TestFixtureSetupAndSetupTestsBase
  {
    [SetUp]
    public new void Setup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived Setup", 2);
    }

    [OneTimeSetUp]
    public new void TestFixtureSetup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived TestFixtureSetup", 4);
    }

    /// <summary>
    /// Expected calls:
    /// Base Setup
    ///   Derived Setup
    ///     Base TestFixtureSetup
    ///       Derived TestFixtureSetup
    ///         Test Method
    ///       Derived TestFixtureTeardown
    ///     Base TestFixtureTeardown
    ///   Derived Teardown
    /// Base Teardown
    /// </summary>
    [Test]
    [Description("Test method")]
    public void Test_method()
    {
      Verify("Test Method", 5);
    }

    [OneTimeTearDown]
    public new void TestFixtureTeardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived TestFixtureTeardown", 6);
    }

    [TearDown]
    public new void Teardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived Teardown", 8);
    }
  }
}