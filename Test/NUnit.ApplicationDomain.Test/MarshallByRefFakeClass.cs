using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{

  // we currently do not have a good way of enforcing that MarshallByRef only comes from the parent domain
  // so for now we allow it and allow the user to shoot themselves
  // 
  //[RunInApplicationDomain]
  //internal class SharedDataStoreFlowTests4
  //{
  //  private const string MarshallKey = "A.Marshall.Key";

  //  [Test]
  //  public void SetMarshallValueInTest_DoesNotWorks()
  //  {
  //    Assert.Throws<InvalidOperationException>(
  //      () => AppDomainRunner.DataStore.Set(MarshallKey, new MarshallByRefFakeClass()));
  //  }
  //}

  public class MarshallByRefFakeClass : MarshalByRefObject
  {
  }
}