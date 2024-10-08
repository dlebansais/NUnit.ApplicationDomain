﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public class RunInAppDomainAttributeTests
  {
    [SetUp]
    public void Setup()
    {
      Console.WriteLine("Setup Running...");
      StaticInformation.Count++;
    }

    [Test, RunInApplicationDomainAttribute]
    [Description("Each test is in different appdomain")]
    public void Each_test_is_in_different_appdomain()
    {
      Assert.That(14, Is.EqualTo(StaticInformation.Count));
      // if we weren't in a separate app domain, the next test would fail:
      StaticInformation.Count++;
    }

    [Test, RunInApplicationDomainAttribute]
    [Description("Each test is in different appdomain")]
    public void Each_test_is_in_different_appdomain2()
    {
      Assert.That(14, Is.EqualTo(StaticInformation.Count));
      // if we weren't in a separate app domain, the next test would fail:
      StaticInformation.Count++;
    }

    [TearDown]
    public void Teardown()
    {
      Console.WriteLine("Teardown called");
    }
  }
}