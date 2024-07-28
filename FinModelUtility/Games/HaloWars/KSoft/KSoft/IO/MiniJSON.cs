/*
 * Copyright (c) 2013 Calvin Rien
 *
 * Based on the JSON parser by Patrick van Bergen
 * http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
 *
 * Simplified it so that it doesn't throw exceptions
 * and can be used in Unity iPhone with maximum code stripping.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * Pulled from: https://gist.github.com/darktable/1411710
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KSoft; // KM00

namespace MiniJSON {
    // Example usage:
    //
    //  using UnityEngine;
    //  using System.Collections;
    //  using System.Collections.Generic;
    //  using MiniJSON;
    //
    //  public class MiniJSONTest : MonoBehaviour {
    //      void Start () {
    //          var jsonString = "{ \"array\": [1.44,2,3], " +
    //                          "\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
    //                          "\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
    //                          "\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
    //                          "\"int\": 65536, " +
    //                          "\"float\": 3.1415926, " +
    //                          "\"bool\": true, " +
    //                          "\"null\": null }";
    //
    //          var dict = Json.Deserialize(jsonString) as Dictionary<string,object>;
    //
    //          Debug.Log("deserialized: " + dict.GetType());
    //          Debug.Log("dict['array'][0]: " + ((List<object>) dict["array"])[0]);
    //          Debug.Log("dict['string']: " + (string) dict["string"]);
    //          Debug.Log("dict['float']: " + (double) dict["float"]); // floats come out as doubles
    //          Debug.Log("dict['int']: " + (long) dict["int"]); // ints come out as longs
    //          Debug.Log("dict['unicode']: " + (string) dict["unicode"]);
    //
    //          var str = Json.Serialize(dict);
    //
    //          Debug.Log("serialized: " + str);
    //      }
    //  }

    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    ///
    /// JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
    /// All numbers are parsed to doubles.
    /// </summary>
    public static class Json {
        public static string PrettyPrintSpace { get; set; } = "\t";

        private static object ConvertInt64(Type returnType, long number)
        {
            switch (Type.GetTypeCode(returnType))
            {
                case TypeCode.Char:
                    return (char)number;
                case TypeCode.SByte:
                    return (sbyte)number;
                case TypeCode.Byte:
                    return (byte)number;
                case TypeCode.Int16:
                    return (short)number;
                case TypeCode.UInt16:
                    return (ushort)number;
                case TypeCode.Int32:
                    return (int)number;
                case TypeCode.UInt32:
                    return (uint)number;
                case TypeCode.Int64:
                    return number;
                case TypeCode.UInt64:
                    return (ulong)number;
                case TypeCode.Single:
                    return (float)number;
                case TypeCode.Double:
                    return (double)number;

                default:
                    return null;
            }
        }

        private static object ConvertDouble(Type returnType, double number)
        {
            switch (Type.GetTypeCode(returnType))
            {
                case TypeCode.Char:
                    return (char)number;
                case TypeCode.SByte:
                    return (sbyte)number;
                case TypeCode.Byte:
                    return (byte)number;
                case TypeCode.Int16:
                    return (short)number;
                case TypeCode.UInt16:
                    return (ushort)number;
                case TypeCode.Int32:
                    return (int)number;
                case TypeCode.UInt32:
                    return (uint)number;
                case TypeCode.Int64:
                    return (long)number;
                case TypeCode.UInt64:
                    return (ulong)number;
                case TypeCode.Single:
                    return (float)number;
                case TypeCode.Double:
                    return number;

                default:
                    return null;
            }
        }

        private static bool IsNumericType(Type returnType)
        {
            if (returnType == null)
                return false;

            var typeCode = Type.GetTypeCode(returnType);

            return typeCode >= TypeCode.SByte && typeCode <= TypeCode.Double;
        }

        #region GetValue
        public static T GetValue<T>(object jsonObject, string[] keyPath, T defaultValue = default(T))
        {
            if (keyPath.Length == 0)
            {
                return defaultValue;
            }

            for (var i = 0; i < keyPath.Length - 1; i++)
            {
                jsonObject = GetValue<object>(jsonObject, keyPath[i]);
            }

            return GetValue<T>(jsonObject, keyPath[keyPath.Length - 1], defaultValue);
        }

        public static object GetValue(object jsonObject, string key, object defaultValue = null)
        {
            return GetValue<object>(jsonObject, key, defaultValue);
        }

        public static T GetValue<T>(object jsonObject, string key, T defaultValue = default(T))
        {
			if (!(jsonObject is Dictionary<string, object> dict))
			{
				return defaultValue;
			}

            if (!dict.TryGetValue(key, out object result))
            {
                return defaultValue;
            }

            var returnType = typeof(T);

            if (IsNumericType(returnType))
            {
                if (result is double)
                {
                    var obj = ConvertDouble(returnType, (double)result);
                    if (obj == null)
                        return defaultValue;

                    return (T)obj;
                }
                else if (result is long)
                {
                    var obj = ConvertInt64(returnType, (long)result);
                    if (obj == null)
                        return defaultValue;

                    return (T)obj;
                }

                return default(T);
            }

            if (returnType == typeof(string) && !(result is string))
            {
                var canConvertToString = IsNumericType(result.GetType());
                if (canConvertToString)
                {
                    result = result.ToString();
                }
            }

            return (T)result;
        }
        #endregion

        #region SetValue
        public static void SetValue(object jsonObject, string[] keyPath, object value)
        {
            for (int i = 0; i < keyPath.Length - 1; i++)
            {
                var next = GetValue(jsonObject, keyPath[i]);
                if (next == null)
                {
                    next = new Dictionary<string, object>();
                    SetValue(jsonObject, keyPath[i], next);
                }
                jsonObject = next;
            }

            SetValue(jsonObject, keyPath[keyPath.Length - 1], value);
        }

        public static void SetValue(object jsonObject, string key, object value)
        {
			if (!(jsonObject is Dictionary<string, object> dict))
				return;

			if (key.IndexOf('.') != -1)
            {
                SetValue(dict, key.Split('.'), value);
            }
            else
            {
                dict[key] = value;
            }
        }
        #endregion

        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
        public static object Deserialize(string json) {
            // save the string for debug information
            if (json == null) {
                return null;
            }

            return Parser.Parse(json);
        }

        sealed class Parser : IDisposable {
            const string WORD_BREAK = "{}[],:\"";

            public static bool IsWordBreak(char c) {
                return Char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
            }

            enum TOKEN {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARED_OPEN,
                SQUARED_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            };

            StringReader json;

            Parser(string jsonString) {
	            this.json = new StringReader(jsonString);
            }

            public static object Parse(string jsonString) {
                using (var instance = new Parser(jsonString)) {
                    return instance.ParseValue();
                }
            }

            public void Dispose() {
	            this.json.Dispose();
	            this.json = null;
            }

            Dictionary<string, object> ParseObject() {
                Dictionary<string, object> table = new Dictionary<string, object>();

                // ditch opening brace
                this.json.Read();

                // {
                while (true) {
                    switch (this.NextToken) {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.CURLY_CLOSE:
                        return table;
                    case TOKEN.STRING:
                        // name
                        string name = this.ParseString();
                        if (name == null) {
                            return null;
                        }

                        // :
                        if (this.NextToken != TOKEN.COLON) {
                            return null;
                        }
                        // ditch the colon
                        this.json.Read();

                        // value
                        TOKEN valueToken = this.NextToken;
                         object value = this.ParseByToken(valueToken);
                         if (value == null && valueToken != TOKEN.NULL)
                             return null;
                         table[name] = value;
                        break;
                    default:
                        return null;
                    }
                }
            }

            List<object> ParseArray() {
                // KM00: changed this method to handle invalid arrays
                // see: https://gist.github.com/darktable/1411710/16b47b4865745c4f6278e2e60d2cda53b84447d3
                // "MiniJSON causes an OutOfMemoryException..."
                // but also had to change the way 'array' is allocated

                // KM00 start
                List<object> array = null;// = new List<object>();
                // KM00 end

                // ditch opening bracket
                this.json.Read();

                // [
                var parsing = true;
                while (parsing) {
                    TOKEN nextToken = this.NextToken;

                    switch (nextToken) {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.SQUARED_CLOSE:
                        parsing = false;
                        break;
                    // KM00 start
                    case TOKEN.COLON:
	                    this.json.Read(); //invalid array: consume colon to prevent infinite loop
                        break;
                    // KM00 end
                    default:
                        object value = this.ParseByToken(nextToken);
                        if (value == null && nextToken != TOKEN.NULL)
                            return null;

                            // KM00 start
                            if (array == null)
                            array = new List<object>();
                        // KM00 end


                        array.Add(value);
                        break;
                    }
                }

                // KM00 start
                if (array == null)
                    array = new List<object>();
                // KM00 end

                return array;
            }

            object ParseValue() {
                TOKEN nextToken = this.NextToken;
                return this.ParseByToken(nextToken);
            }

            object ParseByToken(TOKEN token) {
                switch (token) {
                case TOKEN.STRING:
                    return this.ParseString();
                case TOKEN.NUMBER:
                    return this.ParseNumber();
                case TOKEN.CURLY_OPEN:
                    return this.ParseObject();
                case TOKEN.SQUARED_OPEN:
                    return this.ParseArray();
                case TOKEN.TRUE:
                    return true;
                case TOKEN.FALSE:
                    return false;
                case TOKEN.NULL:
                    return null;
                default:
                    return null;
                }
            }

            // KM00 start
            private StringBuilder mParseStringBuffer;
            private char[] mParseStringHexBuffer;
            private static bool IsHexDigit(char c)
            {
                return
                    (c >= '0' && c <= '9') ||
                    (c >= 'A' && c <= 'F') ||
                    (c >= 'a' && c <= 'f')
                    ;
            }
            // KM00 end
            string ParseString() {
                // KM00 start
                if (this.mParseStringBuffer == null)
	                this.mParseStringBuffer = new StringBuilder();
                else
	                this.mParseStringBuffer.Length = 0;

                StringBuilder s = this.mParseStringBuffer;
                // KM00 end
                char c;

                // ditch opening quote
                this.json.Read();

                bool parsing = true;
                while (parsing) {

                    if (this.json.Peek() == -1) {
                        parsing = false;
                        break;
                    }

                    c = this.NextChar;
                    switch (c) {
                    case '"':
                        parsing = false;
                        break;
                    case '\\':
                        if (this.json.Peek() == -1) {
                            parsing = false;
                            break;
                        }

                        c = this.NextChar;
                        switch (c) {
                        case '"':
                        case '\\':
                        case '/':
                            s.Append(c);
                            break;
                        case 'b':
                            s.Append('\b');
                            break;
                        case 'f':
                            s.Append('\f');
                            break;
                        case 'n':
                            s.Append('\n');
                            break;
                        case 'r':
                            s.Append('\r');
                            break;
                        case 't':
                            s.Append('\t');
                            break;
                        case 'u':
                            // KM00 start
                            if (this.mParseStringHexBuffer == null)
	                            this.mParseStringHexBuffer = new char[4];

                            var hex = this.mParseStringHexBuffer;
                            // KM00 end

                            for (int i=0; i< 4; i++) {
                                hex[i] = this.NextChar;
                                if (!IsHexDigit(hex[i]))
                                    return null;
                            }

                            s.Append((char) Convert.ToInt32(new string(hex), 16));
                            break;
                        }
                        break;
                    default:
                        s.Append(c);
                        break;
                    }
                }

                return s.ToString();
            }

            object ParseNumber() {
                string number = this.NextWord;

                // Allow scientific notation in floating point numbers by @shiwano
                // https://github.com/Jackyjjc/MiniJSON.cs/commit/6de00beb134bbab9d873033a48b32e4067ed0c25
                if (number.IndexOf('.') == -1 && number.IndexOf('E') == -1 && number.IndexOf('e') == -1) {
					// KM00 start
					if (Int64.TryParse(number, out long parsedInt))
					{
						return parsedInt;
					}
					else
					{
					}
					// KM00 end
                }

                // KM00 start
                Numbers.DoubleTryParseInvariant(number, out double parsedDouble);
                // KM00 end
                return parsedDouble;
            }

            void EatWhitespace() {
                // KM00 start
                if (this.json.Peek () == -1)
                    return;
                // KM00 end

                while (Char.IsWhiteSpace(this.PeekChar)) {
	                this.json.Read();

                    if (this.json.Peek() == -1) {
                        break;
                    }
                }
            }

            char PeekChar {
                get {
                    return Convert.ToChar(this.json.Peek());
                }
            }

            char NextChar {
                get {
                    return Convert.ToChar(this.json.Read());
                }
            }

            // KM00 start
            private StringBuilder mNextWordStringBuffer;
            // KM00 end
            string NextWord {
                get {
                    // KM00 start
                    if (this.mNextWordStringBuffer == null)
	                    this.mNextWordStringBuffer = new StringBuilder();
                    else
	                    this.mNextWordStringBuffer.Length = 0;

                    StringBuilder word = this.mNextWordStringBuffer;
                    // KM00 end

                    while (!IsWordBreak(this.PeekChar)) {
                        word.Append(this.NextChar);

                        if (this.json.Peek() == -1) {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            TOKEN NextToken {
                get {
	                this.EatWhitespace();

                    if (this.json.Peek() == -1) {
                        return TOKEN.NONE;
                    }

                    switch (this.PeekChar) {
                    case '{':
                        return TOKEN.CURLY_OPEN;
                    case '}':
	                    this.json.Read();
                        return TOKEN.CURLY_CLOSE;
                    case '[':
                        return TOKEN.SQUARED_OPEN;
                    case ']':
	                    this.json.Read();
                        return TOKEN.SQUARED_CLOSE;
                    case ',':
	                    this.json.Read();
                        return TOKEN.COMMA;
                    case '"':
                        return TOKEN.STRING;
                    case ':':
                        return TOKEN.COLON;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        return TOKEN.NUMBER;
                    }

                    switch (this.NextWord) {
                    case "false":
                        return TOKEN.FALSE;
                    case "true":
                        return TOKEN.TRUE;
                    case "null":
                        return TOKEN.NULL;
                    }

                    return TOKEN.NONE;
                }
            }
        }

        /// <summary>
        /// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
        /// </summary>
        /// <param name="json">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string Serialize(object theObj, bool prettyPrint = false, int numPrettyPrintLevels = 0) {
            return Serializer.Serialize(theObj, prettyPrint, numPrettyPrintLevels);
        }

        sealed class Serializer {
            StringBuilder builder;
            bool prettyPrint;
            int numPrettyPrintLevels;

            Serializer() {
	            this.builder = new StringBuilder();
            }

            public static string Serialize(object obj, bool prettyPrint = false, int numPrettyPrintLevels = 0) {
				var instance = new Serializer
				{
					prettyPrint = prettyPrint,
					numPrettyPrintLevels = numPrettyPrintLevels
				};
				instance.SerializeValue(obj);

                return instance.builder.ToString();
            }

            void SerializeValue(object value, int level = 0) {
                IList asList;
                IDictionary asDict;
                string asStr;

                if (value == null) {
	                this.builder.Append("null");
                } else if ((asStr = value as string) != null) {
	                this.SerializeString(asStr);
                } else if (value is bool) {
	                this.builder.Append((bool) value ? "true" : "false");
                } else if ((asList = value as IList) != null) {
	                this.SerializeArray(asList, level);
                } else if ((asDict = value as IDictionary) != null) {
	                this.SerializeObject(asDict, level);
                } else if (value is char) {
	                this.SerializeString(new string((char) value, 1));
                } else {
	                this.SerializeOther(value);
                }
            }

            void SerializeObject(IDictionary obj, int level = 0) {
                bool first = true;

                this.builder.Append('{');

                foreach (object e in obj.Keys) {
                    if (!first) {
	                    this.builder.Append(',');
                    }

                    if (this.numPrettyPrintLevels <= 0 || (level + 1) <= this.numPrettyPrintLevels)
                    {
	                    this.PrettyPrintNewLine(level + 1);
                    }

                    this.SerializeString(e.ToString());
                    this.builder.Append(':');

                    this.SerializeValue(obj[e], level + 1);

                    first = false;
                }

                if (!first && (this.numPrettyPrintLevels <= 0 || level < this.numPrettyPrintLevels))
                {
	                this.PrettyPrintNewLine(level);
                }

                this.builder.Append('}');
            }

            void SerializeArray(IList anArray, int level = 0) {
	            this.builder.Append('[');

                bool first = true;

                foreach (object obj in anArray) {
                    if (!first) {
	                    this.builder.Append(',');
                    }

                    if (this.numPrettyPrintLevels <= 0 || (level + 1) <= this.numPrettyPrintLevels)
                    {
	                    this.PrettyPrintNewLine(level + 1);
                    }

                    this.SerializeValue(obj, level + 1);

                    first = false;
                }

                if (!first && (this.numPrettyPrintLevels <= 0 || level < this.numPrettyPrintLevels))
                {
	                this.PrettyPrintNewLine(level);
                }

                this.builder.Append(']');
            }

            void SerializeString(string str) {
	            this.builder.Append('\"');

                foreach (var c in str) {
                    switch (c) {
                    case '"':
	                    this.builder.Append("\\\"");
                        break;
                    case '\\':
	                    this.builder.Append("\\\\");
                        break;
                    case '\b':
	                    this.builder.Append("\\b");
                        break;
                    case '\f':
	                    this.builder.Append("\\f");
                        break;
                    case '\n':
	                    this.builder.Append("\\n");
                        break;
                    case '\r':
	                    this.builder.Append("\\r");
                        break;
                    case '\t':
	                    this.builder.Append("\\t");
                        break;
                    default:
                        int codepoint = Convert.ToInt32(c);
                        if ((codepoint >= 32) && (codepoint <= 126)) {
	                        this.builder.Append(c);
                        } else {
	                        this.builder.Append("\\u");
	                        this.builder.Append(codepoint.ToString("x4", Util.InvariantCultureInfo)); // KM00
                        }
                        break;
                    }
                }

                this.builder.Append('\"');
            }

            void SerializeOther(object value) {
                // NOTE: decimals lose precision during serialization.
                // They always have, I'm just letting you know.
                // Previously floats and doubles lost precision too.
                if (value is float) {
                    // KM00 changed to ToStringInvariant with recommended round trip specifier
                    this.builder.Append(((float) value).ToStringInvariant(Numbers.kFloatRoundTripFormatSpecifier));
                } else if (value is int
                    || value is uint
                    || value is long
                    || value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is ulong) {
	                this.builder.Append(value);
                } else if (value is double
                    || value is decimal) {
                    // KM00 changed to ToStringInvariant with recommended round trip specifier
                    this.builder.Append(Convert.ToDouble(value, Util.InvariantCultureInfo).ToStringInvariant(Numbers.kDoubleRoundTripFormatSpecifier));
                } else {
	                this.SerializeString(value.ToString());
                }
            }

            void PrettyPrintNewLine(int numSpaces)
            {
                if (!this.prettyPrint)
                    return;

                this.builder.Append('\n');
                for (int i = 0; i < numSpaces; i++)
                {
	                this.builder.Append(PrettyPrintSpace);
                }
            }
        }
    }
}
