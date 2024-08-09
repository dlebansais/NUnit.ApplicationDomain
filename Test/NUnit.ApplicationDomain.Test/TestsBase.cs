using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public abstract class TestsBase
  {
    [Test, RunInApplicationDomain]
    public void SomeTest()
    {
    }

    protected abstract void SomeChildSetup();
  }
}