using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoScript.Collections
{
    public class ModifierCollection : List<string>
    {
        public static string[] AllModifiers { get; } = new string[] { "public", "protected", "private", "static", "const", "readonly" };

        public int FirstIndex { get; set; }
        public int LastIndex { get; set; }
        public string LastModifier { get; set; } = string.Empty;

        public static ModifierCollection GetModifiers(string script, int startIndex)
        {
            ModifierCollection modifiers = new ModifierCollection();
            modifiers.FirstIndex = -1;
            modifiers.LastIndex = startIndex;

            for (; startIndex > 0; startIndex--)
            {
                if (script[startIndex].Contains(ReservedCollection.Alphabet))
                    modifiers.LastModifier = modifiers.LastModifier.Insert(0, script[startIndex].ToString());

                else if (Regex.IsMatch(script[startIndex].ToString(), "\\s") || startIndex == 0)
                {
                    if (modifiers.LastModifier != string.Empty)
                    {
                        if (AllModifiers.Contains(modifiers.LastModifier))
                        {
                            modifiers.Add(modifiers.LastModifier);
                            modifiers.LastModifier = string.Empty;
                        }
                        else break;
                    }
                }
                else break;
            }

            if (modifiers.LastModifier != string.Empty && AllModifiers.Contains(modifiers.LastModifier))
            {
                modifiers.Add(modifiers.LastModifier);
                modifiers.LastModifier = string.Empty;
            }

            if (modifiers.Count > 0)
            {
                modifiers.FirstIndex = startIndex + 1;

                if (modifiers.LastModifier != string.Empty)
                    modifiers.FirstIndex += modifiers.LastModifier.Length;
            }
            return modifiers;
        }
    }
}