using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public static class StaticInformation
  {
    private static int _count;

    static StaticInformation()
    {
      _count = 13;
    }

    // property so that it can be debugged
    public static int Count
    {
      get { return _count; }
      set { _count = value; }
    }
  }
}