namespace NUnit.ApplicationDomain.Internal;

using Contracts;
using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Reflection;
using AppDomain = System.AppDomain;

/// <summary> Utility methods. </summary>
internal static class Utils
{
    extension(Type? typeUnderTest)
    {
        /// <summary>
        ///  Get all methods in the type's hierarchy that have the designated attribute.
        /// </summary>
        /// <typeparam name="T">The attribute.</typeparam>
        /// <returns>
        ///  Returns methods further down in the type hierarchy first, followed by each subsequent type's
        ///  parents' methods.
        /// </returns>
        public List<MethodInfo> GetMethodsWithAttribute<T>()
            where T : Attribute
        {
            List<MethodInfo> methodsFound = [];

            while (typeUnderTest is not null)
            {
                const BindingFlags searchFlags = BindingFlags.DeclaredOnly
                                                    | BindingFlags.Instance
                                                    | BindingFlags.Public
                                                    | BindingFlags.NonPublic;

                // get only methods that do not have any parameters
                IEnumerable<MethodInfo> methodsOnCurrentType = from method in typeUnderTest.GetMethods(searchFlags)
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
    }

    extension(AppDomain domain)
    {
#if NET8_0_OR_GREATER
        /// <summary>
        /// Create an instance of the object in the given domain.
        /// </summary>
        /// <typeparam name="T">The type of the object to construct.</typeparam>
        /// <param name="usePublicConstructor"><see langword="true"/> to use a public constructor.</param>
        /// <param name="args">Arguments for the constructor.</param>
        /// <returns>An instance of T, unwrapped from the domain.</returns>
        internal object? CreateInstanceAndUnwrap<T>(bool usePublicConstructor = false, params ReadOnlySpan<object> args)
            => domain.CreateInstanceAndUnwrap(typeof(T).Assembly.Location, Contract.AssertNotNull(typeof(T).FullName), usePublicConstructor, args);
#else
        /// <summary>
        /// Create an instance of the object in the given domain.
        /// </summary>
        /// <typeparam name="T">The type of the object to construct.</typeparam>
        /// <returns>An instance of T, unwrapped from the domain.</returns>
        internal T? CreateInstanceAndUnwrap<T>()
            => (T?)domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, Contract.AssertNotNull(typeof(T).FullName));
#endif
    }
}
