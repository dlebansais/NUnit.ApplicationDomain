// We don't try to support this in .NET 8
#if !NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  internal class AppConfigTests
  {
    [Test, RunInApplicationDomain]
    public void VerifyAppConfig_IsLoaded()
    {
      string configValue = ConfigurationManager.AppSettings["verify"];

      Assert.That(configValue, Is.EqualTo("that-it-loaded"));
    }
  }
}
#endif
