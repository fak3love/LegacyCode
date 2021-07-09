using MonoScript.Models;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using System.Collections.Generic;
using System.Linq;
using MonoScript.Models.Application;

namespace MonoScript.Script.Types
{
    public class Struct : MonoType
    {
        public override List<string[]> AllowedModifierGroups { get; set; } = new List<string[]>()
        {
            new string[] { "public", "readonly" },
            new string[] { "private", "readonly" },
        };
        public Struct(string fullpath, object parentObject, params string[] modifiers)
        {
            FullPath = fullpath;
            ParentObject = parentObject;
            AddModifiers(modifiers.ToList());
        }
        public Struct CloneObject()
        {
            Struct obj = (Struct)MemberwiseClone();
            obj.Methods = new List<Method>();
            obj.Fields = new List<Field>();
            obj.OverloadOperators = new List<Method>();
            obj.IsOriginal = false;

            for (int index = 0; index < Methods.Count; index++)
                obj.Methods.Add(Methods[index].CloneObject());

            for (int index = 0; index < Fields.Count; index++)
                obj.Fields.Add(Fields[index].CloneObject());

            for (int index = 0; index < OverloadOperators.Count; index++)
                obj.OverloadOperators.Add(OverloadOperators[index].CloneObject());

            return obj;
        }

        public static string CreateStructRegex { get; } = Extensions.GetPrefixRegex("struct") + $"\\s+{ObjectNameRegex}\\s*";
    }
}
