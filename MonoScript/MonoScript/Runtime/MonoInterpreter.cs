using MonoScript.Analytics;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Application;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Linq;
using MonoScript.Collections;
using MonoScript.Models.Interpreter;
using MonoScript.Models.Script;
using MonoScript.Models.Contexts;
using System.Collections.Generic;
using MonoScript.Models.Exts;
using MonoScript.Libraries.IO;

namespace MonoScript.Runtime
{
    public class MonoInterpreter
    {
        Application app;
        public MonoInterpreter(Application app) => this.app = app;

        public Status RunApplication()
        {
            Status buildStatus = Build();

            if (buildStatus.Success)
                return Run();

            return buildStatus;
        }
        public Status Build()
        {
            MonoConsole.LockConsole();
            Parser.ParseScriptFile(app);
            Application.InitializePublicModifiers(app);
            Application.InitializeConstructors(app);
            Application.InitializeInheritance(app);
            Application.InitializeFields(app);
            Analyzer.AnalyzeAll(app);
            MonoConsole.UnlockConsole();

            if (MLog.AppErrors.Count > 0)
                return Status.ErrorBuild;

            return Status.SuccessBuild;
        }
        public Status Run()
        {
            FindContext findContext = new FindContext(app.MainScript.Root.Method)
            {
                LocalSpace = new LocalSpace(null),
                MonoType = app.MainScript.Root.Class,
                ScriptFile = app.MainScript
            };

            dynamic executeResult = ObjectFromBlockResult(ExecuteScript(app.MainScript.Root.Method.Content, app.MainScript.Root.Method, findContext, ExecuteScriptContextCollection.Method));

            if (MLog.AppErrors.Count > 0)
                return Status.ErrorCompleted;

            return Status.SuccessfullyCompleted(executeResult);
        }

        public static ExecuteBlockResult ExecuteScript(string script, Method method, FindContext context, ExecuteScriptContextCollection executeScriptContext)
        {
            if (script == null)
                script = string.Empty;

            bool hasOperator = true;
            string leftex = string.Empty, scriptex = string.Empty, ignoreChars = "\n\r\t;";
            LocalSpace saveLocalSpace = context.LocalSpace;

            void Refresh()
            {
                hasOperator = true;
                scriptex = string.Empty;
                leftex = string.Empty;
            }

            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (int i = 0; i < script.Length; i++)
            {
                Extensions.IsOpenQuote(script, i, ref quoteModel);

                if (quoteModel.HasQuotes || !script[i].Contains(ignoreChars))
                    scriptex += script[i];

                if (!quoteModel.HasQuotes && hasOperator)
                {
                    if (script[i].Contains(ReservedCollection.Alphabet))
                    {
                        leftex += script[i];

                        if ("const".StartsWith(leftex) || "var".StartsWith(leftex))
                        {
                            if ("const" == leftex)
                            {
                                i++;
                                FirstIndex first = Extensions.FindFirstIndex(script, i, "v", ReservedCollection.WhiteSpace);

                                if (first.IsFirst)
                                {
                                    if (first.Position + 2 < script.Length && script[first.Position + 1] == 'a' && script[first.Position + 2] == 'r')
                                    {
                                        i = first.Position + 3;
                                        ObjectExpressions.ExecuteVarExpression(ref i, script, method, context, quoteModel, "const");

                                        i--;
                                        Refresh();
                                    }
                                    else
                                        MLog.AppErrors.Add(new AppMessage("The modifier must be followed by the var keyword.", scriptex));
                                }
                                else
                                    MLog.AppErrors.Add(new AppMessage("The modifier must be followed by the var keyword.", scriptex));
                            }

                            if ("var" == leftex)
                            {
                                i++;
                                ObjectExpressions.ExecuteVarExpression(ref i, script, method, context, quoteModel);

                                i--;
                                Refresh();
                            }
                            
                            continue;
                        }

                        else if ("for".StartsWith(leftex))
                        {
                            if ("for" == leftex)
                            {
                                i++;
                                saveLocalSpace = context.LocalSpace;
                                context.LocalSpace = new LocalSpace(context.LocalSpace);

                                var executeResult = ObjectExpressions.ExecuteForExpression(ref i, script, method, context, quoteModel);
                                context.LocalSpace = saveLocalSpace;

                                if (executeResult.CanExecuteNextResult(executeScriptContext))
                                    return executeResult.ExecuteNextResult(executeScriptContext);

                                Refresh();
                            }

                            continue;
                        }

                        else if ("while".StartsWith(leftex))
                        {
                            if ("while" == leftex)
                            {
                                i++;
                                var executeResult = ObjectExpressions.ExecuteWhileExpression(ref i, script, method, context, quoteModel);

                                if (executeResult.CanExecuteNextResult(executeScriptContext))
                                    return executeResult.ExecuteNextResult(executeScriptContext);

                                Refresh();
                            }

                            continue;
                        }

                        else if ("if".StartsWith(leftex))
                        {
                            if ("if" == leftex)
                            {
                                i++;
                                var executeResult = ObjectExpressions.ExecuteIfExpression(ref i, script, method, context, quoteModel);

                                if (executeResult.CanExecuteNextResult(executeScriptContext))
                                    return executeResult.ExecuteNextResult(executeScriptContext);

                                Refresh();
                            }

                            continue;
                        }

                        else if ("return".StartsWith(leftex))
                        {
                            if ("return" == leftex)
                            {
                                i++;
                                return ObjectExpressions.ExecuteReturnExpression(ref i, script, context).ExecuteNextResult(executeScriptContext);
                            }

                            continue;
                        }

                        else if ("continue".StartsWith(leftex))
                        {
                            if ("continue" == leftex)
                            {
                                ExecuteBlockResult executeBlock = new ExecuteBlockResult();
                                executeBlock.ResultType = ExecuteResultCollection.Continue;

                                if (executeBlock.CanExecuteNextResult(executeScriptContext))
                                    return executeBlock.ExecuteNextResult(executeScriptContext);

                                Refresh();
                            }

                            continue;
                        }

                        else if ("break".StartsWith(leftex))
                        {
                            if ("break" == leftex)
                            {
                                i++;
                                uint executeResult = ObjectExpressions.ExecuteExitExpression(ref i, script);

                                ExecuteBlockResult executeBlock = new ExecuteBlockResult();
                                executeBlock.Count = executeResult;
                                executeBlock.ResultType = ExecuteResultCollection.Break;

                                if (executeBlock.CanExecuteNextResult(executeScriptContext))
                                    return executeBlock.ExecuteNextResult(executeScriptContext);

                                Refresh();
                            }

                            continue;
                        }

                        else if ("quit".StartsWith(leftex))
                        {
                            if ("quit" == leftex)
                            {
                                i++;
                                uint executeResult = ObjectExpressions.ExecuteExitExpression(ref i, script);

                                ExecuteBlockResult executeBlock = new ExecuteBlockResult();
                                executeBlock.Count = executeResult;
                                executeBlock.ResultType = ExecuteResultCollection.Quit;

                                if (executeBlock.CanExecuteNextResult(executeScriptContext))
                                    return executeBlock.ExecuteNextResult(executeScriptContext);

                                Refresh();
                            }

                            continue;
                        }

                        else
                            hasOperator = false;
                    }
                }

                if ((!quoteModel.HasQuotes && script[i].Contains("\n;")) || i + 1 == script.Length)
                {
                    if (quoteModel.HasQuotes && i + 1 == script.Length)
                        MLog.AppErrors.Add(new AppMessage("The string was not closed.", script));

                    if (!hasOperator)
                    {
                        object newObj = ExecuteExpression(scriptex, context);
                    }

                    Refresh();
                }

                if (quoteModel.HasQuotes && hasOperator)
                    hasOperator = false;
            }

            return new ExecuteBlockResult() { ResultType = ExecuteResultCollection.None, Count = 0, ObjectResult = null };
        }
        public static dynamic ExecuteFieldExpression(string expression, Field destObj, FindContext context, ExecuteContextCollection executeContext)
        {
            if (expression == null)
                expression = string.Empty;

            if (destObj.Modifiers.Contains("const"))
            {
                if (executeContext == ExecuteContextCollection.Const)
                    return ExecuteExpression(expression, context);

                MLog.AppErrors.Add(new AppMessage("Operations with a constant are allowed only during the declaration.", expression));
            }
            else if (destObj.Modifiers.Contains("readonly"))
            {
                if (executeContext == ExecuteContextCollection.Readonly)
                    return ExecuteExpression(expression, context);

                MLog.AppErrors.Add(new AppMessage("Readonly operations are permitted only when declared or in the constructor body.", expression));
            }
            else
                 return ExecuteExpression(expression, context);

            return null;
        }
        public static dynamic ExecuteExpression(string expression, FindContext context, bool returnField = false)
        {
            int index = 0;

            if (returnField)
            {
                dynamic executeResult = ExecuteConditionalExpression(expression, ref index, context);
                return SetObjectValue(executeResult, StringFromShell(ValueFromField(ObjectFromBlockResult(executeResult))));
            }

            return StringFromShell(ValueFromField(ExecuteConditionalExpression(expression, ref index, context)));
        }
        public static dynamic ExecuteConditionalExpression(string expression, ref int index, FindContext context)
        {
            if (expression == null)
                expression = string.Empty;

            dynamic lastObj = null;
            string tmpex = string.Empty;
            bool? lastBool = null;

            MethodExpressionModel methodExpression = new MethodExpressionModel();
            SquareBracketExpressionModel bracketExpression = new SquareBracketExpressionModel();
            InsideQuoteModel insideQuote = new InsideQuoteModel();

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref insideQuote);

                if (insideQuote.HasQuotes || methodExpression.HasOpenBracket || bracketExpression.HasOpenBracket || !expression[index].Contains("()&|"))
                    tmpex += expression[index];

                methodExpression.Read(expression, index);

                if (!insideQuote.HasQuotes)
                {
                    if (!methodExpression.HasOpenBracket && !bracketExpression.HasOpenBracket)
                    {
                        if (expression[index] == '[')
                        {
                            bracketExpression.OpenBracketCount++;
                            continue;
                        }

                        if (expression[index] == '(')
                        {
                            if (methodExpression.MethodName != null)
                            {
                                methodExpression.OpenBracketCount++;
                                tmpex += expression[index];
                                continue;
                            }

                            index++;
                            var executeResult = ExecuteConditionalExpression(expression, ref index, context);
                            var valueFromField = ValueFromField(executeResult);

                            if (valueFromField is bool)
                            {
                                lastObj = executeResult;
                                lastBool = valueFromField;
                                tmpex += valueFromField.ToString().ToLower();
                            }
                            else
                            {
                                lastBool = null;
                                lastObj = executeResult;

                                if (!string.IsNullOrWhiteSpace(tmpex))
                                {
                                    tmpex += valueFromField?.ToString().ToLower();
                                    index--;
                                    continue;
                                }
                                else
                                    tmpex = valueFromField?.ToString().ToLower();
                            }

                            if (index < expression.Length && expression[index] == ')')
                            {
                                index++;
                                break;
                            }

                            index--;
                            continue;
                        } //!!!

                        if (expression[index] == ')')
                        {
                            index++;
                            break;
                        }

                        if (expression[index] == '&')
                        {
                            if (index + 1 >= expression.Length || expression[index + 1] != '&')
                                MLog.AppErrors.Add(new AppMessage("Unknown operator. &", expression));

                            index += 2;
                            tmpex = string.Empty;
                            ExecuteLogicalResult logicalResult = ObjectExpressions.ExecuteLogicalAndExpression(ref index, expression, ref tmpex, ref lastBool, ref lastObj, insideQuote, context);

                            if (!logicalResult.IsNone)
                            {
                                if (logicalResult.IsBreak)
                                    break;

                                if (logicalResult.IsContinue)
                                    continue;

                                if (logicalResult.IsReturn && logicalResult.ReturnValue is bool && logicalResult.ReturnValue)
                                    return logicalResult.ReturnValue;
                            }
                        }

                        if (expression[index] == '|')
                        {
                            if (index + 1 >= expression.Length || expression[index + 1] != '|')
                                MLog.AppErrors.Add(new AppMessage("Unknown operator. |", expression));

                            index += 2;
                            ExecuteLogicalResult logicalResult = ObjectExpressions.ExecuteLogicalOrExpression(ref index, expression, ref tmpex, ref lastBool, ref lastObj, insideQuote, context);
                            tmpex = string.Empty;

                            if (!logicalResult.IsNone)
                            {
                                if (logicalResult.IsBreak)
                                    break;

                                if (logicalResult.IsContinue)
                                    continue;

                                if (logicalResult.IsReturn)
                                    return logicalResult.ReturnValue;
                            }
                        }

                        if (expression[index] == '!' && index + 1 < expression.Length && expression[index + 1] != '=')
                        {
                            tmpex = string.Empty;
                            ObjectExpressions.ExecuteReverseBooleanExpression(ref index, expression, ref lastBool, ref lastObj, insideQuote, context);

                            continue;
                        } //!!!
                    }

                    if (index < expression.Length)
                    {
                        if (methodExpression.HasOpenBracket)
                        {
                            if (expression[index] == '(')
                                methodExpression.OpenBracketCount++;

                            if (expression[index] == ')')
                            {
                                methodExpression.OpenBracketCount--;
                                continue;
                            }
                        }

                        if (bracketExpression.HasOpenBracket)
                        {
                            if (expression[index] == '[')
                                bracketExpression.OpenBracketCount++;

                            if (expression[index] == ']')
                            {
                                bracketExpression.OpenBracketCount--;
                                continue;
                            }
                        }
                    }
                }
            }

            if (methodExpression.OpenBracketCount > 0)
                MLog.AppErrors.Add(new AppMessage("Missing closing bracket. ')'", expression));

            if (!string.IsNullOrWhiteSpace(tmpex))
                return ExecuteEqualityExpression(tmpex, context);
            else
                return lastObj;
        }
        public static dynamic ExecuteEqualityExpression(string expression, FindContext context)
        {
            if (expression == null)
                expression = string.Empty;

            dynamic result = null;
            MethodExpressionModel methodExpression = new MethodExpressionModel();
            SquareBracketExpressionModel bracketExpression = new SquareBracketExpressionModel();
            InsideQuoteModel insideQuote = new InsideQuoteModel();

            for (int i = 0; i < expression.Length; i++)
            {
                Extensions.IsOpenQuote(expression, i, ref insideQuote);

                methodExpression.Read(expression, i);

                if (!insideQuote.HasQuotes)
                {
                    if (!methodExpression.HasOpenBracket && !bracketExpression.HasOpenBracket)
                    {
                        if (expression[i] == '[')
                        {
                            bracketExpression.OpenBracketCount++;
                            continue;
                        }

                        if (expression[i] == '(')
                        {
                            if (methodExpression.MethodName != null)
                            {
                                methodExpression.OpenBracketCount++;
                                continue;
                            }
                        }

                        if (expression[i] == '<')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '=')
                            {
                                dynamic leftObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(0, i), context)));
                                dynamic rightObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(i + 2), context)));

                                result = ObjectExpressions.ExecuteEqualityObjectExpression(leftObj, rightObj, "<=", expression, context);
                                break;
                            }
                            else
                            {
                                dynamic leftObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(0, i), context)));
                                dynamic rightObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(i + 1), context)));

                                result = ObjectExpressions.ExecuteEqualityObjectExpression(leftObj, rightObj, "<", expression, context);
                                break;
                            }
                        }

                        if (expression[i] == '>')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '=')
                            {
                                dynamic leftObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(0, i), context)));
                                dynamic rightObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(i + 2), context)));

                                result = ObjectExpressions.ExecuteEqualityObjectExpression(leftObj, rightObj, ">=", expression, context);
                                break;
                            }
                            else
                            {
                                dynamic leftObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(0, i), context)));
                                dynamic rightObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(i + 1), context)));

                                result = ObjectExpressions.ExecuteEqualityObjectExpression(leftObj, rightObj, ">", expression, context);
                                break;
                            }
                        }

                        if (expression[i] == '=')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '=')
                            {
                                dynamic leftObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(0, i), context)));
                                dynamic rightObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(i + 2), context)));

                                result = ObjectExpressions.ExecuteEqualityObjectExpression(leftObj, rightObj, "==", expression, context);
                                break;
                            }
                            else
                            {
                                dynamic leftObj = ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(0, i), context));
                                dynamic rightObj = ValueFromField(ObjectFromBlockResult(ExecuteExpression(expression.Substring(i + 1), context, true)));

                                if (leftObj is Field field)
                                {
                                    if (field.Modifiers.Contains("readonly"))
                                    {
                                        if (context.ObjectContext is Method method)
                                        {
                                            if (field.Path != method.Path || !method.IsConstructor)
                                                MLog.AppErrors.Add(new AppMessage("You can change the values ​​of a variable with the readonly modifier only in the constructor.", expression));
                                        }
                                        else
                                            MLog.AppErrors.Add(new AppMessage("You can change the values ​​of a variable with the readonly modifier only in the constructor.", expression));
                                    }
                                    
                                    if (field.Modifiers.Contains("const"))
                                        MLog.AppErrors.Add(new AppMessage("It is possible to change the values ​​of a variable with the const modifier only when declaring.", expression));

                                    SetObjectValue(leftObj, rightObj);
                                    result = leftObj;
                                }
                                else
                                    MLog.AppErrors.Add(new AppMessage("The assignment object is not a field.", expression));

                                break;
                            }
                        }

                        if (expression[i] == '!')
                        {
                            if (i + 1 < expression.Length && expression[i + 1] == '=')
                            {
                                dynamic leftObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(0, i), context)));
                                dynamic rightObj = ValueFromField(ObjectFromBlockResult(ExecuteArithmeticExpression(expression.Substring(i + 2), context)));

                                result = ObjectExpressions.ExecuteEqualityObjectExpression(leftObj, rightObj, "!=", expression, context);
                                break;
                            }
                        }

                        if (expression[i].Contains(ReservedCollection.NumberOperations) && i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            i++;
                            continue;
                        }
                    }

                    if (i < expression.Length)
                    {
                        if (methodExpression.HasOpenBracket)
                        {
                            if (expression[i] == '(')
                                methodExpression.OpenBracketCount++;

                            if (expression[i] == ')')
                                methodExpression.OpenBracketCount--;
                        }

                        if (bracketExpression.HasOpenBracket)
                        {
                            if (expression[i] == '[')
                                bracketExpression.OpenBracketCount++;

                            if (expression[i] == ']')
                                bracketExpression.OpenBracketCount--;
                        }
                    }
                }
            }

            if (result == null)
                result = ExecuteArithmeticExpression(expression, context);

            return result;
        }
        public static dynamic ExecuteArithmeticExpression(string expression, FindContext context)
        {
            if (Extensions.Contains(expression, ReservedCollection.NumberOperations))
            {
                bool hasEquality = false;
                ObjectExpressions.ExecuteArithmeticObjectExpression(ref expression, context, "/%", ref hasEquality);
                ObjectExpressions.ExecuteArithmeticObjectExpression(ref expression, context, "*", ref hasEquality);
                ObjectExpressions.ExecuteArithmeticObjectExpression(ref expression, context, "-", ref hasEquality);
                ObjectExpressions.ExecuteArithmeticObjectExpression(ref expression, context, "+", ref hasEquality);
            }

            return ExecuteArgumentExpression(expression, context);
        }
        public static dynamic ExecuteArgumentExpression(string expression, FindContext context)
        {
            if (expression == null)
                expression = string.Empty;

            dynamic lastObj = null;
            string objectName = string.Empty;
            bool canMakeArray = true;
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (int i = 0; i < expression.Length; i++)
            {
                Extensions.IsOpenQuote(expression, i, ref quoteModel);

                if (canMakeArray && !expression[i].Contains(" ["))
                    canMakeArray = false;

                if (!expression[i].Contains("[(+-."))
                    objectName += expression[i];

                if (quoteModel.HasQuotes)
                {
                    if (lastObj == null)
                    {
                        lastObj = string.Empty;
                        objectName = string.Empty;

                        lastObj += ObjectExpressions.ExecuteStringExpression(ref i, expression, quoteModel);
                        continue;
                    }
                }

                if (!quoteModel.HasQuotes)
                {
                    if (lastObj == null && expression[i] != ' ' &&  (i == 0 || (i - 1 >= 0 && !expression[i - 1].Contains(ReservedCollection.AllowedNames))))
                    {
                        if (expression[i].Contains(ReservedCollection.Numbers + "-") || (expression[i] == '.' && i + 1 < expression.Length && expression[i + 1].Contains(ReservedCollection.Numbers)))
                        {
                            int saveIndex = i;
                            var result = ObjectExpressions.ExecuteNumberExpression(ref i, expression, quoteModel);

                            if (result is double)
                            {
                                lastObj = result;
                                objectName = string.Empty;

                                if (expression[i].Contains(ReservedCollection.Numbers))
                                    continue;
                            }
                            else
                                i = saveIndex;
                        }

                        if (i + 3 < expression.Length && expression[i] == 't' && expression[i + 1] == 'r' || (expression[i] == 'f' && expression[i + 1] == 'a'))
                        {
                            int saveIndex = i;
                            var result = ObjectExpressions.ExecuteBooleanExpression(ref i, expression);

                            if (result is bool)
                            {
                                if (i - 1 >= 0 && expression[i - 1] == '!')
                                    result = !result;

                                lastObj = result;
                                objectName = string.Empty;
                                continue;
                            }
                            else
                                i = saveIndex;
                        }

                        if (i + 3 < expression.Length && expression[i] == 'n' && expression[i + 1] == 'u' && expression[i + 2] == 'l')
                        {
                            int saveIndex = i;
                            var result = ObjectExpressions.ExecuteNullExpression(ref i, expression);

                            if (result is null)
                            {
                                lastObj = result;
                                objectName = string.Empty;
                                continue;
                            }
                            else
                                i = saveIndex;
                        }

                        if (i + 3 < expression.Length && expression[i] == 't' && expression[i + 1] == 'h' && expression[i + 2] == 'i')
                        {
                            int saveIndex = i;
                            var result = ObjectExpressions.ExecuteThisExpression(ref i, expression, context);

                            if (result is MonoType)
                            {
                                lastObj = result;
                                objectName = string.Empty;
                                continue;
                            }
                            else if (result is null)
                                i = saveIndex;
                        }
                    }

                    if (canMakeArray && expression[i] == '[')
                    {
                        canMakeArray = false;
                        lastObj = ObjectExpressions.ExecuteArrayExpression(ref i, expression, context);
                        continue;
                    }

                    if (!canMakeArray)
                    {
                        dynamic FindInLastObjectField(string objName, dynamic destObj)
                        {
                            destObj = ValueFromField(destObj);

                            if (destObj is MonoType objType)
                            {
                                Field foundField = objType.Fields.FirstOrDefault(x => x.Name == objName);

                                if (foundField != null)
                                {
                                    bool? allowedAccess = foundField.Modifiers.Contains("public");

                                    if (!allowedAccess.Value && foundField.Modifiers.Contains("private"))
                                        allowedAccess = objType.FullPath == context.MonoType?.FullPath;

                                    if (!allowedAccess.Value && foundField.Modifiers.Contains("protected"))
                                        allowedAccess ??= (objType as Class)?.ContainsParent(context.MonoType as Class);

                                    if (allowedAccess.Value)
                                    {
                                        if (objType.Modifiers.Contains("static"))
                                        {
                                            if (foundField.Modifiers.Contains("static"))
                                                return foundField;
                                        }
                                        else if (!foundField.Modifiers.Contains("static") || objType.IsOriginal)
                                            return foundField;
                                    }

                                    MLog.AppErrors.Add(new AppMessage("Object is below access level or static.", $"Object {objName}"));
                                }
                                else
                                {
                                    dynamic result = Finder.FindObject(IPath.CombinePath(objName, objType.FullPath), context, FindOption.FindType);

                                    if (result != null)
                                        return result;

                                    MLog.AppErrors.Add(new AppMessage("An object with this name was not found in the class or structure.", $"Object {objName}"));
                                }
                            }

                            if (destObj is MonoEnum objEnum)
                                return objEnum.Values.FirstOrDefault(x => x.Name == objName);

                            return null;
                        }
                        dynamic FindInLastObjectMethod(string methodName, dynamic destObj)
                        {
                            destObj = ValueFromField(destObj);

                            if (destObj is MonoType objType)
                            {
                                Method foundMethod = objType.Methods.FirstOrDefault(x => x.Name == methodName);

                                if (foundMethod != null)
                                {
                                    bool? allowedAccess = foundMethod.Modifiers.Contains("public");

                                    if (!allowedAccess.Value && foundMethod.Modifiers.Contains("private"))
                                        allowedAccess = objType.FullPath == context.MonoType?.FullPath;

                                    if (!allowedAccess.Value && foundMethod.Modifiers.Contains("protected"))
                                        allowedAccess ??= (objType as Class)?.ContainsParent(context.MonoType as Class);

                                    if (allowedAccess.Value)
                                    {
                                        if (objType.Modifiers.Contains("static"))
                                        {
                                            if (foundMethod.Modifiers.Contains("static"))
                                                return foundMethod;
                                        }
                                        else if (!foundMethod.Modifiers.Contains("static") || objType.IsOriginal)
                                            return foundMethod;
                                    }

                                    MLog.AppErrors.Add(new AppMessage("Object is below access level or static.", $"Method {methodName}"));
                                }
                            }

                            return lastObj;
                        }

                        if (i + 1 == expression.Length && !string.IsNullOrWhiteSpace(objectName))
                        {
                            objectName = IPath.NormalizeWithTrim(objectName);

                            if (lastObj != null)
                            {
                                var resultField = FindInLastObjectField(objectName, lastObj);

                                if (resultField != null)
                                    lastObj = resultField;
                                else
                                    MLog.AppErrors.Add(new AppMessage("The object does not contain such a field.", $"Field {objectName}"));
                            }
                            else
                                lastObj = Finder.FindObject(objectName, context, FindOption.FindType | FindOption.FindField | FindOption.FindEnumValue);

                            if (lastObj == null)
                                MLog.AppErrors.Add(new AppMessage("Field does not exist.", expression));

                            break;
                        }

                        if (expression[i] == '[')
                        {
                            objectName = IPath.NormalizeWithTrim(objectName);

                            if (lastObj != null && !string.IsNullOrWhiteSpace(objectName))
                                lastObj = FindInLastObjectField(ValueFromField(objectName), lastObj);

                            lastObj = StringInShell(ObjectFromBlockResult(ObjectExpressions.ExecuteOperatorGetElementExpression(ref i, expression, objectName, StringFromShell(ValueFromField(ObjectFromBlockResult(lastObj))), context)));

                            objectName = string.Empty;
                            continue;
                        }

                        if (expression[i] == '(')
                        {
                            objectName = IPath.NormalizeWithTrim(objectName);

                            if (lastObj != null && !string.IsNullOrWhiteSpace(objectName))
                                lastObj = FindInLastObjectMethod(objectName, ValueFromField(lastObj));

                            lastObj = StringInShell(ObjectFromBlockResult(ObjectExpressions.ExecuteMethodExpression(ref i, expression, objectName, StringFromShell(ValueFromField(ObjectFromBlockResult(lastObj))), context)));

                            objectName = string.Empty;
                            continue;
                        }

                        if (expression[i] == '.')
                        {
                            objectName = IPath.NormalizeWithTrim(objectName);

                            if (i + 1 < expression.Length && expression[i + 1] == '.')
                                MLog.AppErrors.Add(new AppMessage("Incorrect dot declaration.", expression));

                            if (!string.IsNullOrWhiteSpace(objectName))
                            {
                                if (lastObj == null)
                                {
                                    lastObj = Finder.FindObject(objectName, context, FindOption.FindType | FindOption.FindField | FindOption.FindEnumValue);

                                    if (lastObj != null)
                                        objectName = string.Empty;
                                }
                                else
                                {
                                    lastObj = FindInLastObjectField(ValueFromField(objectName), lastObj);
                                    objectName = string.Empty;
                                }
                            }

                            if (lastObj == null)
                                objectName += ".";

                            continue;
                        }

                        if (expression[i] == '+')
                        {
                            objectName = IPath.NormalizeWithTrim(objectName);

                            if (i + 1 < expression.Length && expression[i + 1] == '+')
                            {
                                lastObj = ObjectExpressions.ExecuteIncrementExpression(ref i, expression, objectName, lastObj, context);
                                i++;
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("Incorrect increment declaration.", expression));

                            objectName = string.Empty;
                            continue;
                        }

                        if (expression[i] == '-')
                        {
                            objectName = IPath.NormalizeWithTrim(objectName);

                            if (i + 1 < expression.Length && expression[i + 1] == '-')
                            {
                                lastObj = ObjectExpressions.ExecuteDecrementExpression(ref i, expression, objectName, lastObj, context);
                                i++;
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("Incorrect decrement declaration.", expression));

                            objectName = string.Empty;
                            continue;
                        }
                    }
                }
            }

            return SetObjectValue(lastObj, StringInShell(ValueFromField(ObjectFromBlockResult(lastObj))));
        }

        public static dynamic ObjectFromBlockResult(dynamic obj)
        {
            if (obj is ExecuteBlockResult blockResult)
                return blockResult.ObjectResult;

            return obj;
        }
        public static dynamic SetObjectValue(dynamic obj, dynamic value)
        {
            if (obj is Field field)
            {
                if (value is Struct structValue)
                    value = structValue.CloneObject();

                field.Value = value;
                return obj;
            }

            return obj = value;
        }
        public static dynamic ValueFromField(dynamic obj)
        {
            if (obj is Field field)
                return field.Value;

            return obj;
        }
        public static dynamic StringFromShell(dynamic value)
        {
            if (value is string valueString)
            {
                if (valueString.Length >= 2 && valueString[0].Contains(ReservedCollection.Quotes) && valueString[0] == valueString[valueString.Length - 1])
                    return valueString.Remove(0, 1).Remove(valueString.Length - 2);
            }

            return value;
        }
        public static dynamic StringInShell(dynamic value)
        {
            if (value is string stringValue && (stringValue.Length < 1 || !stringValue[0].Contains(ReservedCollection.Quotes)))
                return $"\"{stringValue}\"";

            return value;
        }
    }
}
