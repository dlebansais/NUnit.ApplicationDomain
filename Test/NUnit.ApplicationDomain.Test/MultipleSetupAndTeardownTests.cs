using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  [RunInApplicationDomain]
  public class MultipleSetupAndTeardownTests : MultipleSetupAndTeardownTestsBase
  {
    [SetUp]
    public new void Setup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived Setup", 2);
    }

    [Test]
    [Description("Test method")]
    public void Test_method()
    {
      Verify("Test Method", 3);
    }

    [TearDown]
    public new void Teardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived Teardown", 4);
    }
  }
}