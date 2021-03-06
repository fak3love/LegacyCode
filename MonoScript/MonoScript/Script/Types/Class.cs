using MonoScript.Analytics;
using MonoScript.Models;
using MonoScript.Runtime;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using System.Collections.Generic;
using System.Linq;
using MonoScript.Models.Application;
using MonoScript.Models.Analytics;
using MonoScript.Models.Script;
using MonoScript.Models.Contexts;

namespace MonoScript.Script.Types
{
    public class Class : MonoType, IInherit<Class>
    {
        public Parent<Class> Parent { get; set; } = new Parent<Class>();
        public override List<string[]> AllowedModifierGroups { get; set; } = new List<string[]>()
        {
            new string[] { "public", "static", "readonly" },
            new string[] { "private", "static", "readonly" }
        };
        public Class(string fullpath, object parentObject, params string[] modifiers)
        {
            FullPath = fullpath;
            ParentObject = parentObject;

            AddModifiers(modifiers.ToList());
        }
        public Class CloneObject()
        {
            Class obj = (Class)MemberwiseClone();
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

        public void InheritParent()
        {
            if (Parent.StringValue != null && Parent.ObjectValue == null)
            {
                if (Modifiers.Contains("static"))
                    MLog.AppErrors.Add(new AppMessage("You cannot inherit a static class.", $"Path {FullPath}"));

                Class parentObj = Finder.FindObject(Parent.StringValue, new FindContext(this) { ScriptFile = ParentObject as ScriptFile }, FindOption.FindType) as Class;
                IInherit<Class>.GetErrors(this, parentObj);

                if (parentObj != null)
                {
                    Parent.ObjectValue = parentObj;
                    Parent.ObjectValue.InheritParent();

                    foreach (var obj in parentObj.Methods.Where(x => x.Modifiers.Contains("public", "protected") && !x.Modifiers.Contains("const", "static")))
                    {
                        Method method = Methods.FirstOrDefault(x => x.Name == obj.Name && x.Parameters.Count == obj.Parameters.Count);

                        if (method == null)
                        {
                            if (Methods.FirstOrDefault(x => x.Name == obj.Name) == null)
                            {
                                method = obj.CloneObject();
                                method.FullPath = IPath.CombinePath(method.Name, FullPath);

                                Methods.Add(method);
                            }
                            else MLog.AppErrors.Add(new AppMessage("A method was found with the same name as the parent class. Use modifier new.", $"Path {obj.Name}"));
                        }
                    }
                    foreach (var obj in parentObj.Fields.Where(x => x.Modifiers.Contains("public", "protected") && !x.Modifiers.Contains("const", "static")))
                    {
                        Field field = Fields.FirstOrDefault(x => x.Name == obj.Name);

                        if (field == null)
                        {
                            if (Fields.FirstOrDefault(x => x.Name == obj.Name) == null)
                            {
                                field = obj.CloneObject();
                                field.FullPath = IPath.CombinePath(field.Name, FullPath);

                                Fields.Add(field);
                            }
                            else MLog.AppErrors.Add(new AppMessage("A field was found with the same name as the parent class. Use modifier new.", $"Path {obj.Name}"));
                        }
                    }   
                }
            }
        }
        public bool ContainsParent(Class objContext)
        {
            if (objContext == null)
                return false;

            bool result = false;
            Class objClass = this;

            if (FullPath == objContext.FullPath)
                result = true;
            else
            {
                while (objClass.Parent.ObjectValue != null)
                {
                    objClass = objClass.Parent.ObjectValue;

                    if (objClass.FullPath == objContext.FullPath)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public static string CreateClassRegex { get; } = Extensions.GetPrefixRegex("class") + $"\\s+{ObjectNameRegex}\\s*";
    }
}
