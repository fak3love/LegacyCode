using MonoScript.Analytics;
using MonoScript.Collections;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Models.Contexts;
using MonoScript.Models.Exts;
using MonoScript.Models.Interpreter;
using MonoScript.Models.Script;
using MonoScript.Script.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoScript.Runtime
{
    public static class HelperExpressions
    {
        public static List<string> GetStringMethodParameters(string expression, ref int index)
        {
            string tmpex = string.Empty;
            List<string> values = new List<string>();

            int bracketRoundCount = 0;
            int bracketSquareCount = 0;
            InsideQuoteModel quoteModel = new InsideQuoteModel();

            for (; index < expression.Length; index++)
            {
                Extensions.IsOpenQuote(expression, index, ref quoteModel);

                if (expression[index].Contains("(["))
                {
                    if (quoteModel.HasQuotes || bracketRoundCount + bracketSquareCount > 0)
                        tmpex += expression[index];
                }
                else if (expression[index].Contains(")]"))
                {
                    if (quoteModel.HasQuotes || bracketRoundCount + bracketSquareCount > 1)
                        tmpex += expression[index];
                }
                else if (expression[index].Contains(","))
                {
                    if (quoteModel.HasQuotes || bracketRoundCount + bracketSquareCount > 1)
                        tmpex += expression[index];
                }
                else
                    tmpex += expression[index];

                if (!quoteModel.HasQuotes)
                {
                    if (expression[index] == '[')
                        bracketSquareCount++;

                    else if (expression[index] == ']')
                        bracketSquareCount--;

                    else if (expression[index] == '(')
                        bracketRoundCount++;

                    else if (expression[index] == ')')
                        bracketRoundCount--;

                    if (bracketRoundCount + bracketRoundCount == 0 && expression[index].Contains(")]"))
                        break;

                    if (bracketRoundCount + bracketSquareCount == 1 && expression[index] == ',')
                    {
                        values.Add(tmpex);
                        tmpex = string.Empty;
                        continue;
                    }
                }
            }

            if (bracketRoundCount == 0 && bracketSquareCount == 0)
            {
                if (!string.IsNullOrWhiteSpace(tmpex))
                    values.Add(tmpex);
            }

            return values;
        }
        public static List<(string Name, dynamic Value)> GetObjectMethodParameters(List<string> stringValues, FindContext context)
        {
            List<(string, dynamic)> methodInputs = new List<(string, dynamic)>();
            bool hasFieldName = false, hasAllowFieldName = true;

            for (int index = 0; index < stringValues.Count; index++)
            {
                if (index == 0)
                {
                    int indexOf = -1;
                    for (int i = 0; i < stringValues[0].Length; i++)
                    {
                        if (stringValues[0][i] == '=' && i + 1 < stringValues[0].Length && stringValues[0][i + 1] != '=' && i - 1 >= 0 && !stringValues[0][i - 1].Contains(ReservedCollection.EqualityChars))
                        {
                            indexOf = i;
                            break;
                        }

                        if (!stringValues[0][i].Contains(ReservedCollection.AllowedNames + ReservedCollection.WhiteSpace))
                            hasAllowFieldName = false;
                    }

                    if (indexOf != -1 && hasAllowFieldName)
                        hasFieldName = true;

                    if (hasFieldName)
                    {
                        string[] splitValues = stringValues[0].Split('=', 2);
                        string objName = splitValues[0].Trim(' ');

                        methodInputs.Add((objName, MonoInterpreter.ExecuteExpression(splitValues[1], context)));
                        continue;
                    }
                }

                if (hasFieldName)
                {
                    int indexOf = stringValues[index].IndexOf("=");

                    if (indexOf != -1 && !Extensions.InsideQuotes(stringValues[index], indexOf).HasQuotes)
                    {
                        string[] splitValues = stringValues[index].Split('=', 2);
                        string objName = splitValues[0].Trim(' ');

                        methodInputs.Add((objName, MonoInterpreter.ExecuteExpression(splitValues[1], context)));
                    }
                    else MLog.AppErrors.Add(new AppMessage("No assignment operator found for method object.", stringValues[index]));
                }
                else
                    methodInputs.Add((null, MonoInterpreter.ExecuteExpression(stringValues[index], context)));
            }

            return methodInputs;
        }
        public static void SkipIfElseRemaining(ref int index, string expression)
        {
            FirstIndex first = Extensions.FindFirstIndex(expression, index, "e", ReservedCollection.WhiteSpace);

            if (first.IsFirst)
            {
                if (first.Position + 3 < expression.Length && expression[first.Position + 1] == 'l' && expression[first.Position + 2] == 's' && expression[first.Position + 3] == 'e')
                {
                    index = first.Position + 4;
                    first = Extensions.FindFirstIndex(expression, index, "i{", ReservedCollection.WhiteSpace);

                    if (first.IsFirst)
                    {
                        if (first.FirstChar == 'i')
                        {
                            if (first.Position + 1 < expression.Length && expression[first.Position + 1] == 'f')
                            {
                                index = first.Position + 2;

                                first = Extensions.FindFirstIndex(expression, index, "(", ReservedCollection.WhiteSpace);

                                if (first.IsFirst)
                                {
                                    int bracketRoundCount = 1, bracketSquareCount = 0;
                                    string equalityExpression = string.Empty;
                                    InsideQuoteModel quoteModel = new InsideQuoteModel();

                                    for (index = first.Position + 1; index < expression.Length; index++)
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

                                    if (string.IsNullOrWhiteSpace(equalityExpression))
                                        MLog.AppErrors.Add(new AppMessage("The conditional expression inside If cannot be empty.", expression));

                                    first = Extensions.FindFirstIndex(expression, index, "{", ReservedCollection.WhiteSpace);

                                    if (first.IsFirst)
                                    {
                                        int bracketCount = 1;
                                        index = first.Position + 1;

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

                                        index++;
                                        SkipIfElseRemaining(ref index, expression);
                                    }
                                    else
                                        MLog.AppErrors.Add(new AppMessage("The if statement must be followed by a curly brace.", expression));
                                }
                                else
                                    MLog.AppErrors.Add(new AppMessage("Incorrect if statement body.", expression));
                            }
                            else
                                MLog.AppErrors.Add(new AppMessage("The else statement must be followed by its body or the declaration of the if statement.", expression));
                        }
                        else
                        {
                            int bracketCount = 1;
                            index = first.Position + 1;

                            InsideQuoteModel quoteModel = new InsideQuoteModel();

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
                        }
                    }
                    else
                        MLog.AppErrors.Add(new AppMessage("The else statement must be followed by its body or the declaration of the if statement.", expression));
                }
            }
        }
    }
}
