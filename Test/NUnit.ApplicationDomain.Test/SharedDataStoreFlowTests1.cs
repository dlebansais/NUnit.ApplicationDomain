using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  internal class SharedDataStoreFlowTests1
  {
    private const string StringKey = "A.String.Key";
    private const string StringValue = "A.String.Value";

    [SetUp]
    public void Setup()
    {
      if (AppDomainRunner.IsNotInTestAppDomain)
      {
        AppDomainRunner.DataStore.Set(StringKey, StringValue);
      }
    }

    [Test, RunInApplicationDomain]
    public void GetSerialiableValueInTest_Works()
    {
      var actualStringValue = AppDomainRunner.DataStore.Get<string>(StringKey);
      Assert.That(actualStringValue, Is.EqualTo(StringValue));
    }
  }
}