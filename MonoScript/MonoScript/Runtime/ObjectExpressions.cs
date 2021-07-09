using MonoScript.Analytics;
using MonoScript.Collections;
using MonoScript.Libraries;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;
using MonoScript.Models.Exts;
using MonoScript.Models.Interpreter;
using MonoScript.Models.Script;
using MonoScript.Script.Elements;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoScript.Runtime
{
    public static class ObjectExpressions
    {
        public static ExecuteLogicalResult ExecuteReverseBooleanExpression(ref int index, string expression, ref bool? lastBool, ref dynamic lastObj, InsideQuoteModel insideQuote, FindContext context)
        {
            index++;
            FirstIndex firstIndex = expression.FindFirstIndex(index, "(", ReservedCollection.WhiteSpace);
            index = firstIndex.Position;

            if (firstIndex.IsFirst)
            {
                index++;
                ReverseBool(MonoInterpreter.ExecuteConditionalExpression(expression, ref index, context), ref lastBool, ref lastObj);

                if (index < expression.Length && index - 1 >= 0 && expression[index - 1] == ')')
                    index--;

                return ExecuteLogicalResult.ReturnResult(lastBool);
            }
            else
            {
                if (firstIndex.FirstChar == '!')
                {
                    ExecuteLogicalResult logicalResult = ExecuteReverseBooleanExpression(ref index, expression, ref lastBool, ref lastObj, insideQuote, context);

                    if (logicalResult.IsReturn)
                        return ReverseBool(logicalResult.ReturnValue, ref lastBool, ref lastObj);
                    else
                        return logicalResult;
                }
                else
                {
                    bool hasQuote = false;
                    string tmpex = null;
                    MethodExpressionModel methodExpression = new MethodExpressionModel();
                    SquareBracketExpressionModel bracketExpression = new SquareBracketExpressionModel();

                    for (; index < expression.Length; index++)
                    {
                        Extensions.IsOpenQuote(expression, index, ref insideQuote);

                        if (hasQuote && !insideQuote.HasQuotes)
                        {
                            hasQuote = false;
                            tmpex += expression[index];
                            continue;
                        }

                        if (!hasQuote && insideQuote.HasQuotes)
                            hasQuote = true;

                        if (insideQuote.HasQuotes)
                            tmpex += expression[index];
                        else if (expression[index].Contains(ReservedCollection.AllowedNames + "([." + ReservedCollection.WhiteSpace))
                            tmpex += expression[index];
                        else if ((methodExpression.HasOpenBracket || bracketExpression.HasOpenBracket) && expression[index].Contains(")]"))
                            tmpex += expression[index];
                        else
                        {
                            index--;
                            break;
                        }

                        methodExpression.Read(expression, index);

                        if (!insideQuote.HasQuotes)
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
                                    continue;
                                }

                                continue;
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

                    if (!string.IsNullOrWhiteSpace(tmpex))
                        return ReverseBool(MonoInterpreter.ExecuteArgumentExpression(tmpex, context), ref lastBool, ref lastObj);

                    return ExecuteLogicalResult.ReturnResult(null);
                }
            }

            ExecuteLogicalResult ReverseBool(dynamic executeResult, ref bool? lastBool, ref dynamic lastObj)
            {
                var valueFromField = MonoInterpreter.ValueFromField(executeResult);

                if (valueFromField is bool)
                {
                    lastBool = !valueFromField;
                    lastObj = executeResult;

                    MonoInterpreter.SetObjectValue(lastObj, lastBool);
                }
                else
                    MLog.AppErrors.Add(new AppMessage("Incorrect use of the value conversion operator.", expression));
                

                return ExecuteLogicalResult.ReturnResult(executeResult == null ? executeResult : lastBool);
            }
        }
        public static ExecuteLogicalResult ExecuteLogicalOrExpression(ref int index, string expression, ref string tmpex, ref bool? lastBool, ref dynamic lastObj, InsideQuoteModel insideQuote, FindContext context)
        {
            if (!lastBool.HasValue)
            {
                var executeResult = MonoInterpreter.ExecuteEqualityExpression(tmpex, context);
                var valueFromField = MonoInterpreter.ValueFromField(executeResult);

                if (valueFromField is bool)
                {
                    lastBool = valueFromField;
                    lastObj = executeResult;
                    tmpex = string.Empty;
                }
                else
                {
                    MLog.AppErrors.Add(new AppMessage("Only logical entities can participate in a conditional statement.", tmpex));
                    return ExecuteLogicalResult.ReturnResult(null);
                }
            }

            if (!lastBool.Value)
            {
                lastBool = null;
                lastObj = null;
                tmpex = string.Empty;
                return ExecuteLogicalResult.ContinueResult;
            }
            else
            {
                char? bracketChar = null;
                int bracketRoundCount = 0;
                int bracketSquareCount = 0;

                for (; index < expression.Length; index++)
                {
                    Extensions.IsOpenQuote(expression, index, ref insideQuote);

                    if (!insideQuote.HasQuotes)
                    {
                        if (bracketChar == null && expression[index].Contains("(["))
                        {
                            if (expression[index] == '(')
                                bracketRoundCount++;
                            if (expression[index] == '[')
                                bracketSquareCount++;

                            bracketChar = expression[index];
                            continue;
                        }

                        if (bracketChar == '[' && expression[index] == '[')
                            bracketSquareCount++;

                        if (bracketChar == '[' && expression[index] == ']')
                            bracketSquareCount--;

                        if (bracketChar == '(' && expression[index] == '(')
                            bracketRoundCount++;

                        if (bracketChar == '(' && expression[index] == ')')
                        {
                            bracketRoundCount--;

                            if (bracketRoundCount == -1)
                                break;
                        }

                        if (bracketChar == null && expression[index] == ')')
                            break;
                    }
                }

                if (index >= expression.Length || expression[index] == ')')
                {
                    index++;
                    return ExecuteLogicalResult.ReturnResult(true);
                }
            }

            return ExecuteLogicalResult.NoneResult;
        }
        public static ExecuteLogicalResult ExecuteLogicalAndExpression(ref int index, string expression, ref string tmpex, ref bool? lastBool, ref dynamic lastObj, InsideQuoteModel insideQuote, FindContext context)
        {
            if (!lastBool.HasValue)
            {
                var executeResult = MonoInterpreter.ExecuteEqualityExpression(tmpex, context);
                var valueFromField = MonoInterpreter.ValueFromField(executeResult);

                if (valueFromField is bool)
                {
                    lastBool = valueFromField;
                    lastObj = executeResult;
                    tmpex = string.Empty;
                }
                else
                {
                    MLog.AppErrors.Add(new AppMessage("Only logical entities can participate in a conditional statement.", tmpex));
                    return ExecuteLogicalResult.ReturnResult(null);
                }
            }

            if (lastBool.Value)
            {
                return ExecuteLogicalResult.ContinueResult;
            }
            else
            {
                char? bracketChar = null;
                int bracketRoundCount = 0;
                int bracketSquareCount = 0;

                for (; index < expression.Length; index++)
                {
                    Extensions.IsOpenQuote(expression, index, ref insideQuote);

                    if (!insideQuote.HasQuotes)
                    {
                        if (bracketChar == null && expression[index].Contains("(["))
                        {
                            if (expression[index] == '(')
                                bracketRoundCount++;
                            if (expression[index] == '[')
                                bracketSquareCount++;

                            bracketChar = expression[index];
                            continue;
                        }

                        if (expression[index] == '|' && bracketRoundCount == 0 && bracketSquareCount == 0)
                        {
                            if (index < expression.Length && expression[index + 1] == '|')
                            {
                                index++;
                                return ExecuteLogicalResult.ContinueResult;
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("Incorrect operator declaration or. |", tmpex));
                        }

                        if (bracketChar == '[' && expression[index] == '[')
                            bracketSquareCount++;

                        if (bracketChar == '[' && expression[index] == ']')
                            bracketSquareCount--;

                        if (bracketChar == '(' && expression[index] == '(')
                            bracketRoundCount++;

                        if (bracketChar == '(' && expression[index] == ')')
                        {
                            bracketRoundCount--;

                            if (bracketRoundCount == -1)
                                break;
                        }

                        if (bracketChar == null && expression[index] == ')')
                            break;
                    }
                }

                if (index >= expression.Length || expression[index] == ')')
                {
                    index++;
                    return ExecuteLogicalResult.ReturnResult(false);
                }
            }

            return ExecuteLogicalResult.NoneResult;
        }
        public static dynamic ExecuteEqualityObjectExpression(dynamic leftObj, dynamic rightObj, string equalitySign, string errorSource, FindContext context)
        {
            MonoType leftType = leftObj as MonoType;
            MonoType rightType = rightObj as MonoType;

            if (leftType != null && rightType != null)
            {
                if (leftType.Path != rightType.Path)
                {
                    MLog.AppErrors.Add(new AppMessage("Cannot perform comparison operations with different types.", errorSource));
                    return null;
                }

                Operator oper = OperatorCollection.GetOperatorBySign(equalitySign);
                Method overloadMethod = leftType.OverloadOperators.FirstOrDefault(sdef => sdef.Name == oper.Name && sdef.Parameters.Count == oper.Parameters);

                if (overloadMethod != null)
                {
                    LocalSpace localSpace = new LocalSpace(null);
                    localSpace.Fields.Add(new Field(overloadMethod.Parameters[0].Name, null) { Value = leftObj });
                    localSpace.Fields.Add(new Field(overloadMethod.Parameters[1].Name, null) { Value = rightObj });

                    FindContext overloadMethodFindContext = new FindContext(overloadMethod);
                    overloadMethodFindContext.LocalSpace = localSpace;
                    overloadMethodFindContext.MonoType = overloadMethod.ParentObject as MonoType;
                    overloadMethodFindContext.ScriptFile = overloadMethodFindContext?.MonoType.ParentObject as ScriptFile;

                    if (!context.IsStaticObject)
                        return MonoInterpreter.ExecuteScript(overloadMethod.Content, overloadMethod, overloadMethodFindContext, ExecuteScriptContextCollection.Method);

                    MLog.AppErrors.Add(new AppMessage("Operators cannot be overridden in static classes.", errorSource));
                }
                else
                    MLog.AppErrors.Add(new AppMessage("No special comparison function found for this type. NotEqual.", string.Format("Path: {0}", leftType.Path)));

                return null;
            }

            if (leftObj is EnumValue && rightObj is EnumValue)
            {
                var leftEnumValue = leftObj as EnumValue;
                var rightEnumValue = rightObj as EnumValue;

                if (equalitySign == "==" && leftEnumValue.EnumPath == rightEnumValue.EnumPath)
                    return leftEnumValue.Value == rightEnumValue.Value;

                if (equalitySign == "!=" && leftEnumValue.EnumPath == rightEnumValue.EnumPath)
                    return leftEnumValue.Value == rightEnumValue.Value;

                MLog.AppErrors.Add(new AppMessage("Cannot perform comparison operations with different types.", errorSource));

                return null;
            }

            try
            {
                if (equalitySign == ">")
                    return leftObj > rightObj;

                if (equalitySign == ">=")
                    return leftObj >= rightObj;

                if (equalitySign == "<")
                    return leftObj < rightObj;

                if (equalitySign == "<=")
                    return leftObj <= rightObj;

                if (equalitySign == "==")
                    return leftObj == rightObj;

                if (equalitySign == "!=")
                    return leftObj != rightObj;
            }
            catch (Exception) { MLog.AppErrors.Add(new AppMessage($"Incorrect data comparison. Operator: {equalitySign}", errorSource)); }

            return null;
        }
        public static void ExecuteArithmeticObjectExpression(ref string expression, FindContext context, string arithSign, ref bool hasEquality)
        {
            if (expression == null)
                expression = string.Empty;

            string ariths = ReservedCollection.NumberOperations, leftex = null;
            int leng = expression.Length, leftexStartIndex = -1;
            MethodExpressionModel methodExpression = new MethodExpressionModel();
            SquareBracketExpressionModel bracketExpression = new SquareBracketExpressionModel();
            InsideQuoteModel insideQuote = new InsideQuoteModel();

            for (int i = 0; i < leng; i++)
            {
                Extensions.IsOpenQuote(expression, i, ref insideQuote);

                methodExpression.Read(expression, i);

                if (!insideQuote.HasQuotes)
                {
                    if (!methodExpression.HasOpenBracket && !bracketExpression.HasOpenBracket)
                    {
                        string rightex = null;

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

                        if (expression[i].Contains(arithSign))
                        {
                            if (string.IsNullOrWhiteSpace(leftex) && expression[i] == '-')
                                continue;

                            if (expression[i].Contains("+-"))
                            {
                                if (i + 1 < expression.Length && expression[i] == expression[i + 1])
                                {
                                    i++;
                                    continue;
                                }
                                else if (i == 0)
                                    MLog.AppErrors.Add(new AppMessage("Arithmetic operation error.", expression));
                            }

                            if (i + 1 < expression.Length && expression[i + 1] == '=')
                                hasEquality = true;
                            else
                                hasEquality = false;

                            if (i + 1 < expression.Length)
                            {
                                int index = i - 1;

                                for (; index >= 0; index--)
                                {
                                    if (expression[index] == '=')
                                        continue;

                                    if (!expression[index].Contains(ariths))
                                        leftex += expression[index];
                                    else
                                    {
                                        if (index - 1 >= 0 && expression[index] == expression[index - 1])
                                        {
                                            index--;
                                            leftex += new string(expression[index], 2);
                                        }
                                        else
                                            break;
                                    }

                                    leftexStartIndex = index;
                                }

                                if (leftex == null)
                                {
                                    MLog.AppErrors.Add(new AppMessage("Unknown Arithmetic operation error.", expression));
                                    return;
                                }

                                leftex = new string(leftex.Reverse().ToArray());

                                if (hasEquality)
                                    i++;

                                for (index = i + 1; index < leng; index++)
                                {
                                    if (expression[index] == '=')
                                        continue;

                                    if (!expression[index].Contains(ariths))
                                        rightex += expression[index];
                                    else
                                    {
                                        if (index + 1 < expression.Length && expression[index] == expression[index + 1])
                                        {
                                            index++;
                                            rightex += new string(expression[index], 2);
                                        }
                                        else
                                            break;
                                    }
                                }

                                if (hasEquality)
                                {
                                    string substring = expression.Substring(i + 1);
                                    expression = expression.Remove(i + 1);

                                    bool hEquality = false;
                                    ExecuteArithmeticObjectExpression(ref substring, context, ariths, ref hEquality);

                                    expression += substring;
                                    rightex = substring;
                                }

                                if (leftex == null) MLog.AppErrors.Add(new AppMessage($"Missing left argument in calculation operation.", expression));
                                if (rightex == null) MLog.AppErrors.Add(new AppMessage($"Missing right argument in calculation operation.", expression));

                                dynamic leftObj = MonoInterpreter.ObjectFromBlockResult(MonoInterpreter.ExecuteArgumentExpression(leftex, context));
                                dynamic rightObj = MonoInterpreter.ObjectFromBlockResult(MonoInterpreter.ExecuteArgumentExpression(rightex, context));

                                if (leftObj != null && rightObj != null)
                                {
                                    var leftObjValue = MonoInterpreter.ValueFromField(leftObj);
                                    var rightObjValue = MonoInterpreter.ValueFromField(rightObj);

                                    MonoType leftType = leftObj as MonoType;
                                    MonoType rightType = rightObj as MonoType;

                                    if (leftType != null && rightType != null)
                                    {
                                        if (leftType.Path != rightType.Path)
                                        {
                                            MLog.AppErrors.Add(new AppMessage("Mathematical operations are not possible with objects of different types.", expression));
                                            return;
                                        }

                                        Operator oper = OperatorCollection.GetOperatorBySign(expression[i].ToString());
                                        Method overloadMethod = leftType.OverloadOperators.FirstOrDefault(sdef => sdef.Name == oper.Name && sdef.Parameters.Count == oper.Parameters);

                                        if (overloadMethod != null)
                                        {
                                            LocalSpace overloadLocalSpace = new LocalSpace(null);
                                            overloadLocalSpace.Fields.Add(new Field(overloadMethod.Parameters[0].Name, null) { Value = leftObj });
                                            overloadLocalSpace.Fields.Add(new Field(overloadMethod.Parameters[1].Name, null) { Value = rightObj });

                                            if (!context.IsStaticObject)
                                            {
                                                FindContext overloadMethodFindContext = new FindContext(overloadMethod);
                                                overloadMethodFindContext.LocalSpace = overloadLocalSpace;
                                                overloadMethodFindContext.MonoType = overloadMethod.ParentObject as MonoType;
                                                overloadMethodFindContext.ScriptFile = overloadMethodFindContext?.MonoType.ParentObject as ScriptFile;

                                                ExecuteBlockResult result = MonoInterpreter.ExecuteScript(overloadMethod.Content, overloadMethod, overloadMethodFindContext, ExecuteScriptContextCollection.Method);
                                                string tmpname = $"monosys_{context.LocalSpace.FreeMonoSysValue}";

                                                if (hasEquality)
                                                    leftObj = result;

                                                context.LocalSpace.Fields.Add(new Field(tmpname, null) { Value = result.ObjectResult });
                                                expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1).Insert(leftexStartIndex, tmpname);
                                            }
                                            else MLog.AppErrors.Add(new AppMessage("Operators cannot be overridden in static classes.", $"Path {leftType.Path}"));
                                        }
                                        else MLog.AppErrors.Add(new AppMessage("No special arithmetic function found for this type.", $"Path {leftType.Path}"));

                                        i = -1;
                                        leng = expression.Length;
                                        leftex = null;

                                        continue;
                                    }

                                    try
                                    {
                                        int summator = 0;
                                        if (hasEquality)
                                        {
                                            i--;
                                            summator = 1;
                                        }

                                        switch (expression[i])
                                        {
                                            case '/':
                                                {
                                                    if (hasEquality)
                                                    {
                                                        if (leftObj as Field == null)
                                                            MLog.AppErrors.Add(new AppMessage($"The {expression[i]}= operator can only be used with variables.", expression));

                                                        MonoInterpreter.SetObjectValue(leftObj, leftObjValue / rightObjValue);
                                                    }

                                                    if (leftObj is Field && hasEquality)
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, leftex);
                                                    else
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, ((double)leftObjValue / (double)rightObjValue).ToString().Replace(",", "."));

                                                    break;
                                                }
                                            case '%':
                                                {
                                                    if (hasEquality)
                                                    {
                                                        if (leftObj as Field == null)
                                                            MLog.AppErrors.Add(new AppMessage($"The {expression[i]}= operator can only be used with variables.", expression));

                                                        MonoInterpreter.SetObjectValue(leftObj, leftObjValue % rightObjValue);
                                                    }
                                                    
                                                    if (leftObj is Field && hasEquality)
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, leftex);
                                                    else
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, ((double)leftObjValue % (double)rightObjValue).ToString().Replace(",", "."));

                                                    break;
                                                }
                                            case '*':
                                                {
                                                    if (hasEquality)
                                                    {
                                                        if (leftObj as Field == null)
                                                            MLog.AppErrors.Add(new AppMessage($"The {expression[i]}= operator can only be used with variables.", expression));

                                                        MonoInterpreter.SetObjectValue(leftObj, leftObjValue * rightObjValue);
                                                    }
                                                    
                                                    if (leftObj is Field && hasEquality)
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, leftex);
                                                    else
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, ((double)leftObjValue * (double)rightObjValue).ToString().Replace(",", "."));

                                                    break;
                                                }
                                            case '-':
                                                {
                                                    if (hasEquality)
                                                    {
                                                        if (leftObj as Field == null)
                                                            MLog.AppErrors.Add(new AppMessage($"The {expression[i]}= operator can only be used with variables.", expression));

                                                        MonoInterpreter.SetObjectValue(leftObj, leftObjValue - rightObjValue);
                                                    }
                                                    
                                                    if (leftObj is Field && hasEquality)
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, leftex);
                                                    else
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, ((double)leftObjValue - (double)rightObjValue).ToString().Replace(",", "."));

                                                    break;
                                                }
                                            case '+':
                                                {
                                                    string pasteString = string.Empty;

                                                    if (hasEquality)
                                                    {
                                                        if (leftObj as Field == null)
                                                            MLog.AppErrors.Add(new AppMessage($"The {expression[i]}= operator can only be used with variables.", expression));

                                                        if (leftObjValue is string leftstr)
                                                        {
                                                            if (leftstr[0].Contains(ReservedCollection.Quotes) && leftstr[0] == leftstr[leftstr.Length - 1])
                                                                pasteString = leftstr.Remove(0, 1).Remove(leftstr.Length - 2);
                                                            else
                                                                pasteString = leftObjValue;

                                                            if (rightObjValue is string rightstr && rightstr[0].Contains(ReservedCollection.Quotes) && rightstr[0] == rightstr[rightstr.Length - 1])
                                                                pasteString += rightstr.Remove(0, 1).Remove(rightstr.Length - 2);
                                                            else
                                                                pasteString += rightObjValue;

                                                            MonoInterpreter.SetObjectValue(leftObj, MonoInterpreter.StringInShell(pasteString));
                                                        }
                                                        else
                                                            MonoInterpreter.SetObjectValue(leftObj, (double)leftObjValue + (double)rightObjValue);
                                                    }
                                                    else
                                                    {
                                                        if (leftObjValue is string || rightObjValue is string)
                                                        {

                                                            if (leftObjValue is string leftstr && leftstr[0].Contains(ReservedCollection.Quotes) && leftstr[0] == leftstr[leftstr.Length - 1])
                                                                pasteString = leftstr.Remove(0, 1).Remove(leftstr.Length - 2);
                                                            else
                                                                pasteString = leftObjValue;

                                                            if (rightObjValue is string rightstr && rightstr[0].Contains(ReservedCollection.Quotes) && rightstr[0] == rightstr[rightstr.Length - 1])
                                                                pasteString += rightstr.Remove(0, 1).Remove(rightstr.Length - 2);
                                                            else
                                                                pasteString += rightObjValue;

                                                            pasteString = MonoInterpreter.StringInShell(pasteString);
                                                        }
                                                    }

                                                    if (leftObj is Field && hasEquality)
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, leftex);
                                                    else if (!string.IsNullOrEmpty(pasteString))
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, pasteString);
                                                    else
                                                        expression = expression.Remove(leftexStartIndex, leftex.Length + rightex.Length + 1 + summator).Insert(leftexStartIndex, (leftObjValue + rightObjValue).ToString().Replace(",", "."));

                                                    break;
                                                }
                                            default:
                                                break;
                                        }

                                        i = -1;
                                        leng = expression.Length;
                                        leftex = null;

                                        continue;
                                    }
                                    catch { MLog.AppErrors.Add(new AppMessage($"Incorrect arithmetic operation. Operation: {expression[i]}", expression)); }
                                }

                                MLog.AppErrors.Add(new AppMessage("Arithmetic operations are not possible with a null object.", expression));
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("Incorrect expression of an arithmetic operation.", expression));

                            return;
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

            return;
        }
        public static dynamic ExecuteDecrementExpression(ref int index, string expression, string objectPath, dynamic lastObj, FindContext context)
        {
            if (lastObj == null)
            {
                if (string.IsNullOrWhiteSpace(objectPath))
                {
                    int bracketSquareCount = 0;
                    InsideQuoteModel quoteModel = new InsideQuoteModel();
                    for (index += 2; index < expression.Length; index++)
                    {
                        Extensions.IsOpenQuote(expression, index, ref quoteModel);

                        if (!quoteModel.HasQuotes)
                        {
                            if (expression[index] == '[')
                                bracketSquareCount++;

                            if (expression[index] == ']')
                                bracketSquareCount--;

                            if (expression[index].Contains(".()") && bracketSquareCount == 0)
                            {
                                index--;
                                break;
                            }
                        }

                        objectPath += expression[index];
                    }

                    lastObj = MonoInterpreter.ExecuteArithmeticExpression(objectPath, context);

                    if (lastObj is Field field && field.Value is double)
                        --field.Value;
                    else
                        MLog.AppErrors.Add(new AppMessage("Use decrement is possible only for fields of type Number.", expression));
                }
                else
                {
                    lastObj = Finder.FindObject(objectPath, context, FindOption.FindField);

                    if (lastObj is Field field && field.Value is double)
                        field.Value--;
                    else
                        MLog.AppErrors.Add(new AppMessage("Use decrement is possible only for fields of type Number.", expression));
                }
            }
            else
            {
                if (lastObj is Field field && field.Value is double)
                    field.Value--;
                else
                    MLog.AppErrors.Add(new AppMessage("Use decrement is possible only for fields of type Number.", expression));
            }


            return lastObj;
        }
        public static dynamic ExecuteIncrementExpression(ref int index, string expression, string objectPath, dynamic lastObj, FindContext context)
        {
            if (lastObj == null)
            {
                if (string.IsNullOrWhiteSpace(objectPath))
                {
                    int bracketSquareCount = 0;
                    InsideQuoteModel quoteModel = new InsideQuoteModel();
                    for (index += 2; index < expression.Length; index++)
                    {
                        Extensions.IsOpenQuote(expression, index, ref quoteModel);

                        if (!quoteModel.HasQuotes)
                        {
                            if (expression[index] == '[')
                                bracketSquareCount++;

                            if (expression[index] == ']')
                                bracketSquareCount--;

                            if (expression[index].Contains(".()") && bracketSquareCount == 0)
                            {
                                index--;
                                break;
                            }
                        }

                        objectPath += expression[index];
                    }

                    lastObj = MonoInterpreter.ExecuteArithmeticExpression(objectPath, context);

                    if (lastObj is Field field && field.Value is double)
                        ++field.Value;
                    else
                        MLog.AppErrors.Add(new AppMessage("Use increment is possible only for fields of type Number.", expression));
                }
                else
                {
                    lastObj = Finder.FindObject(objectPath, context, FindOption.FindField);

                    if (lastObj is Field field && field.Value is double)
                        field.Value++;
                    else
                        MLog.AppErrors.Add(new AppMessage("Use increment is possible only for fields of type Number.", expression));
                }
            }
            else
            {
                if (lastObj is Field field && field.Value is double)
                    field.Value++;
                else
                    MLog.AppErrors.Add(new AppMessage("Use increment is possible only for fields of type Number.", expression));
            }

            return lastObj;
        }
        public static dynamic ExecuteThisExpression(ref int index, string expression, FindContext context)
        {
            string resultString = string.Empty;

            if (index + 3 < expression.Length)
            {
                if (index == 0)
                {
                    if (expression[index] == 't' && expression[index + 1] == 'h' && expression[index + 2] == 'i' && expression[index + 3] == 's')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            resultString = "this";
                        if (expression.Length == 4)
                            resultString = "this";
                    }
                }
                if (index - 1 >= 0 && !expression[index - 1].Contains(ReservedCollection.AllowedNames))
                {
                    if (expression[index] == 't' && expression[index + 1] == 'h' && expression[index + 2] == 'i' && expression[index + 3] == 's')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            resultString = "this";
                        if (expression.Length == 4)
                            resultString = "this";
                    }
                }
            }

            if (resultString == "this")
            {
                if (context.IsStaticObject)
                {
                    MLog.AppErrors.Add(new AppMessage("Operator this cannot be called in a static class.", expression));
                    return false;
                }
                else
                    return context.MonoType;
            }

            return null;
        }
        public static dynamic ExecuteNullExpression(ref int index, string expression)
        {
            if (index + 3 < expression.Length)
            {
                if (index == 0)
                {
                    if (expression[index] == 'n' && expression[index + 1] == 'u' && expression[index + 2] == 'l' && expression[index + 3] == 'l')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            return null;
                        if (expression.Length == 4)
                            return null;
                    }
                }
                if (index - 1 >= 0 && !expression[index - 1].Contains(ReservedCollection.AllowedNames))
                {
                    if (expression[index] == 'n' && expression[index + 1] == 'u' && expression[index + 2] == 'l' && expression[index + 3] == 'l')
                    {
                        index += 3;

                        if (index + 4 >= expression.Length || !expression[index + 4].Contains(ReservedCollection.AllowedNames))
                            return null;
                        if (expression.Length == 4)
                            return null;
                    }
                }
            }

            return false;
        }
        public static dynamic ExecuteBooleanExpression(ref int index, string expression)
        {
            if (index + 3 < expression.Length)
            {
                if (index == 0)
                {
                    if (expression[index] == 't' && expression[index + 1] == 'r' && expression[index + 2] == 'u' && expression[index + 3] == 'e')
                    {
                        index += 3;

                        if (index + 1 >= expression.Length || !expression[index + 1].Contains(ReservedCollection.AllowedNames))
                            return true;
                        if (expression.Length == 4)
                            return true;
                    }

                    if (expression[index] == 'f' && expression[index + 1] == 'a' && expression[index + 2] == 'l' && expression[index + 3] == 's' && expression[index + 4] == 'e')
                    {
                        index += 4;

                        if (index + 1 >= expression.Length || !expression[index + 1].Contains(ReservedCollection.AllowedNames))
                            return false;
                        if (expression.Length == 5)
                            return false;
                    }
                }
                if (index - 1 >= 0 && !expression[index - 1].Contains(ReservedCollection.AllowedNames))
                {
                    if (expression[index] == 't' && expression[index + 1] == 'r' && expression[index + 2] == 'u' && expression[index + 3] == 'e')
                    {
                        index += 3;

                        if (index + 1 >= expression.Length || !expression[index + 1].Contains(ReservedCollection.AllowedNames))
                            return true;
                        if (expression.Length == 4)
                            return true;
                    }

                    if (expression[index] == 'f' && expression[index + 1] == 'a' && expression[index + 2] == 'l' && expression[index + 3] == 's' && expression[index + 4] == 'e')
                    {
                        index += 4;

                        if (index + 1 >= expression.Length || !expression[index + 1].Contains(ReservedCollection.AllowedNames))
                            return false;
                        if (expression.Length == 5)
                            return false;
                    }
                }
            }

            return null;
        }
        public static dynamic ExecuteNumberExpression(ref int index, string expression, InsideQuoteModel quoteModel = null)
        {
            string numberString = string.Empty;
            bool hasBodyNumber = false, hasResidue = false;
            bool isNegative = false;
            double resultDouble;

            if (quoteModel == null)
                quoteModel = new InsideQuoteModel();

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (!quoteModel.HasQuotes)
                {
                    if (index + 1 == expression.Length)
                    {
                        if (expression[index].Contains(ReservedCollection.Numbers))
                            numberString += expression[index];

                        if (double.TryParse(numberString, out resultDouble))
                            return resultDouble;
                    }

                    if (expression[index].Contains(ReservedCollection.Numbers))
                    {
                        numberString += expression[index];

                        if (!hasResidue)
                            hasBodyNumber = true;
                    }
                    else
                    {
                        if (expression[index] == '+')
                        {
                            if (!string.IsNullOrWhiteSpace(numberString))
                            {
                                if (index + 1 < expression.Length && expression[index + 1] == '+')
                                    MLog.AppErrors.Add(new AppMessage("Incorrect increment declaration.", expression));
                                else
                                    MLog.AppErrors.Add(new AppMessage("Invalid addition operator.", expression));

                                return null;
                            }
                            else
                                return null;
                        }

                        if (expression[index] == '-')
                        {
                            if (string.IsNullOrWhiteSpace(numberString))
                            {
                                if (!isNegative)
                                {
                                    isNegative = true;
                                    numberString += "-";
                                }
                                else
                                {
                                    MLog.AppErrors.Add(new AppMessage("Incorrect decrement declaration.", expression));
                                    return null;
                                }
                            }
                            else
                            {
                                if (index + 1 < expression.Length && expression[index + 1] == '-')
                                    MLog.AppErrors.Add(new AppMessage("Incorrect decrement declaration.", expression));

                                return null;
                            }
                        }

                        if (!hasResidue)
                        {
                            if (expression[index] == '.')
                            {
                                if (hasBodyNumber)
                                {
                                    if (index + 1 < expression.Length)
                                    {
                                        if (expression[index + 1].Contains(ReservedCollection.Numbers))
                                        {
                                            hasResidue = true;
                                            numberString += ",";
                                        }
                                        else
                                        {
                                            if (double.TryParse(numberString, out resultDouble))
                                                return resultDouble;

                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (double.TryParse(numberString, out resultDouble))
                                            return resultDouble;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (index + 1 < expression.Length)
                                    {
                                        if (expression[index + 1].Contains(ReservedCollection.Numbers))
                                        {
                                            hasResidue = true;
                                            numberString += ",";
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }
                            }
                            if (expression[index] == ' ')
                            {
                                if (hasBodyNumber)
                                {
                                    if (double.TryParse(numberString, out resultDouble))
                                        return resultDouble;

                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (expression[index].Contains(". "))
                            {
                                if (hasBodyNumber)
                                {
                                    if (double.TryParse(numberString, out resultDouble))
                                        return resultDouble;

                                    break;
                                }
                                else
                                {
                                    if (numberString.Length >= 2)
                                    {
                                        if (double.TryParse(numberString, out resultDouble))
                                            return resultDouble;

                                        break;
                                    }
                                    else
                                    {
                                        MLog.AppErrors.Add(new AppMessage("Invalid number declaration.", expression));
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                MLog.AppErrors.Add(new AppMessage("Invalid number declaration.", expression));
                                break;
                            }
                        }
                    }
                }
                else
                {
                    MLog.AppErrors.Add(new AppMessage("Invalid line declaration after number.", expression));
                    break;
                }
            }

            return null;
        }
        public static dynamic ExecuteStringExpression(ref int index, string expression, InsideQuoteModel quoteModel = null)
        {
            string resultString = string.Empty;
            bool firstOpen = quoteModel == null ? false : quoteModel.HasQuotes;

            if (quoteModel != null)
            {
                if (expression[index] == quoteModel.Quote)
                    index++;
            }
            else
                quoteModel = new InsideQuoteModel();

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (quoteModel.HasQuotes)
                {
                    firstOpen = true;

                    if (quoteModel.IsOnlyString && expression[index] != quoteModel.Quote)
                        resultString += expression[index];

                    if (!quoteModel.IsOnlyString)
                    {
                        if (expression[index] == '\\')
                        {
                            if (index + 1 < expression.Length)
                            {
                                char? portableCharacter = PortableCharacterCollection.GetCharacterByString(new string(new char[] { expression[index], expression[index + 1] }));

                                if (portableCharacter != null)
                                {
                                    index++;
                                    resultString += portableCharacter.Value;
                                }
                                else
                                    MLog.AppErrors.Add(new AppMessage("Special character not found.", expression));
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("No special character specified.", expression));
                        }
                        else if (expression[index] != quoteModel.Quote)
                            resultString += expression[index];
                    }
                    else
                    {
                        if (expression[index] == '\\')
                            resultString += "\\";
                    }
                }
                else if (firstOpen)
                    break;
            }

            return $"\"{resultString}\"";
        }
        public static dynamic ExecuteArrayExpression(ref int index, string expression, FindContext context, bool isRecursive = false)
        {
            List<dynamic> arrayValues = new List<dynamic>();

            string tmpex = null;
            bool hasComma = true;
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (quoteModel.HasQuotes)
                    tmpex += expression[index];
                else if (!expression[index].Contains("[],"))
                    tmpex += expression[index];
                else
                {
                    if (expression[index] == ',')
                    {
                        hasComma = true;

                        if (!string.IsNullOrWhiteSpace(tmpex))
                        {
                            Field field = new Field(string.Empty, null);
                            field.Value = MonoInterpreter.ExecuteExpression(tmpex, context);

                            arrayValues.Add(field);
                            tmpex = null;
                        }

                        continue;
                    }

                    if (expression[index] == '[')
                    {
                        if (hasComma)
                        {
                            index++;
                            hasComma = false;

                            arrayValues.Add(ExecuteArrayExpression(ref index, expression, context, true));

                            if (index < expression.Length && expression[index] == ',')
                                index--;

                            continue;
                        }
                        else
                            MLog.AppErrors.Add(new AppMessage("Array element separator not found.", expression));
                    }

                    if (expression[index] == ']')
                    {
                        index++;
                        if (!string.IsNullOrWhiteSpace(tmpex))
                        {
                            Field field = new Field(string.Empty, null);
                            field.Value = MonoInterpreter.ExecuteExpression(tmpex, context);

                            arrayValues.Add(field);
                        }
                        break;
                    }
                }

                if (!quoteModel.HasQuotes && expression[index] == '(')
                    {
                        index++;
                        int opencount = 1;
                        for (; index < expression.Length; index++)
                        {
                            tmpex += expression[index];
                            Extensions.IsOpenQuote(expression, index, ref quoteModel);

                            if (!quoteModel.HasQuotes)
                            {
                                if (expression[index] == '(')
                                    opencount++;

                                if (expression[index] == ')')
                                    opencount--;

                                if (opencount == 0)
                                    break;
                            }
                        }

                        continue;
                    }
            }

            if (!isRecursive && arrayValues.Count == 1)
                return arrayValues[0];

            return arrayValues;
        }
        public static dynamic ExecuteOperatorGetElementExpression(ref int index, string expression, string methodPath, dynamic lastObj, FindContext context)
        {
            var inputs = HelperExpressions.GetObjectMethodParameters(HelperExpressions.GetStringMethodParameters(expression, ref index), context);

            if (lastObj is MonoType monoObjType && !string.IsNullOrWhiteSpace(methodPath))
            {
                string newMethodPath1 = IPath.CombinePath(methodPath, monoObjType.Name);
                string newMethodPath2 = IPath.CombinePath(methodPath, monoObjType.FullPath);

                lastObj = Finder.FindObject(newMethodPath1, context, FindOption.FindMethod, inputs.Count);

                if (lastObj == null)
                    lastObj = Finder.FindObject(newMethodPath2, context, FindOption.FindMethod, inputs.Count);
            }
            else if (lastObj as Method == null)
                lastObj = Finder.FindObject(methodPath, context, FindOption.FindMethod | FindOption.FindField, inputs.Count);

            if (lastObj != null)
            {
                if (lastObj is MonoType objType)
                {
                    Method overloadMethod = objType.OverloadOperators.FirstOrDefault(sdef => sdef.Name == OperatorCollection.GetElement.Name);

                    if (overloadMethod != null)
                    {
                        lastObj = ObjectExpressions.ExecuteMethod(overloadMethod, inputs);
                    }
                    else
                        MLog.AppErrors.Add(new AppMessage("No overload method found for GetElement statement.", $"Object {objType.Name}"));
                }
                else if (Extensions.HasEnumerator(MonoInterpreter.ValueFromField(lastObj)))
                {
                    lastObj = MonoInterpreter.ValueFromField(lastObj);

                    if (inputs.Count == 1 && inputs[0].Value is double numberValue)
                    {
                        dynamic resultGetIndex = lastObj[int.Parse(numberValue.ToString().Split(',')[0])];

                        if (resultGetIndex is char)
                            lastObj = resultGetIndex.ToString();
                        else
                            lastObj = resultGetIndex;
                    }
                    else
                        MLog.AppErrors.Add(new AppMessage("Invalid array element retrieval options.", expression));
                }
                else
                    MLog.AppErrors.Add(new AppMessage("An object is not an array, structure, or class.", expression));
            }
            else
                MLog.AppErrors.Add(new AppMessage("Object does not exist.", $"Object {methodPath}"));

            return lastObj;
        }
        public static dynamic ExecuteMethodExpression(ref int index, string expression, string methodPath, dynamic lastObj, FindContext context)
        {
            List<(string Name, dynamic Value)> inputs = HelperExpressions.GetObjectMethodParameters(HelperExpressions.GetStringMethodParameters(expression, ref index), context);

            dynamic saveLastObj = lastObj;

            if (lastObj is MonoType objType && !string.IsNullOrWhiteSpace(methodPath))
            {
                string newMethodPath1 = IPath.CombinePath(methodPath, objType.Name);
                string newMethodPath2 = IPath.CombinePath(methodPath, objType.FullPath);

                lastObj = Finder.FindObject(newMethodPath1, context, FindOption.FindMethod, inputs.Count);

                if (lastObj == null)
                    lastObj = Finder.FindObject(newMethodPath2, context, FindOption.FindMethod, inputs.Count);
            }
            else if (lastObj as Method == null)
                lastObj = Finder.FindObject(methodPath, context, FindOption.FindMethod, inputs.Count);

            if (lastObj == null)
                lastObj = saveLastObj;

            if (lastObj is Method)
                return ExecuteMethod(lastObj, inputs);

            if (lastObj == null)
                return BasicMethods.InvokeMethod(methodPath, false, inputs.Select(x => x.Value).ToArray());

            return BasicMethods.InvokeMethod(methodPath, true, lastObj);
        }
        public static List<Field> ExecuteVarExpression(ref int index, string expression, Method method, FindContext context, InsideQuoteModel quoteModel = null, params string[] modifiers)
        {
            List<string> varList = new List<string>();

            string varExpression = null;
            int bracketRoundCount = 0, bracketSquareCount = 0;
            bool hasSeparator = false, hasAnotherSym = false;

            if (quoteModel == null)
                quoteModel = new InsideQuoteModel();

            List<Field> ExecuteAndReturnField()
            {
                List<Field> fields = new List<Field>();

                foreach (var varExpression in varList)
                {
                    if (!string.IsNullOrWhiteSpace(varExpression))
                    {
                        string[] splits = varExpression.Split('=', 2);

                        var field = new Field(splits[0], method);

                        field.AddModifiers(modifiers.ToList());

                        if (splits.Length == 2)
                        {
                            var result = MonoInterpreter.ExecuteExpression(splits[1], context);

                            if (result is Struct resultStruct)
                                result = resultStruct.CloneObject();

                            field.Value = result;
                        }

                        fields.Add(field);
                        context.LocalSpace.Add(field);
                    }
                    else
                        MLog.AppErrors.Add(new AppMessage("Variable declaration expression cannot be empty.", expression));
                }

                return fields;
            }

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (quoteModel.HasQuotes && !hasSeparator)
                    MLog.AppErrors.Add(new AppMessage("Missing assignment separator.", expression));

                if ((!quoteModel.HasQuotes && expression[index] == '\n') || (!quoteModel.IsOnlyString && expression[index] == '\n') || (index + 1 == expression.Length))
                {
                    if (hasSeparator)
                        varExpression += expression[index];

                    if (!string.IsNullOrWhiteSpace(varExpression))
                        varList.Add(varExpression);

                    if (varList.Count == 0)
                        MLog.AppErrors.Add(new AppMessage("Missing variable name.", expression));

                    if (quoteModel.HasQuotes)
                    {
                        if (!quoteModel.IsOnlyString)
                        {
                            index++;
                            MLog.AppErrors.Add(new AppMessage("String expression was not closed.", expression));

                            return ExecuteAndReturnField();
                        }
                        else
                            continue;
                    }

                    index++;
                    return ExecuteAndReturnField();
                }

                if (!quoteModel.HasQuotes)
                {
                    if (hasSeparator)
                    {
                        if (expression[index] == '(')
                            bracketRoundCount++;

                        if (expression[index] == ')')
                            bracketRoundCount--;

                        if (expression[index] == '[')
                            bracketSquareCount++;

                        if (expression[index] == ']')
                            bracketSquareCount--;
                    }

                    if (!hasSeparator && expression[index] == '=')
                    {
                        if (!string.IsNullOrEmpty(varExpression))
                            hasSeparator = true;
                        else
                            MLog.AppErrors.Add(new AppMessage("Missing variable name.", expression));

                        varExpression += "=";

                        continue;
                    }

                    if (bracketRoundCount + bracketSquareCount == 0)
                    {
                        if (expression[index] == ',')
                        {
                            varList.Add(varExpression);

                            hasSeparator = false;
                            hasAnotherSym = false;
                            varExpression = string.Empty;
                            continue;
                        }

                        if (expression[index] == ';')
                        {
                            index++;

                            if (!string.IsNullOrWhiteSpace(varExpression))
                                varList.Add(varExpression);

                            if (varList.Count == 0)
                                MLog.AppErrors.Add(new AppMessage("Missing variable name.", expression));

                            return ExecuteAndReturnField();
                        }
                    }
                }

                if (!hasSeparator)
                {
                    if (!quoteModel.HasQuotes)
                    {
                        if (expression[index].Contains(ReservedCollection.AllowedNames))
                        {
                            if (!hasAnotherSym)
                                varExpression += expression[index];
                            else
                                break;

                            continue;
                        }
                        else if (expression[index] == ' ')
                        {
                            if (!string.IsNullOrWhiteSpace(varExpression))
                                hasAnotherSym = true;

                            continue;
                        }
                        else
                            break;
                    }

                    break;
                }
                else
                    varExpression += expression[index];
            }

            return null;
        }
        public static ExecuteBlockResult ExecuteForExpression(ref int index, string expression, Method method, FindContext context, InsideQuoteModel quoteModel = null)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();

            if (quoteModel == null)
                quoteModel = new InsideQuoteModel();

            string equalityExpression = string.Empty, afterExpression = string.Empty;
            List<string> afterExpressionList = new List<string>();

            FirstIndex firstIndex = Extensions.FindFirstIndex(expression, index, "(", ReservedCollection.WhiteSpace);

            if (firstIndex.IsFirst)
            {
                int separatorCount = 0;
                int bracketRoundCount = 1, bracketSquareCount = 0;

                for (index = firstIndex.Position + 1; index < expression.Length; index++)
                {
                    Extensions.IsOpenQuote(expression, index, ref quoteModel);

                    if (!quoteModel.HasQuotes) 
                    {
                        if (expression[index] == ';' && bracketRoundCount + bracketSquareCount == 1)
                        {
                            separatorCount++;
                            continue;
                        }

                        if (expression[index] == '(')
                            bracketRoundCount++;

                        if (expression[index] == ')')
                            bracketRoundCount--;

                        if (expression[index] == '[')
                            bracketSquareCount++;

                        if (expression[index] == ']')
                            bracketSquareCount--;

                        if (bracketRoundCount + bracketSquareCount == 0)
                        {
                            if (!string.IsNullOrWhiteSpace(afterExpression))
                                afterExpressionList.Add(afterExpression);

                            index++;
                            break;
                        }
                    }

                    if (separatorCount == 0)
                    {
                        var first = Extensions.FindFirstIndex(expression, index, "v", ReservedCollection.WhiteSpace);

                        if (first.IsFirst)
                        {
                            if (first.Position + 2 < expression.Length && expression[first.Position + 1] == 'a' && expression[first.Position + 2] == 'r')
                            {
                                index = first.Position + 3;

                                ExecuteVarExpression(ref index, expression, method, context, quoteModel);
                            }
                            else
                                MonoInterpreter.ExecuteExpression(expression, context);
                        }
                        else
                            MonoInterpreter.ExecuteExpression(expression, context);

                        separatorCount++;
                        continue;
                    }

                    if (separatorCount == 1)
                        equalityExpression += expression[index];

                    if (separatorCount == 2)
                    {
                        if (!quoteModel.HasQuotes && bracketRoundCount + bracketSquareCount == 1 && expression[index] == ',')
                        {
                            afterExpressionList.Add(afterExpression);
                            afterExpression = string.Empty;
                            continue;
                        }

                        afterExpression += expression[index];
                    }
                }
            }
            else
                MLog.AppErrors.Add(new AppMessage("Invalid For statement declaration.", expression));

            FirstIndex firstBracket = Extensions.FindFirstIndex(expression, index, "{", ReservedCollection.WhiteSpace);

            if (!firstBracket.IsFirst)
                MLog.AppErrors.Add(new AppMessage("Invalid operator expression For.", expression));
            else
            {
                int bracketCount = 1;
                index = firstBracket.Position + 1;

                for (; index < expression.Length; index++)
                {
                    Extensions.IsOpenQuote(expression, index, ref quoteModel);

                    if (!quoteModel.HasQuotes)
                    {
                        if (expression[index] == '{')
                            bracketCount++;

                        if (expression[index] == '}')
                            bracketCount--;

                        if (bracketCount == 0)
                        {
                            break;
                        }
                    }
                }

                while (MonoInterpreter.ExecuteExpression(equalityExpression, context))
                {
                    LocalSpace saveLocalSpace = context.LocalSpace;
                    context.LocalSpace = new LocalSpace(context.LocalSpace);

                    ExecuteBlockResult executeBlockResult = MonoInterpreter.ExecuteScript(Extensions.SubstringIndex(expression, firstBracket.Position + 1, index), method, context, ExecuteScriptContextCollection.CycleOrSwitch);
                    context.LocalSpace = saveLocalSpace;

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Return)
                        return executeBlockResult;

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Continue || executeBlockResult.ResultType == ExecuteResultCollection.None)
                    {
                        foreach (var afterexp in afterExpressionList)
                        {
                            MonoInterpreter.ExecuteExpression(afterexp, context);
                        }

                        continue;
                    }

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Break)
                    {
                        if (executeBlockResult.Count > 0)
                            return executeBlockResult;

                        executeBlockResult.ResultType = ExecuteResultCollection.None;
                        return executeBlockResult;
                    }

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Quit)
                    {
                        if (executeBlockResult.Count > 0)
                            return executeBlockResult;

                        executeBlockResult.ResultType = ExecuteResultCollection.None;
                        return executeBlockResult;
                    }
                }
            }

            return executeResult;
        }
        public static ExecuteBlockResult ExecuteWhileExpression(ref int index, string expression, Method method, FindContext context, InsideQuoteModel quoteModel = null)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();

            if (quoteModel == null)
                quoteModel = new InsideQuoteModel();

            string equalityExpression = string.Empty;

            FirstIndex firstIndex = Extensions.FindFirstIndex(expression, index, "(", ReservedCollection.WhiteSpace);

            if (firstIndex.IsFirst)
            {
                int bracketRoundCount = 1, bracketSquareCount = 0;

                for (index = firstIndex.Position + 1; index < expression.Length; index++)
                {
                    Extensions.IsOpenQuote(expression, index, ref quoteModel);

                    if (!quoteModel.HasQuotes)
                    {
                        if (expression[index] == ';' && bracketRoundCount + bracketSquareCount == 1)
                        {
                            MLog.AppErrors.Add(new AppMessage("Expression terminator cannot be declared in a conditional expression.", expression));
                            continue;
                        }

                        if (expression[index] == '(')
                            bracketRoundCount++;

                        if (expression[index] == ')')
                            bracketRoundCount--;

                        if (expression[index] == '[')
                            bracketSquareCount++;

                        if (expression[index] == ']')
                            bracketSquareCount--;

                        if (bracketRoundCount + bracketSquareCount == 0)
                        {
                            index++;
                            break;
                        }
                    }

                    equalityExpression += expression[index];
                }
            }
            else
                MLog.AppErrors.Add(new AppMessage("Invalid While statement declaration.", expression));

            FirstIndex firstBracket = Extensions.FindFirstIndex(expression, index, "{", ReservedCollection.WhiteSpace);

            if (!firstBracket.IsFirst)
                MLog.AppErrors.Add(new AppMessage("Invalid operator expression While.", expression));
            else
            {
                int bracketCount = 1;
                index = firstBracket.Position + 1;

                for (; index < expression.Length; index++)
                {
                    Extensions.IsOpenQuote(expression, index, ref quoteModel);

                    if (!quoteModel.HasQuotes)
                    {
                        if (expression[index] == '{')
                            bracketCount++;

                        if (expression[index] == '}')
                            bracketCount--;

                        if (bracketCount == 0)
                        {
                            break;
                        }
                    }
                }

                while (MonoInterpreter.ExecuteExpression(equalityExpression, context))
                {
                    LocalSpace saveLocalSpace = context.LocalSpace;
                    context.LocalSpace = new LocalSpace(context.LocalSpace);

                    ExecuteBlockResult executeBlockResult = MonoInterpreter.ExecuteScript(Extensions.SubstringIndex(expression, firstBracket.Position + 1, index), method, context, ExecuteScriptContextCollection.CycleOrSwitch);
                    context.LocalSpace = saveLocalSpace;

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Return)
                        return executeBlockResult;

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Continue || executeBlockResult.ResultType == ExecuteResultCollection.None)
                        continue;

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Break)
                    {
                        if (executeBlockResult.Count > 0)
                            return executeBlockResult;

                        executeBlockResult.ResultType = ExecuteResultCollection.None;
                        return executeBlockResult;
                    }

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Quit)
                    {
                        if (executeBlockResult.Count > 0)
                            return executeBlockResult;

                        executeBlockResult.ResultType = ExecuteResultCollection.None;
                        return executeBlockResult;
                    }
                }
            }

            return executeResult;
        }
        public static ExecuteBlockResult ExecuteIfExpression(ref int index, string expression, Method method, FindContext context, InsideQuoteModel quoteModel = null)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();
            executeResult.ResultType = ExecuteResultCollection.None;

            if (quoteModel == null)
                quoteModel = new InsideQuoteModel();

            string equalityExpression = string.Empty;

            FirstIndex firstIndex = Extensions.FindFirstIndex(expression, index, "(", ReservedCollection.WhiteSpace);

            if (firstIndex.IsFirst)
            {
                int bracketRoundCount = 1, bracketSquareCount = 0;

                for (index = firstIndex.Position + 1; index < expression.Length; index++)
                {
                    Extensions.IsOpenQuote(expression, index, ref quoteModel);

                    if (!quoteModel.HasQuotes)
                    {
                        if (expression[index] == ';' && bracketRoundCount + bracketSquareCount == 1)
                        {
                            MLog.AppErrors.Add(new AppMessage("Expression terminator cannot be declared in a conditional expression.", expression));
                            continue;
                        }

                        if (expression[index] == '(')
                            bracketRoundCount++;

                        if (expression[index] == ')')
                            bracketRoundCount--;

                        if (expression[index] == '[')
                            bracketSquareCount++;

                        if (expression[index] == ']')
                            bracketSquareCount--;

                        if (bracketRoundCount + bracketSquareCount == 0)
                        {
                            index++;
                            break;
                        }
                    }

                    equalityExpression += expression[index];
                }
            }
            else
                MLog.AppErrors.Add(new AppMessage("Invalid If statement declaration.", expression));

            FirstIndex firstBracket = Extensions.FindFirstIndex(expression, index, "{", ReservedCollection.WhiteSpace);

            if (!firstBracket.IsFirst)
                MLog.AppErrors.Add(new AppMessage("Invalid operator expression If.", expression));
            else
            {
                int bracketCount = 1;
                index = firstBracket.Position + 1;

                for (; index < expression.Length; index++)
                {
                    Extensions.IsOpenQuote(expression, index, ref quoteModel);

                    if (!quoteModel.HasQuotes)
                    {
                        if (expression[index] == '{')
                            bracketCount++;

                        if (expression[index] == '}')
                            bracketCount--;

                        if (bracketCount == 0)
                        {
                            break;
                        }
                    }
                }

                if (MonoInterpreter.ExecuteExpression(equalityExpression, context))
                {
                    LocalSpace saveLocalSpace = context.LocalSpace;
                    context.LocalSpace = new LocalSpace(context.LocalSpace);

                    ExecuteBlockResult executeBlockResult = MonoInterpreter.ExecuteScript(Extensions.SubstringIndex(expression, firstBracket.Position + 1, index), method, context, ExecuteScriptContextCollection.IfElseSwitch);
                    context.LocalSpace = saveLocalSpace;

                    if (executeBlockResult.ResultType == ExecuteResultCollection.None)
                    {
                        index++;
                        HelperExpressions.SkipIfElseRemaining(ref index, expression);
                        return executeBlockResult;
                    }

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Return || executeBlockResult.ResultType == ExecuteResultCollection.Continue)
                        return executeBlockResult;

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Break)
                    {
                        if (executeBlockResult.Count > 0)
                            return executeBlockResult;

                        executeBlockResult.ResultType = ExecuteResultCollection.None;
                        return executeBlockResult;
                    }

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Quit)
                    {
                        if (executeBlockResult.Count > 0)
                            return executeBlockResult;

                        executeBlockResult.ResultType = ExecuteResultCollection.None;
                        return executeBlockResult;
                    }
                }
                else
                {
                    firstBracket = Extensions.FindFirstIndex(expression, index + 1, "e", ReservedCollection.WhiteSpace);

                    if (firstBracket.IsFirst)
                    {
                        if (firstBracket.Position + 3 < expression.Length && expression[firstBracket.Position + 1] == 'l' && expression[firstBracket.Position + 2] == 's' && expression[firstBracket.Position + 3] == 'e')
                        {
                            index = firstBracket.Position + 4;

                            return ExecuteElseExpression(ref index, expression, method, context, quoteModel);
                        }
                    }
                }
            }

            return executeResult;
        }
        public static ExecuteBlockResult ExecuteElseExpression(ref int index, string expression, Method method, FindContext context, InsideQuoteModel quoteModel = null)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();
            executeResult.ResultType = ExecuteResultCollection.None;

            if (quoteModel == null)
                quoteModel = new InsideQuoteModel();

            FirstIndex first = Extensions.FindFirstIndex(expression, index, "i{", ReservedCollection.WhiteSpace);

            if (first.IsFirst)
            {
                if (first.FirstChar == '{')
                {
                    index = first.Position + 1;
                    int bracketCount = 1;

                    for (; index < expression.Length; index++)
                    {
                        Extensions.IsOpenQuote(expression, index, ref quoteModel);

                        if (!quoteModel.HasQuotes)
                        {
                            if (expression[index] == '{')
                                bracketCount++;

                            if (expression[index] == '}')
                                bracketCount--;

                            if (bracketCount == 0)
                            {
                                break;
                            }
                        }
                    }

                    LocalSpace saveLocalSpace = context.LocalSpace;
                    context.LocalSpace = new LocalSpace(context.LocalSpace);

                    ExecuteBlockResult executeBlockResult = MonoInterpreter.ExecuteScript(Extensions.SubstringIndex(expression, first.Position + 1, index), method, context, ExecuteScriptContextCollection.IfElseSwitch);
                    context.LocalSpace = saveLocalSpace;

                    if (executeBlockResult.ResultType == ExecuteResultCollection.None)
                    {
                        index++;
                        HelperExpressions.SkipIfElseRemaining(ref index, expression);
                        return executeBlockResult;
                    }

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Return || executeBlockResult.ResultType == ExecuteResultCollection.Continue)
                        return executeBlockResult;

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Break)
                    {
                        if (executeBlockResult.Count > 0)
                            return executeBlockResult;

                        executeBlockResult.ResultType = ExecuteResultCollection.None;
                        return executeBlockResult;
                    }

                    if (executeBlockResult.ResultType == ExecuteResultCollection.Quit)
                    {
                        if (executeBlockResult.Count > 0)
                            return executeBlockResult;

                        executeBlockResult.ResultType = ExecuteResultCollection.None;
                        return executeBlockResult;
                    }
                }
                else
                {
                    index = first.Position - 1;
                    executeResult.ResultType = ExecuteResultCollection.None;
                }
            }
            else
                MLog.AppErrors.Add(new AppMessage("The else statement must be followed by its body or the declaration of the if statement.", expression));

            return executeResult;
        }
        public static ExecuteBlockResult ExecuteReturnExpression(ref int index, string expression, FindContext context)
        {
            ExecuteBlockResult executeResult = new ExecuteBlockResult();
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            string resultString = string.Empty;
            string ignoreChars = "\n\r;";

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (quoteModel.HasQuotes || !expression[index].Contains(ignoreChars))
                    resultString += expression[index];

                if (!quoteModel.HasQuotes && expression[index].Contains("\n;") || index + 1 == expression.Length)
                {
                    executeResult.ObjectResult = MonoInterpreter.ExecuteExpression(resultString, context);
                    executeResult.ResultType = ExecuteResultCollection.Return;
                    break;
                }
            }

            return executeResult;
        }
        public static uint ExecuteExitExpression(ref int index, string expression)
        {
            FirstIndex first = Extensions.FindFirstIndex(expression, index, ReservedCollection.Numbers, "\r\t ");
            index = first.Position;

            if (first.IsFirst)
            {
                string number = string.Empty;

                for (; index < expression.Length; index++)
                {
                    if (expression[index].Contains(";\n"))
                        break;

                    number += expression[index];
                }

                uint newUInt;
                if (uint.TryParse(IPath.NormalizeWithTrim(number), out newUInt))
                    return newUInt;

                MLog.AppErrors.Add(new AppMessage("The number of outputs must be a positive integer value.", expression));
            }
            else
            {
                if (first.FirstChar.Contains(";\n"))
                    return 1;
                else
                    MLog.AppErrors.Add(new AppMessage("Invalid exit statement declaration.", expression));
            }

            return 0;
        }
        public static dynamic ExecuteMethod(Method originalMethod, List<(string Name, dynamic Value)> inputs)
        {
            Method methodClone = (originalMethod as Method).CloneObject();
            methodClone.UpdateParametersFromInputs(inputs);

            FindContext context = new FindContext(methodClone) { LocalSpace = LocalSpace.LoadFromMethod(methodClone) };
            context.MonoType = methodClone.ParentObject as MonoType;
            context.ScriptFile = context.MonoType?.ParentObject as ScriptFile;

            if (methodClone.IsConstructor)
                return ExecuteConstructor(methodClone, context);

            return MonoInterpreter.ExecuteScript(methodClone.Content, originalMethod, context, ExecuteScriptContextCollection.Method);
        }
        public static dynamic ExecuteConstructor(Method methodClone, FindContext context)
        {
            context = context.CloneFindContext();

            if (methodClone.ParentObject is Class objClass)
            {
                var newObj = objClass.CloneObject();

                context.MonoType = newObj;
                context.ScriptFile = context.MonoType?.ParentObject as ScriptFile;

                MonoInterpreter.ExecuteScript(methodClone.Content, methodClone, context, ExecuteScriptContextCollection.Method);

                return newObj;
            }
            
            if (methodClone.ParentObject is Struct objStruct)
            {
                var newObj = objStruct.CloneObject();

                context.MonoType = newObj;
                context.ScriptFile = context.MonoType?.ParentObject as ScriptFile;

                MonoInterpreter.ExecuteScript(methodClone.Content, methodClone, context, ExecuteScriptContextCollection.Method);

                return newObj;
            }

            return null;
        }
    }
}
