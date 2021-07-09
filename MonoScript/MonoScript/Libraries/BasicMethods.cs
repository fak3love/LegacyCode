using MonoScript.Libraries.IO;
using MonoScript.Runtime;
using MonoScript.Script.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MonoScript.Libraries
{
    public static class BasicMethods
    {
        public static dynamic InvokeMethod(string methodName, bool useHowExtension, params object[] objs)
        {
            if (methodName == "InvokeMethod")
                return null;

            foreach(var method in typeof(BasicMethods).GetMethods())
            {
                var attributeInfo = method.GetCustomAttributes(typeof(AllowedNameAttribute), false).FirstOrDefault() as AllowedNameAttribute;

                if (attributeInfo != null)
                {
                    bool fine = false;
                    for (int index = 0; index < attributeInfo.Names.Length; index++)
                    {
                        if (attributeInfo.Names[index] == methodName)
                            fine = true;
                    }

                    try
                    {
                        if (fine && method.GetParameters().Length == objs.Length)
                        {
                            if (useHowExtension && !attributeInfo.UseHowExtension)
                                break;

                            return method.Invoke(null, objs);
                        }
                    }
                    catch { break; }
                }
            }

            return null;
        }

        [AllowedName("ToString", "toString")]
        public static dynamic ToString(object obj)
        {
            return obj?.ToString();
        }
        [AllowedName("ToNumber", "toNumber")]
        public static dynamic ToNumber(object obj)
        {
            double result;

            if (double.TryParse(obj?.ToString(), out result))
                return result;

            return null;
        }
        [AllowedName("ToBoolean", "toBoolean", "ToBool", "toBool")]
        public static dynamic ToBoolean(object obj)
        {
            bool result;

            if (bool.TryParse(obj?.ToString(), out result))
                return result;

            return null;
        }
        [AllowedName("ToLower", "toLower")]
        public static dynamic ToLower(object obj)
        {
            return obj?.ToString().ToLower();
        }
        [AllowedName("ToUpper", "toUpper")]
        public static dynamic ToUpper(object obj)
        {
            return obj?.ToString().ToUpper();
        }
        [AllowedName("Length", "length", "len")]
        public static dynamic Length(dynamic obj)
        {
            if (Extensions.HasEnumerator(obj))
            {
                try
                {
                    return obj.Length;
                }
                catch { }

                try
                {
                    return obj.Count;
                }
                catch { }
            }

            return null;
        }
        [AllowedName("Print", "print")]
        public static dynamic ConsoleWrite(dynamic obj)
        {
            MonoConsole.Write(obj);

            return null;
        }
        [AllowedName("PrintLine", "println", UseHowExtension = false)]
        public static dynamic ConsoleWriteLine(dynamic obj)
        {
            MonoConsole.WriteLine(obj);

            return null;
        }
        [AllowedName("ReadLine", "readln", UseHowExtension = false)]
        public static dynamic ConsoleReadLine()
        {
            return MonoConsole.ReadLine();
        }
        [AllowedName("ReadKey", "read", UseHowExtension = false)]
        public static dynamic ConsoleReadKey()
        {
            return MonoConsole.ReadKey()?.ToString();
        }
        [AllowedName("Clear", "clear", UseHowExtension = false)]
        public static dynamic ConsoleClear()
        {
            MonoConsole.Clear();

            return null;
        }
        [AllowedName("Rand", "rand", UseHowExtension = false)]
        public static dynamic Random(dynamic minValue, dynamic maxValue)
        {
            return new Random().Next((int)minValue, (int)maxValue);
        }
        [AllowedName("Rand", "rand", UseHowExtension = false)]
        public static dynamic Random(dynamic maxValue)
        {
            return new Random().Next((int)maxValue);
        }
        [AllowedName("Rand", "rand", UseHowExtension = false)]
        public static dynamic Random()
        {
            return new Random().Next();
        }
        [AllowedName("FileWrite", "fw", UseHowExtension = false)]
        public static dynamic FileWrite(dynamic path, dynamic text)
        {
            File.WriteAllText(path, text);

            return null;
        }
        [AllowedName("FileRead", "fr", UseHowExtension = false)]
        public static dynamic FileRead(dynamic path)
        {
            return File.ReadAllText(path);
        }
        [AllowedName("FileWriteBytes", "fwb", UseHowExtension = false)]
        public static dynamic FileWriteBytes(dynamic path, dynamic bytes)
        {
            var result = (bytes as List<dynamic>).Select(x => MonoInterpreter.ValueFromField(x));

            File.WriteAllBytes(path, Array.ConvertAll(result.ToArray(), new Converter<dynamic, byte>(x => { return (byte)x; })));

            return null;
        }
        [AllowedName("FileReadBytes", "frb", UseHowExtension = false)]
        public static dynamic FileReadBytes(dynamic path)
        {
            return Array.ConvertAll<byte, dynamic>(File.ReadAllBytes(path), new Converter<byte, dynamic>(x => { return (double)x; }));
        }
        [AllowedName("Value", "value", UseHowExtension = false)]
        public static dynamic Value(dynamic obj)
        {
            if (obj is Field field)
                return field.Value;

            if (obj is EnumValue value)
                return value.Value;

            return null;
        }
    }
}
