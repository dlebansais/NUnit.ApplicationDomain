#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System;

using global::System;

public class AppDomainSetup : MarshalByRefObject
{
    internal AppDomainSetup()
    {
    }

    public AppDomainSetup(string? applicationBase, string? configurationFile)
    {
        ApplicationBase = applicationBase;
        ConfigurationFile = configurationFile;
    }

    public string? ApplicationBase { get; set; }
    public string? ConfigurationFile { get; set; }
}

#endif