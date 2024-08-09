using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  internal class SharedDataStoreFlowTests3
  {
    private const string MarshallKey = "A.Marshall.Key";

    [SetUp]
    public void Setup()
    {
      if (AppDomainRunner.IsNotInTestAppDomain)
      {
        AppDomainRunner.DataStore.Set(MarshallKey, new MarshallByRefFakeClass());
      }
    }

    [Test, RunInApplicationDomain]
    public void GetMarshallByValueInTest_Works()
    {
      var actualStringValue = AppDomainRunner.DataStore.Get<MarshallByRefFakeClass>(MarshallKey);
      Assert.That(actualStringValue, Is.Not.Null);
    }
  }
}