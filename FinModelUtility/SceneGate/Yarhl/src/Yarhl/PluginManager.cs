// Copyright (c) 2019 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Yarhl;

using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using FileFormat;

/// <summary>
/// Plugin manager.
/// </summary>
/// <remarks>
/// <para>Plugin assemblies are loaded from the directory with the Yarhl
/// assembly and the 'Plugins' subfolder with its children.</para>
/// </remarks>
public sealed class PluginManager
{
    static readonly string[] IgnoredLibraries = [
        "System.",
        "Microsoft.",
        "netstandard",
        "nuget",
        "nunit",
        "testhost"
    ];

    static readonly object LockObj = new object();
    static PluginManager? singleInstance;

    readonly CompositionHost container;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManager"/> class.
    /// </summary>
    PluginManager()
    {
            container = InitializeContainer();
        }

    /// <summary>
    /// Gets the name of the plugins directory.
    /// </summary>
    public static string PluginDirectory => "Plugins";

    /// <summary>
    /// Gets the plugin manager instance.
    /// </summary>
    /// <remarks><para>It initializes the manager if needed.</para></remarks>
    public static PluginManager Instance {
        get {
                if (singleInstance == null) {
                    lock (LockObj) {
                        if (singleInstance == null)
                            singleInstance = new PluginManager();
                    }
                }

                return singleInstance;
            }
    }

    /// <summary>
    /// Finds all the extensions from the given base type.
    /// </summary>
    /// <returns>The extensions.</returns>
    /// <typeparam name="T">Type of the extension point.</typeparam>
    public IEnumerable<T> FindExtensions<T>()
    {
            return container.GetExports<T>();
        }

    /// <summary>
    /// Finds all the extensions from the given base type.
    /// </summary>
    /// <returns>The extensions.</returns>
    /// <param name="extension">Type of the extension point.</param>
    public IEnumerable<object> FindExtensions(Type extension)
    {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            return container.GetExports(extension);
        }

    /// <summary>
    /// Finds all the extensions from the given base type and return their
    /// lazy type for initialization.
    /// </summary>
    /// <typeparam name="T">Type of the extension point.</typeparam>
    /// <returns>The lazy extensions.</returns>
    public IEnumerable<ExportFactory<T>> FindLazyExtensions<T>()
    {
            return container.GetExports<ExportFactory<T>>();
        }

    /// <summary>
    /// Finds all the extensions from the given base type and returns
    /// a factory to initialize the type.
    /// </summary>
    /// <param name="extension">Type of the extension point.</param>
    /// <returns>The extension factory.</returns>
    public IEnumerable<object> FindLazyExtensions(Type extension)
    {
            if (extension == null) {
                throw new ArgumentNullException(nameof(extension));
            }

            Type lazyType = typeof(ExportFactory<>).MakeGenericType(extension);
            return container.GetExports(lazyType);
        }

    /// <summary>
    /// Finds all the extensions from the given base type and returns
    /// a factory to initialize the type and its associated metadata.
    /// </summary>
    /// <typeparam name="T">Type of the extension point.</typeparam>
    /// <typeparam name="TMetadata">Type of the metadata.</typeparam>
    /// <returns>The extension factory.</returns>
    public IEnumerable<ExportFactory<T, TMetadata>> FindLazyExtensions<T, TMetadata>()
        where TMetadata : IExportMetadata
    {
            // Because of technical limitations / bugs there can be upto
            // 3 copies of the same extension. We filter by type.
            return container.GetExports<ExportFactory<T, TMetadata>>()
                .GroupBy(f => f.Metadata.Type)
                .Select(f => f.First());
        }

    /// <summary>
    /// Get a list of format extensions.
    /// </summary>
    /// <returns>Enumerable of lazy formats with metadata.</returns>
    public IEnumerable<ExportFactory<IFormat, FormatMetadata>> GetFormats()
    {
            return FindLazyExtensions<IFormat, FormatMetadata>();
        }

    /// <summary>
    /// Get a list of converter extensions.
    /// </summary>
    /// <returns>Enumerable of lazy converters with metadata.</returns>
    public IEnumerable<ExportFactory<IConverter, ConverterMetadata>> GetConverters()
    {
            return FindLazyExtensions<IConverter, ConverterMetadata>();
        }

    static void DefineFormatConventions(ConventionBuilder conventions)
    {
            conventions
                .ForTypesDerivedFrom<IFormat>()
                .Export<IFormat>(
                    export => export
                        .AddMetadata("Name", t => t.FullName)
                        .AddMetadata("Type", t => t))
                .SelectConstructor(ctors =>
                    ctors.OrderBy(ctor => ctor.GetParameters().Length)
                    .First());
        }

    static void DefineConverterConventions(ConventionBuilder conventions)
    {
            bool ConverterInterfaceFilter(Type i) =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().IsEquivalentTo(typeof(IConverter<,>));

            // We export three types each converter:
            // 1.- Export the specific generic converter types
            // 2.- Export the IConverter interfaces with the interfaces metadata
            // 3.- Export again the IConverter interface to fill common metadata
            conventions
                .ForTypesDerivedFrom(typeof(IConverter<,>))
                .ExportInterfaces(ConverterInterfaceFilter)
                .ExportInterfaces(
                    ConverterInterfaceFilter,
                    (inter, export) => export
                        .AddMetadata("InternalSources", inter.GenericTypeArguments[0])
                        .AddMetadata("InternalDestinations", inter.GenericTypeArguments[1])
                        .AsContractType<IConverter>())
                .Export<IConverter>(
                    export => export
                    .AddMetadata("Name", t => t.FullName)
                    .AddMetadata("Type", t => t))
                .SelectConstructor(ctors =>
                    ctors.OrderBy(ctor => ctor.GetParameters().Length)
                    .First());
        }

    static IEnumerable<Assembly> LoadAssemblies(IEnumerable<string> paths)
    {
            // Skip libraries that match the ignored libraries because
            // MEF would try to load its dependencies.
            return paths
                .Select(p => new { Name = Path.GetFileName(p), Path = p })
                .Where(p => !IgnoredLibraries.Any(
                    ign => p.Name.StartsWith(ign, StringComparison.OrdinalIgnoreCase)))
                .Select(p => p.Path)
                .LoadAssemblies();
        }

    static CompositionHost InitializeContainer()
    {
            var conventions = new ConventionBuilder();
            DefineFormatConventions(conventions);
            DefineConverterConventions(conventions);

            var containerConfig = new ContainerConfiguration()
                .WithDefaultConventions(conventions);

            // Assemblies from the program directory (including this one).
            var programDir = AppDomain.CurrentDomain.BaseDirectory;
            var libraryAssemblies = Directory.GetFiles(programDir, "*.dll");
            var programAssembly = Directory.GetFiles(programDir, "*.exe");
            containerConfig
                .WithAssemblies(LoadAssemblies(libraryAssemblies))
                .WithAssemblies(LoadAssemblies(programAssembly));

            // Assemblies from the Plugin directory and subfolders
            string pluginDir = Path.Combine(programDir, PluginDirectory);
            if (Directory.Exists(pluginDir)) {
                var pluginFiles = Directory.GetFiles(
                    pluginDir,
                    "*.dll",
                    SearchOption.AllDirectories);
                containerConfig.WithAssemblies(LoadAssemblies(pluginFiles));
            }

            return containerConfig.CreateContainer();
        }
}
