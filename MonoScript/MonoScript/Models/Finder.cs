using MonoScript.Runtime;
using MonoScript.Script;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;
using MonoScript.Collections;

namespace MonoScript.Models
{
    public static class Finder
    {
        public static dynamic FindObject(string path, FindContext context, FindOption findOption = FindOption.None, int parameterCount = -1)
        {
            bool isNone = (findOption & FindOption.None) == FindOption.None;
            bool isFindEnumValue = (findOption & FindOption.FindEnumValue) == FindOption.FindEnumValue;
            bool isFindField = (findOption & FindOption.FindField) == FindOption.FindField;
            bool isFindMethod = (findOption & FindOption.FindMethod) == FindOption.FindMethod;
            bool isFindType = (findOption & FindOption.FindType) == FindOption.FindType;
            bool isNoStatic = (findOption & FindOption.NoStatic) == FindOption.NoStatic;

            if (context.LocalSpace != null)
            {
                string[] splitPaths = IPath.SplitPath(path);

                if (splitPaths.Length > 1)
                {
                    Field foundField = context.LocalSpace.Find(splitPaths[0]);

                    if (foundField != null)
                    {
                        object foundObj = foundField.Value;

                        for (int i = 1; i < splitPaths.Length; i++)
                        {
                            if (foundObj is MonoType)
                            {
                                var obj = (foundObj as MonoType).Fields.FirstOrDefault(x => x.Name == splitPaths[i]);

                                if (obj != null)
                                {
                                    bool fine = obj.Modifiers.Contains("public");

                                    if (!fine && obj.Modifiers.Contains("private"))
                                        fine = (obj.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                    if (!fine && obj.Modifiers.Contains("protected") && obj.ParentObject is Class objClass)
                                        fine = objClass.ContainsParent(context.MonoType as Class);

                                    if (fine)
                                        foundObj = obj;
                                    else
                                        return null;
                                }
                                else
                                {
                                    Method method = parameterCount == -1 ? (foundObj as MonoType).Methods.FirstOrDefault(x => x.Name == splitPaths[i]) : (foundObj as MonoType).Methods.FirstOrDefault(x => x.Name == splitPaths[i] && x.Parameters.Count == parameterCount);

                                    if (method != null && i + 1 == splitPaths.Length)
                                    {
                                        bool fine = method.Modifiers.Contains("public");

                                        if (!fine && method.Modifiers.Contains("private"))
                                            fine = (method.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                        if (!fine && method.Modifiers.Contains("protected") && method.ParentObject is Class objClass)
                                            fine = objClass.ContainsParent(context.MonoType as Class);

                                        if (fine && isNone)
                                                return method;

                                        if (isFindMethod)
                                            if (!isNoStatic || !method.Modifiers.Contains("static"))
                                                return method;

                                        return null;
                                    }
                                }

                                continue;
                            }

                            if (foundObj is EnumValue)
                            {
                                if (i + 1 != splitPaths.Length)
                                    return null;

                                if (isNone && isFindEnumValue)
                                    return foundObj;

                                return null;
                            }

                            if (foundObj is Field)
                            {
                                bool fine = (foundObj as Field).Modifiers.Contains("public");

                                if (!fine && (foundObj as Field).Modifiers.Contains("private"))
                                    fine = ((foundObj as Field).ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                if (!fine && (foundObj as Field).Modifiers.Contains("protected") && (foundObj as Field).ParentObject is Class objClass)
                                    fine = objClass.ContainsParent(context.MonoType as Class);

                                if (fine)
                                {
                                    if (i + 1 == splitPaths.Length)
                                    {
                                        if (isNone)
                                            return foundObj;

                                        if (isFindField)
                                            if (!isNoStatic || !(foundObj as Field).Modifiers.Contains("static"))
                                                return foundObj;

                                        return null;
                                    }
                                    else
                                        foundObj = (foundObj as Field).Value;
                                }
                                else
                                    return null;
                            }
                        }

                        return foundObj;
                    }
                }
                else
                {
                    var foundObj = context.LocalSpace.Find(IPath.NormalizeWithTrim(path));

                    if (foundObj != null)
                    {
                        if (isFindField)
                        {
                            if (isNone)
                                return foundObj;

                            if (isFindField)
                                if (!isNoStatic && !foundObj.Modifiers.Contains("static"))
                                    return foundObj;
                        }

                        return null;
                    }
                }
            }

            if (context.MonoType != null)
            {
                string[] splitPaths = IPath.SplitPath(IPath.NormalizeWithTrim(path));

                if (splitPaths.Length > 1)
                {
                    Field foundField = context.MonoType.Fields.FirstOrDefault(x => x.Name == splitPaths[0]);

                    if (foundField != null)
                    {
                        object foundObj = foundField.Value;

                        for (int i = 1; i < splitPaths.Length; i++)
                        {
                            if (foundObj is MonoType)
                            {
                                var obj = (foundObj as MonoType).Fields.FirstOrDefault(x => x.Name == splitPaths[i]);

                                if (obj != null)
                                {
                                    bool fine = obj.Modifiers.Contains("public");

                                    if (!fine && obj.Modifiers.Contains("private"))
                                        fine = (obj.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                    if (!fine && obj.Modifiers.Contains("protected") && obj.ParentObject is Class objClass)
                                        fine = objClass.ContainsParent(context.MonoType as Class);

                                    if (fine)
                                        foundObj = obj;
                                    else
                                        return null;
                                }
                                else
                                {
                                    Method method = parameterCount == -1 ? (foundObj as MonoType).Methods.FirstOrDefault(x => x.Name == splitPaths[i]) : (foundObj as MonoType).Methods.FirstOrDefault(x => x.Name == splitPaths[i] && x.Parameters.Count == parameterCount);

                                    if (method != null && i + 1 == splitPaths.Length)
                                    {
                                        bool fine = method.Modifiers.Contains("public");

                                        if (!fine && method.Modifiers.Contains("private"))
                                            fine = (method.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                        if (!fine && method.Modifiers.Contains("protected") && method.ParentObject is Class objClass)
                                            fine = objClass.ContainsParent(context.MonoType as Class);

                                        if (fine && isNone)
                                            return method;

                                        if (isFindMethod)
                                            if (!isNoStatic || !method.Modifiers.Contains("static"))
                                                return method;

                                        return null;
                                    }
                                }

                                continue;
                            }

                            if (foundObj is EnumValue)
                            {
                                if (i + 1 != splitPaths.Length)
                                    return null;

                                if (isNone || isFindEnumValue)
                                    return foundObj;

                                return null;
                            }

                            if (foundObj is Field)
                            {
                                bool fine = (foundObj as Field).Modifiers.Contains("public");

                                if (!fine && (foundObj as Field).Modifiers.Contains("private"))
                                    fine = ((foundObj as Field).ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                if (!fine && (foundObj as Field).Modifiers.Contains("protected") && (foundObj as Field).ParentObject is Class objClass)
                                    fine = objClass.ContainsParent(context.MonoType as Class);

                                if (fine)
                                {
                                    if (i + 1 == splitPaths.Length)
                                    {
                                        if (isNone)
                                            return foundObj;

                                        if (isFindField)
                                            if (!isNoStatic || !(foundObj as Field).Modifiers.Contains("static"))
                                                return foundObj;

                                        return null;
                                    }
                                    else
                                        foundObj = (foundObj as Field).Value;
                                }
                                else
                                    return null;
                            }
                        }

                        return foundObj;
                    }
                }
                else
                {
                    var foundMethod = parameterCount == -1 ? context.MonoType.Methods.FirstOrDefault(x => x.Name == IPath.NormalizeWithTrim(path)) : context.MonoType.Methods.FirstOrDefault(x => x.Name == IPath.NormalizeWithTrim(path) && x.Parameters.Count == parameterCount);

                    if (foundMethod != null)
                    {
                        if (isNone)
                            return foundMethod;

                        if (isFindMethod)
                            if (!isNoStatic || !foundMethod.Modifiers.Contains("static"))
                                return foundMethod;

                        return null;
                    }

                    var foundObj = context.MonoType.Fields.FirstOrDefault(x => x.Name == IPath.NormalizeWithTrim(path));

                    if (foundObj != null)
                    {
                        if (isNone)
                            return foundObj;

                        if (isFindField)
                            if (!isNoStatic || !foundObj.Modifiers.Contains("static"))
                                return foundObj;

                        return null;
                    }
                }
            }

            if (context.ScriptFile != null && context.MonoType != null)
            {
                List<MonoType> monoTypes = new List<MonoType>();
                
                if (context.MonoType.FullPath.StartsWith(context.ScriptFile.Root.Class.Path)) 
                    monoTypes.Add(context.ScriptFile.Root.Class); 

                monoTypes.AddRange(context.ScriptFile.Classes.Where(x => context.MonoType.FullPath.StartsWith(x.Path)));
                monoTypes.AddRange(context.ScriptFile.Structs.Where(x => context.MonoType.FullPath.StartsWith(x.Path)));

                foreach (var obj in monoTypes)
                {
                    string[] splitPaths = IPath.SplitPath(path);

                    int indexOf = obj.FullPath.IndexOf(splitPaths[0]);

                    if (indexOf != -1)
                    {
                        if (obj.Name == splitPaths[0])
                        {
                            if (splitPaths.Length == 1)
                            {
                                if (isNone)
                                    return obj;

                                if (isFindMethod)
                                    if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                        return parameterCount == -1 ? obj.Methods.FirstOrDefault(x => x.IsConstructor) : obj.Methods.FirstOrDefault(x => x.IsConstructor && x.Parameters.Count == parameterCount);

                                if (isFindType)
                                    if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                        return obj;

                                continue;
                            }

                            string fullpath = IPath.CombinePath(IPath.CombinePath(splitPaths.Skip(1).ToArray()), obj.FullPath);

                            dynamic LocalChecker(MonoType obj, string[] splitPaths)
                            {
                                if (splitPaths.Length == 1)
                                {
                                    if (isNone || isFindField)
                                    {
                                        var result = obj.Fields.FirstOrDefault(x => x.Name == splitPaths[0]);

                                        if (result != null)
                                        {
                                            bool fine = result.Modifiers.Contains("public");

                                            if (!fine && obj.Modifiers.Contains("private"))
                                                fine = (obj.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                            if (!fine && obj.Modifiers.Contains("protected") && obj.ParentObject is Class objClass)
                                                fine = objClass.ContainsParent(context.MonoType as Class);

                                            if (fine)
                                            {
                                                if (!isNoStatic || !result.Modifiers.Contains("static"))
                                                    return result;
                                            }

                                            return null;
                                        }

                                        if (!isNone || result != null)
                                            return result;
                                    }

                                    if (isNone || isFindMethod)
                                    {
                                        Method result = parameterCount == -1 ? obj.Methods.FirstOrDefault(x => x.Name == splitPaths[0]) : obj.Methods.FirstOrDefault(x => x.Name == splitPaths[0] && x.Parameters.Count == parameterCount);

                                        if (result != null)
                                        {
                                            bool fine = result.Modifiers.Contains("public");

                                            if (!fine && obj.Modifiers.Contains("private"))
                                                fine = (obj.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                            if (!fine && obj.Modifiers.Contains("protected") && obj.ParentObject is Class objClass)
                                                fine = objClass.ContainsParent(context.MonoType as Class);

                                            if (fine)
                                            {
                                                if (!isNoStatic || !result.Modifiers.Contains("static"))
                                                    return result;
                                            }

                                            return null;
                                        }
                                        else
                                        {
                                            obj = context.ScriptFile.Root.Class.FullPath == fullpath ? context.ScriptFile.Root.Class : null;

                                            if (obj == null)
                                                obj = context.ScriptFile.Classes.FirstOrDefault(x => x.FullPath == fullpath);

                                            if (obj == null)
                                                obj = context.ScriptFile.Structs.FirstOrDefault(x => x.FullPath == fullpath);

                                            if (obj != null)
                                            {
                                                bool fine = obj.Modifiers.Contains("public");

                                                if (!fine && obj.Modifiers.Contains("private"))
                                                    fine = IPath.ContainsWithLevelAccess(context.MonoType.FullPath, obj.FullPath);

                                                if (fine)
                                                {
                                                    if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                                    {
                                                        result = parameterCount == -1 ? obj.Methods.FirstOrDefault(x => x.Name == splitPaths[0]) : obj.Methods.FirstOrDefault(x => x.Name == splitPaths[0] && x.Parameters.Count == parameterCount);

                                                        if (result != null)
                                                        {
                                                            result.Modifiers.Contains("public");

                                                            if (!fine && obj.Modifiers.Contains("private"))
                                                                fine = (obj.ParentObject as MonoType)?.FullPath == context.MonoType?.FullPath;

                                                            if (!fine && obj.Modifiers.Contains("protected") && obj.ParentObject is Class objClass)
                                                                fine = objClass.ContainsParent(context.MonoType as Class);

                                                            if (fine)
                                                            {
                                                                if (!isNoStatic || !result.Modifiers.Contains("static"))
                                                                    return result;
                                                            }

                                                            return null;
                                                        }
                                                    }
                                                }

                                                return null;
                                            }
                                        }

                                        if (!isNone || result != null)
                                            return result;
                                    }

                                    if (isNone || isFindType)
                                    {
                                        MonoType result = context.ScriptFile.Root.Class.FullPath == fullpath ? context.ScriptFile.Root.Class : null;

                                        if (result == null)
                                            result = context.ScriptFile.Classes.FirstOrDefault(x => x.FullPath == fullpath);

                                        if (result == null)
                                            result = context.ScriptFile.Structs.FirstOrDefault(x => x.FullPath == fullpath);

                                        if (result != null)
                                        {
                                            bool fine = result.Modifiers.Contains("public");

                                            if (!fine && result.Modifiers.Contains("private"))
                                                fine = IPath.ContainsWithLevelAccess(context.MonoType.FullPath, result.FullPath);

                                            if (fine)
                                            {
                                                if (!isNoStatic || !result.Modifiers.Contains("static"))
                                                    return result;
                                            }

                                            return null;
                                        }

                                        return result;
                                    }
                                }

                                string newpath = IPath.CombinePath(IPath.CombinePath(splitPaths.Take(1).ToArray()), obj.FullPath);

                                MonoType newresult = context.ScriptFile.Root.Class.FullPath == newpath ? context.ScriptFile.Root.Class : null;

                                if (newresult == null)
                                    newresult = context.ScriptFile.Classes.FirstOrDefault(x => x.FullPath == newpath);

                                if (newresult == null)
                                    newresult = context.ScriptFile.Structs.FirstOrDefault(x => x.FullPath == newpath);

                                if (newresult != null)
                                {
                                    bool fine = newresult.Modifiers.Contains("public");

                                    if (!fine && newresult.Modifiers.Contains("private"))
                                        fine = IPath.ContainsWithLevelAccess(context.MonoType.FullPath, newresult.FullPath);

                                    if (fine)
                                    {
                                        if (!isNoStatic || !newresult.Modifiers.Contains("static"))
                                            return LocalChecker(newresult, splitPaths.Skip(1).ToArray());
                                    }
                                }

                                return null;
                            }

                            var result = LocalChecker(obj, splitPaths.Skip(1).ToArray());

                            if (result != null)
                                return result;
                        }
                    }
                }

                foreach (var obj in context.ScriptFile.Enums.Where(x => context.MonoType.FullPath.StartsWith(x.Path)))
                {
                    string[] splitPaths = IPath.SplitPath(path);

                    int indexOf = obj.FullPath.IndexOf(splitPaths[0]);

                    if (indexOf != -1)
                    {
                        if (obj.Name == splitPaths[0])
                        {
                            if (splitPaths.Length == 2)
                            {
                                if (isNone || isFindEnumValue)
                                    return obj.Values.FirstOrDefault(x => x.Name == splitPaths[1]);

                                continue;
                            }
                        }
                    }
                }

                monoTypes = new List<MonoType>();
                monoTypes.Add(context.ScriptFile.Root.Class);
                monoTypes.AddRange(context.ScriptFile.Classes);
                monoTypes.AddRange(context.ScriptFile.Structs);

                foreach (var obj in monoTypes)
                {
                    if (obj.FullPath == path)
                    {
                        if (isNone)
                            return obj;

                        if (isFindMethod)
                            if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                return parameterCount == -1 ? obj.Methods.FirstOrDefault(x => x.IsConstructor) : obj.Methods.FirstOrDefault(x => x.IsConstructor && x.Parameters.Count == parameterCount);

                        if (isFindType)
                            if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                return obj;

                        return null;
                    }

                    string[] splitPaths = IPath.SplitPath(path);

                    foreach (var objUsing in context.ScriptFile.Usings)
                    {
                        if (!string.IsNullOrWhiteSpace(objUsing.AsName))
                        {
                            if (objUsing.AsName == splitPaths[0] && IPath.CombinePath(IPath.CombinePath(splitPaths.Skip(1).ToArray()), objUsing.Path) == obj.FullPath)
                            {
                                if (isNone)
                                    return obj;

                                if (isFindMethod)
                                    if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                        return parameterCount == -1 ? obj.Methods.FirstOrDefault(x => x.IsConstructor) : obj.Methods.FirstOrDefault(x => x.IsConstructor && x.Parameters.Count == parameterCount);

                                if (isFindType)
                                    if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                        return obj;

                                return null;
                            }
                        }
                        else
                        {
                            if (IPath.CombinePath(path, objUsing.Path) == obj.FullPath)
                            {
                                if (isFindMethod)
                                    if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                        return parameterCount == -1 ? obj.Methods.FirstOrDefault(x => x.IsConstructor) : obj.Methods.FirstOrDefault(x => x.IsConstructor && x.Parameters.Count == parameterCount);

                                if (isFindType)
                                    if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                        return obj;

                                return null;
                            }
                        }
                    }
                }

                foreach (var obj in context.ScriptFile.Enums)
                {
                    if (obj.FullPath == path)
                        return obj;

                    string[] splitPaths = IPath.SplitPath(path);

                    foreach (var objUsing in context.ScriptFile.Usings)
                    {
                        if (!string.IsNullOrWhiteSpace(objUsing.AsName))
                        {
                            if (objUsing.AsName == splitPaths[0] && IPath.CombinePath(IPath.CombinePath(splitPaths.Skip(1).ToArray()), objUsing.Path) == obj.FullPath)
                                return obj;
                        }
                        else
                        {
                            if (IPath.CombinePath(path, objUsing.Path) == obj.FullPath)
                                return obj;
                        }
                    }
                }

                foreach (var import in context.ScriptFile.Imports)
                {
                    monoTypes = new List<MonoType>();
                    monoTypes.Add(import.Root.Class);
                    monoTypes.AddRange(import.Classes);
                    monoTypes.AddRange(import.Structs);

                    foreach (var obj in monoTypes)
                    {
                        if (obj.FullPath == path)
                        {
                            if (isNone)
                                return obj;

                            if (isFindMethod)
                                if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                    return parameterCount == -1 ? obj.Methods.FirstOrDefault(x => x.IsConstructor) : obj.Methods.FirstOrDefault(x => x.IsConstructor && x.Parameters.Count == parameterCount);

                            if (isFindType)
                                if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                    return obj;

                            return null;
                        }

                        string[] splitPaths = IPath.SplitPath(path);

                        foreach (var objUsing in context.ScriptFile.Usings)
                        {
                            if (!string.IsNullOrWhiteSpace(objUsing.AsName))
                            {
                                if (objUsing.AsName == splitPaths[0] && IPath.CombinePath(IPath.CombinePath(splitPaths.Skip(1).ToArray()), objUsing.Path) == obj.FullPath)
                                {
                                    if (isNone)
                                        return obj;

                                    if (isFindMethod)
                                        if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                            return parameterCount == -1 ? obj.Methods.FirstOrDefault(x => x.IsConstructor) : obj.Methods.FirstOrDefault(x => x.IsConstructor && x.Parameters.Count == parameterCount);

                                    if (isFindType)
                                        if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                            return obj;

                                    return null;
                                }
                            }
                            else
                            {
                                if (IPath.CombinePath(path, objUsing.Path) == obj.FullPath)
                                {
                                    if (isFindMethod)
                                        if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                            return parameterCount == -1 ? obj.Methods.FirstOrDefault(x => x.IsConstructor) : obj.Methods.FirstOrDefault(x => x.IsConstructor && x.Parameters.Count == parameterCount);

                                    if (isFindType)
                                        if (!isNoStatic || !obj.Modifiers.Contains("static"))
                                            return obj;

                                    return null;
                                }
                            }
                        }
                    }

                    foreach (var obj in context.ScriptFile.Enums)
                    {
                        if (obj.FullPath == path)
                            return obj;

                        string[] splitPaths = IPath.SplitPath(path);

                        foreach (var objUsing in context.ScriptFile.Usings)
                        {
                            if (!string.IsNullOrWhiteSpace(objUsing.AsName))
                            {
                                if (objUsing.AsName == splitPaths[0] && IPath.CombinePath(IPath.CombinePath(splitPaths.Skip(1).ToArray()), objUsing.Path) == obj.FullPath)
                                    return obj;
                            }
                            else
                            {
                                if (IPath.CombinePath(path, objUsing.Path) == obj.FullPath)
                                    return obj;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }

    [Flags]
    public enum FindOption
    {
        None = 1,
        FindMethod = 2,
        FindType = 4,
        FindField = 8,
        FindEnumValue = 16,
        NoStatic = 32,
    }
}