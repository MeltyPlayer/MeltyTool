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
namespace Yarhl.FileFormat
{
    using System;
    using System.Linq;

    /// <summary>
    /// Convert formats with converters.
    /// </summary>
    public static class ConvertFormat
    {
        /// <summary>
        /// Converts the format to the specified type.
        /// </summary>
        /// <returns>The new format.</returns>
        /// <param name="src">Format to convert.</param>
        /// <typeparam name="TDst">The destination format type.</typeparam>
        public static TDst To<TDst>(dynamic src)
        {
            return (TDst)To(typeof(TDst), src);
        }

        /// <summary>
        /// Converts the format into the specified type.
        /// </summary>
        /// <returns>The new format.</returns>
        /// <param name="dstType">Type of the destination format.</param>
        /// <param name="src">Format to convert.</param>
        public static object To(Type dstType, dynamic src)
        {
            if (dstType == null)
                throw new ArgumentNullException(nameof(dstType));

            if (src == null)
                throw new ArgumentNullException(nameof(src));

            // Search the converter for the giving types.
            Type srcType = src.GetType();
            var extensions = PluginManager.Instance.GetConverters()
                .Where(e => e.Metadata.CanConvert(srcType, dstType));

            // Same as Single operation but with nice errors
            if (!extensions.Any()) {
                throw new InvalidOperationException(
                    $"Cannot find converter for: {srcType} -> {dstType}");
            } else if (extensions.Skip(1).Any()) {
                throw new InvalidOperationException(
                    $"Multiple converters for: {srcType} -> {dstType}");
            }

            dynamic converter = extensions.First().CreateExport().Value;
            return converter.Convert(src);
        }

        /// <summary>
        /// Converts the format using a converter with the specified type.
        /// </summary>
        /// <param name="converterType">Type of the converter.</param>
        /// <param name="src">Format to convert.</param>
        /// <returns>The new format.</returns>
        public static object With(Type converterType, dynamic src)
        {
            if (converterType == null)
                throw new ArgumentNullException(nameof(converterType));

            var extensions = PluginManager.Instance.GetConverters()
                .Where(x => x.Metadata.Type == converterType);

            // Same as Single operation but with nice errors
            if (!extensions.Any()) {
                throw new InvalidOperationException(
                    $"Cannot find converter {converterType}");
            }

            dynamic converter = extensions.First().CreateExport().Value;
            return converter.Convert(src);
        }

        /// <summary>
        /// Converts the format using a converter with the specified type.
        /// </summary>
        /// <param name="src">Format to convert.</param>
        /// <typeparam name="TConv">Type of the converter.</typeparam>
        /// <returns>The new format.</returns>
        public static object With<TConv>(dynamic src)
            where TConv : IConverter, new()
        {
            var converter = new TConv();
            return With(converter, src);
        }

        /// <summary>
        /// Converts the format using a converter with the specified type
        /// and initialized with some parameters.
        /// </summary>
        /// <param name="param">Parameters to initialize the converter.</param>
        /// <param name="src">Format to convert.</param>
        /// <typeparam name="TConv">Type of the converter.</typeparam>
        /// <typeparam name="TParam">Type of the parameters.</typeparam>
        /// <returns>The new format.</returns>
        public static object With<TConv, TParam>(TParam param, dynamic src)
            where TConv : IConverter, IInitializer<TParam>, new()
        {
            var converter = new TConv();
            converter.Initialize(param);
            return With(converter, src);
        }

        /// <summary>
        /// Converts the format using the specified converter.
        /// </summary>
        /// <returns>The new format.</returns>
        /// <param name="converter">Converter to use.</param>
        /// <param name="src">Format to convert.</param>
        public static object With(IConverter converter, dynamic src)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            if (src == null)
                throw new ArgumentNullException(nameof(src));

            Type[] converterInterfaces = converter.GetType().GetInterfaces();
            bool implementConverter = converterInterfaces.Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IConverter<,>));

            if (!implementConverter) {
                throw new InvalidOperationException(
                    "Converter doesn't implement IConverter<,>");
            }

            bool canConvert = converterInterfaces.Any(i =>
                i.IsGenericType &&
                i.GenericTypeArguments.Length == 2 &&
                i.GenericTypeArguments[0].IsAssignableFrom(src.GetType()));

            if (!canConvert) {
                throw new InvalidOperationException(
                    "Converter cannot convert from/to the type");
            }

            return ((dynamic)converter).Convert(src);
        }
    }
}
