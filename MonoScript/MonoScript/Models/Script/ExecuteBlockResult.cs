using MonoScript.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Script
{
    public class ExecuteBlockResult
    {
        public dynamic ObjectResult { get; set; }
        public ExecuteResultCollection ResultType { get; set; }
        public uint Count { get; set; }

        public bool CanExecuteNextResult(ExecuteScriptContextCollection executeScriptContext)
        {
            if (ResultType == ExecuteResultCollection.None)
                return false;

            if (ResultType == ExecuteResultCollection.Return)
                return true;

            if (ResultType == ExecuteResultCollection.Continue)
                return true;

            return Count > 0 && executeScriptContext != ExecuteScriptContextCollection.Method;
        }

        public ExecuteBlockResult ExecuteNextResult(ExecuteScriptContextCollection executeScriptContext)
        {
            if (ResultType == ExecuteResultCollection.Return)
                return this;

            if ((executeScriptContext == ExecuteScriptContextCollection.CycleOrSwitch && ResultType == ExecuteResultCollection.Break) || (executeScriptContext == ExecuteScriptContextCollection.IfElseSwitch && ResultType == ExecuteResultCollection.Quit))
                Count--;

            return this;
        }
    }
}
