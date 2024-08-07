﻿namespace NUnit.ApplicationDomain.Internal;

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Reflection;
#if NET8_0_OR_GREATER
using AppDomain = System.AppDomain;
#endif

/// <summary> Utility methods. </summary>
internal static class Utils
{
    /// <summary>
    ///  Get all methods in the type's hierarchy that have the designated attribute.
    /// </summary>
    /// <returns>
    ///  Returns methods further down in the type hierarchy first, followed by each subsequent type's
    ///  parents' methods.
    /// </returns>
    public static List<MethodInfo> GetMethodsWithAttribute<T>(this Type? typeUnderTest)
        where T : Attribute
    {
        var methodsFound = new List<MethodInfo>();

        while (typeUnderTest != null)
        {
            const BindingFlags searchFlags = BindingFlags.DeclaredOnly
                                                | BindingFlags.Instance
                                                | BindingFlags.Public
                                                | BindingFlags.NonPublic;

            // get only methods that do not have any parameters 
            var methodsOnCurrentType = from method in typeUnderTest.GetMethods(searchFlags)
                                        where method.GetParameters().Length == 0
                                        let attributes = (T[])method.GetCustomAttributes(typeof(T), false)
                                        where attributes.Length >= 1
                                        select method;

            methodsFound.AddRange(methodsOnCurrentType);

            // now get the Setup methods in the base type
            typeUnderTest = typeUnderTest.BaseType;
        }

        return methodsFound;
    }

    /// <summary> Create an instance of the object in the given domain. </summary>
    /// <param name="domain"> The domain in which the object should be constructed. </param>
    /// <typeparam name="T"> The type of the object to construct </typeparam>
    /// <returns> An instance of T, unwrapped from the domain. </returns>
#if NET8_0_OR_GREATER
    internal static object? CreateInstanceAndUnwrap<T>(this AppDomain domain, bool usePublicConstructor = false, params object[] args)
    {
        return domain.CreateInstanceAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName!, usePublicConstructor, args);
    }
#else
    internal static T? CreateInstanceAndUnwrap<T>(this AppDomain domain)
    {
        return (T?)domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName!, typeof(T).FullName!);
    }
#endif
}
