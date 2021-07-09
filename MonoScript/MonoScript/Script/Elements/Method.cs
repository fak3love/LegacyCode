using MonoScript.Analytics;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Exts;
using MonoScript.Models.Script;
using MonoScript.Script.Basic;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Types;
using System.Collections.Generic;
using System.Linq;

namespace MonoScript.Script.Elements
{
    public class Method : MonoObject, IModifier, IObjectParent
    {
        public object ParentObject { get; }
        public string Content { get; set; }
        public bool IsConstructor
        {
            get
            {
                if (ParentObject != null && (ParentObject as MonoType)?.Name == Name)
                    return true;

                return false;
            }
        }
        public List<Field> Parameters { get; set; } = new List<Field>();
        public List<string> Modifiers { get; set; } = new List<string>();
        public List<string[]> AllowedModifierGroups { get; set; } = new List<string[]>()
        {
            new string[] { "public", "static" },
            new string[] { "private", "static" },
            new string[] { "protected", "static" },
        };
        public Method(string fullpath, object parentObject, params string[] modifiers)
        {
            FullPath = fullpath;
            ParentObject = parentObject;
            AddModifiers(modifiers.ToList());
        }
        public Method CloneObject()
        {
            Method method = (Method)MemberwiseClone();
            method.Parameters = new List<Field>();

            for (int index = 0; index < Parameters.Count; index++)
                method.Parameters.Add(Parameters[index].CloneObject());

            return method;
        }

        public LocalSpace GetLocalSpace(LocalSpace parentSpace)
        {
            LocalSpace localSpace = new LocalSpace(parentSpace);

            foreach (var field in Parameters)
                localSpace.Add(field);

            return localSpace;
        }
        public void UpdateParametersFromInputs(List<(string Name, dynamic Value)> inputs)
        {
            if (inputs.Count < GetCountRequiredField())
            {
                MLog.AppErrors.Add(new AppMessage("The number of method input parameters is less than the number of required parameters.", $"Method {FullPath}"));
                return;
            }

            if (inputs.Count > 0)
            {
                if (!string.IsNullOrEmpty(inputs[0].Name))
                {
                    if (inputs.Count != Parameters.Count)
                    {
                        MLog.AppErrors.Add(new AppMessage("You need to explicitly specify all named input parameters of the method.", $"Method {FullPath}"));
                        return;
                    }

                    for (int index = 0; index < inputs.Count; index++)
                    {
                        int inputCount = inputs.Count(x => x.Name == inputs[index].Name);

                        if (inputCount > 1)
                        {
                            MLog.AppErrors.Add(new AppMessage("Method input named parameters must not be repeated.", $"Method {FullPath}"));
                            return;
                        }

                        Field foundField = Parameters.FirstOrDefault(x => x.Name == inputs[index].Name);

                        if (foundField == null)
                        {
                            MLog.AppErrors.Add(new AppMessage("Input named method parameter not found.", $"Method {FullPath}"));
                            return;
                        }

                        foundField.Value = inputs[index].Value;
                    }
                }
                else
                {
                    if (inputs.Count > Parameters.Count)
                    {
                        MLog.AppErrors.Add(new AppMessage("More input parameters than the method have.", $"Method {FullPath}"));
                        return;
                    }

                    for (int index = 0; index < inputs.Count; index++)
                    {
                        Parameters[index].Value = inputs[index].Value;
                    }

                    for (int index = inputs.Count; index < Parameters.Count; index++)
                    {
                        if (Parameters[index].IsRequiredField)
                        {
                            MLog.AppErrors.Add(new AppMessage("Missing required input method parameter.", $"Methpd {FullPath}"));
                            return;
                        }
                    }
                }
            }
        }

        public int GetCountRequiredField()
        {
            int requiredCount = 0;

            foreach (var field in Parameters)
            {
                if (field.IsRequiredField)
                    requiredCount++;
            }

            return requiredCount;
        }
        public void AddModifiers(List<string> modifiers)
        {
            Modifiers.AddRange(modifiers);

            bool hasError = false;

            foreach (var group in AllowedModifierGroups)
            {
                int count = 0;
                foreach (var modifier in modifiers)
                {
                    if (!hasError && !group.Contains(modifier))
                        hasError = true;

                    if (group.Contains(modifier))
                        count++;
                }

                if (!hasError || count == modifiers.Count)
                    return;
            }

            if (hasError)
                MLog.AppErrors.Add(new AppMessage("Invalid modifier group.", $"Path {FullPath}"));
        }

        public static List<Field> GetParameters(string exfields, string methodPath, object parentObject)
        {
            if (exfields == null)
                return new List<Field>();

            List<Field> parameters = new List<Field>();
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            string name = null, value = null;

            for (int i = 0; i < exfields.Length; i++)
            {
                Extensions.IsOpenQuote(exfields, i, ref quoteModel);

                if (value == null && !exfields[i].Contains("=,"))
                    name += exfields[i];

                if (value != null && (quoteModel.HasQuotes || (!quoteModel.HasQuotes && !exfields[i].Contains("=,"))))
                    value += exfields[i];

                if (!quoteModel.HasQuotes)
                {
                    if (exfields[i] == '=')
                        value = "";

                    if (exfields[i] == ',')
                    {
                        if (value == "")
                            MLog.AppErrors.Add(new AppMessage("The field does not have a value.", $"Method: {methodPath}"));

                        parameters.Add(new Field(IPath.CombinePath(name.Trim(' '), methodPath), parentObject) { Value = value });

                        name = null;
                        value = null;
                    }
                }
            }

            if (name != null)
            {
                if (value == "")
                    MLog.AppErrors.Add(new AppMessage("The field does not have a value.", $"Method: {methodPath}"));

                parameters.Add(new Field(IPath.CombinePath(name.Trim(' '), methodPath), parentObject) { Value = value });
            }

            return parameters;
        }
        public static string CreateMethodRegex { get; } = Extensions.GetPrefixRegex("def") + $"\\s+{ObjectNameRegex}\\s*\\(";
    }
}
