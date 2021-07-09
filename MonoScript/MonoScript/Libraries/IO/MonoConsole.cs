using MonoScript.Models.Analytics;
using MonoScript.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MonoScript.Libraries.IO
{
    public static class MonoConsole
    {
        public static string NewLine = "\r\n";

        public delegate void ClearDelegate();
        public delegate void WriteApplicationMessageDelegate(AppMessage message);
        public delegate void WriteDelegate(string text);
        public delegate string ReadLineDelegate();
        public delegate char ReadKeyDelegate();

        public static event WriteApplicationMessageDelegate WriteApplicationMessageEvent;
        public static event WriteDelegate WriteEvent;
        public static event ReadLineDelegate ReadLineEvent;
        public static event ReadKeyDelegate ReadKeyEvent;
        public static event ClearDelegate ClearEvent;
        public static bool IsBlocked { get; private set; }
        public static void LockConsole() => IsBlocked = true;
        public static void UnlockConsole() => IsBlocked = false;

        public static void WriteAppMessage(AppMessage message)
        {
            if (WriteApplicationMessageEvent == null)
            {
                Console.Write($"Message: {message.Message}, Source: {message.Source}");
                return;
            }

            WriteApplicationMessageEvent?.Invoke(message);
        }
        public static void Write(string value)
        {
            if (!IsBlocked)
            {
                if (string.IsNullOrEmpty(value))
                    value = "Null";

                if (WriteEvent == null)
                {
                    Console.Write(value);
                    return;
                }

                WriteEvent.Invoke(value);
            }
        }
        public static void Write(object value)
        {
            Write(GetTextBlocksFromArray(value));
        }
        public static void Write(int value)
        {
            Write(value.ToString());
        }
        public static void Write(bool value)
        {
            Write(value ? "True" : "False");
        }
        public static void WriteLine(string value)
        {
            Write(value + NewLine);
        }
        public static void WriteLine(object value)
        {
            Write(GetTextBlocksFromArray(value) + NewLine);
        }
        public static void WriteLine(int value)
        {
            Write(value + NewLine);
        }
        public static void WriteLine(bool value)
        {
            Write(value ? "True" + NewLine : "False" + NewLine);
        }
        public static void WriteLine()
        {
            Write(NewLine);
        }
        public static string ReadLine()
        {
            if (!IsBlocked)
            {
                if (ReadLineEvent == null)
                    return Console.ReadLine();

                return ReadLineEvent.Invoke();
            }

            return null;
        }
        public static char? ReadKey()
        {
            if (!IsBlocked)
            {
                if (ReadKeyEvent == null)
                    return Console.ReadKey().KeyChar;

                return ReadKeyEvent.Invoke();
            }

            return null;
        }
        public static void Clear()
        {
            if (!IsBlocked)
            {
                if (ClearEvent == null)
                {
                    Console.Clear();
                    return;
                }

                ClearEvent.Invoke();
            }
        }

        public static string GetTextBlocksFromArray(object value)
        {
            string text = string.Empty;

            object valueFromField = MonoInterpreter.ValueFromField(value);

            if (Extensions.HasEnumerator(valueFromField) && !(valueFromField is string))
            {
                int index = 0;
                foreach (var str in valueFromField as List<dynamic>)
                {
                    dynamic strValueFromField = MonoInterpreter.ValueFromField(str);

                    if (Extensions.HasEnumerator(strValueFromField))
                    {
                        if (index == (valueFromField as List<dynamic>).Count - 1)
                        {
                            if ((valueFromField as List<dynamic>).Count == 1)
                                text += GetTextBlocksFromArray(strValueFromField);
                            else
                                text += GetTextBlocksFromArray(strValueFromField);
                        }
                        else
                            text += GetTextBlocksFromArray(strValueFromField) + ", ";
                    }
                    else
                    {
                        if (index == (valueFromField as List<dynamic>).Count - 1)
                            text += MonoInterpreter.ValueFromField(strValueFromField) == null ? "Null" : MonoInterpreter.ValueFromField(strValueFromField).ToString();
                        else
                            text += MonoInterpreter.ValueFromField(strValueFromField) == null ? "Null, " : MonoInterpreter.ValueFromField(strValueFromField).ToString() + ", ";
                    }

                    index++;
                }

                text = string.Format("[{0}]", text);
            }
            else
                text = valueFromField == null ? "Null" : valueFromField.ToString(); ;

            return text;
        }
    }
}
