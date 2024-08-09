using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  [RunInApplicationDomain]
  public class MultipleTestFixtureSetupAndTestFixtureTeardownTests : MultipleTestFixtureSetupAndTestFixtureTeardownTestsBase
  {
    [OneTimeSetUp]
    public new void TestFixtureSetup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived TestFixtureSetup", 2);
    }

    [Test]
    [Description("Test method")]
    public void Test_method()
    {
      Verify("Test Method", 3);
    }

    [OneTimeTearDown]
    public new void TestFixtureTeardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived TestFixtureTeardown", 4);
    }
  }
}