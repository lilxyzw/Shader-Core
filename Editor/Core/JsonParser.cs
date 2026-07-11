using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace jp.lilxyzw.shadercore
{
    internal class JsonParser
    {
        private readonly string json;
        private int index = 0;

        public static T Deserialize<T>(string json) where T : new() => (T)ConvertToType(new JsonParser(json).ParseValue(), typeof(T));

        private static object ConvertToType(object value, Type targetType)
        {
            if (value == null)
                return null;

            // string
            if (targetType == typeof(string))
                return value.ToString();

            // enum
            if (targetType.IsEnum)
                return Enum.Parse(targetType, value.ToString());

            // primitive
            if (targetType.IsPrimitive || targetType == typeof(decimal))
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

            // List<T>
            if (targetType.IsGenericType &&
                targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = targetType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(targetType);
                foreach (var item in (List<object>)value)
                    list.Add(ConvertToType(item, elementType));
                return list;
            }

            // Array
            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType();
                var src = (List<object>)value;
                var arr = Array.CreateInstance(elementType, src.Count);
                for (int i = 0; i < src.Count; i++)
                    arr.SetValue(ConvertToType(src[i], elementType), i);
                return arr;
            }

            // Object
            var dict = value as Dictionary<string, object>;
            var obj = Activator.CreateInstance(targetType);
            foreach (FieldInfo field in targetType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (dict.TryGetValue(field.Name, out object fieldValue))
                    field.SetValue(obj, ConvertToType(fieldValue, field.FieldType));

            return obj;
        }

        public JsonParser(string json) => this.json = json;

        public object ParseValue()
        {
            SkipWhitespace();
            if (index >= json.Length) throw new Exception("Unexpected end of JSON");

            char c = json[index];
            switch (c)
            {
                case '{': return ParseObject();
                case '[': return ParseArray();
                case '"': return ParseString();
                case 't':
                    Expect("true");
                    return true;
                case 'f':
                    Expect("false");
                    return false;
                case 'n':
                    Expect("null");
                    return null;
                default:
                    if (char.IsDigit(c) || c == '-') return ParseNumber();
                    throw new Exception($"Unexpected character: {c}");
            }
        }

        private Dictionary<string, object> ParseObject()
        {
            var obj = new Dictionary<string, object>();
            index++; // {
            while (true)
            {
                SkipWhitespace();

                if (json[index] == '}')
                {
                    index++;
                    break;
                }

                var key = ParseString();

                SkipWhitespace();

                if (json[index] != ':') throw new Exception("Expected ':'");

                index++;

                obj[key] = ParseValue();

                SkipWhitespace();

                if (json[index] == ',')
                {
                    index++;
                    continue;
                }

                if (json[index] == '}')
                {
                    index++;
                    break;
                }

                throw new Exception("Expected ',' or '}'");
            }

            return obj;
        }

        private List<object> ParseArray()
        {
            var list = new List<object>();

            index++; // [

            while (true)
            {
                SkipWhitespace();

                if (json[index] == ']')
                {
                    index++;
                    break;
                }

                list.Add(ParseValue());

                SkipWhitespace();

                if (json[index] == ',')
                {
                    index++;
                    continue;
                }

                if (json[index] == ']')
                {
                    index++;
                    break;
                }

                throw new Exception("Expected ',' or ']'");
            }

            return list;
        }

        private string ParseString()
        {
            var sb = new StringBuilder();
            index++; // "
            while (true)
            {
                if (index >= json.Length) throw new Exception("Unexpected end of string");
                char c = json[index++];
                if (c == '"') break;
                if (c == '\\')
                {
                    if (index >= json.Length) throw new Exception("Invalid escape");
                    char esc = json[index++];
                    switch (esc)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            string hex = json.Substring(index, 4);
                            sb.Append((char)Convert.ToInt32(hex, 16));
                            index += 4;
                            break;

                        default: throw new Exception($"Invalid escape: \\{esc}");
                    }
                }
                else sb.Append(c);
            }

            return sb.ToString();
        }

        private object ParseNumber()
        {
            int start = index;
            bool isFloat = false;

            if (json[index] == '-') index++;

            while (index < json.Length && char.IsDigit(json[index])) index++;

            if (index < json.Length && json[index] == '.')
            {
                isFloat = true;
                index++;
                while (index < json.Length && char.IsDigit(json[index])) index++;
            }

            if (index < json.Length && (json[index] == 'e' || json[index] == 'E'))
            {
                isFloat = true;
                index++;
                if (json[index] == '+' || json[index] == '-') index++;
                while (index < json.Length && char.IsDigit(json[index])) index++;
            }

            string num = json[start..index];
            if (isFloat) return double.Parse(num, CultureInfo.InvariantCulture);
            return long.Parse(num, CultureInfo.InvariantCulture);
        }

        private void SkipWhitespace()
        {
            while (index < json.Length && char.IsWhiteSpace(json[index])) index++;
        }

        private void Expect(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (json[index + i] != s[i]) throw new Exception($"Expected '{s}'");
            index += s.Length;
        }
    }
}
