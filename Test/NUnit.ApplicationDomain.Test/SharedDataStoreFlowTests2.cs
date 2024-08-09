using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  internal class SharedDataStoreFlowTests2
  {
    private const string StringKey = "A.String.Key";
    private const string StringValue = "A.String.Value";

    [Test, RunInApplicationDomain]
    public void SetSerialiableValueInTest_Works()
    {
      AppDomainRunner.DataStore.Set(StringKey, StringValue);
        }

        [TearDown]
    public void GetSerialiableValueInTest_Works()
    {
            var actualStringValue = AppDomainRunner.DataStore.Get<string>(StringKey);
            Assert.That(actualStringValue, Is.EqualTo(StringValue));
        }
    }
}