#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System;

public class AppDomainSetup
{
    public string? ApplicationBase { get; set; }
    public string? ConfigurationFile { get; set; }
}

#endif