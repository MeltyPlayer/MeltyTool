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

    /// <summary>
    /// Metadata associated to a IConverter interface.
    /// </summary>
    public class ConverterMetadata : IExportMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterMetadata" /> class.
        /// </summary>
        public ConverterMetadata()
        {
            // MEF should always set these properties, so they won't be null.
            // We set some initial values to ensure later they are not set to null.
            Name = "<invalid>";
            Type = typeof(ConverterMetadata);
            InternalSources = Type.EmptyTypes;
            InternalDestinations = Type.EmptyTypes;
        }

        /// <summary>
        /// Gets or sets the full name of the type. Shortcut of Type.FullName.
        /// </summary>
        /// <value>The full name of the type.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of class implementing the converter.
        /// </summary>
        /// <value>Type of the converter.</value>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets a single type or list of types that the converter
        /// can convert from.
        /// </summary>
        /// <value>Single or list of types for conversion.</value>
        public object InternalSources { get; set; }

        /// <summary>
        /// Gets or sets a single type or list of types the converter can
        /// convert to.
        /// </summary>
        /// <value>Single or list of types the converter can convert to.</value>
        public object InternalDestinations { get; set; }

        /// <summary>
        /// Gets a list of source types that can convert from.
        /// </summary>
        /// <returns>List of source types that can convert from.</returns>
        public Type[] GetSources()
        {
            if (InternalSources is not Type[] sourceList) {
                sourceList = [(Type)InternalSources];
            }

            return sourceList;
        }

        /// <summary>
        /// Gets a list of destination types it can convert to.
        /// </summary>
        /// <returns>Destination types it can convert to.</returns>
        public Type[] GetDestinations()
        {
            if (InternalDestinations is not Type[] destList) {
                destList = [(Type)InternalDestinations];
            }

            return destList;
        }

        /// <summary>
        /// Check if the associated converter can convert from a given type.
        /// It checks applying covariance rules.
        /// </summary>
        /// <param name="source">Source type for conversion.</param>
        /// <returns>If this converter can realize the operation.</returns>
        public bool CanConvert(Type source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Type[] sources = GetSources();
            for (int i = 0; i < sources.Length; i++) {
                if (sources[i].IsAssignableFrom(source)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the associated converter can convert from a given type
        /// into another. It checks applying covariance and contravariance
        /// rules.
        /// </summary>
        /// <param name="source">Source type for conversion.</param>
        /// <param name="dest">Destination type for conversion.</param>
        /// <returns>If this converter can realize the operation.</returns>
        public bool CanConvert(Type source, Type dest)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (dest == null)
                throw new ArgumentNullException(nameof(dest));

            Type[] sources = GetSources();
            Type[] destinations = GetDestinations();

            for (int i = 0; i < sources.Length; i++) {
                bool matchSource = sources[i].IsAssignableFrom(source);
                bool matchDest = dest.IsAssignableFrom(destinations[i]);
                if (matchSource && matchDest) {
                    return true;
                }
            }

            return false;
        }
    }
}
