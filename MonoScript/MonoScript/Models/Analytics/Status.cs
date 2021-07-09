using MonoScript.Analytics;
using MonoScript.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Analytics
{
    public class Status
    {
        public string Message { get; }
        public bool Success { get; set; }
        public dynamic ExecuteResult { get; set; }

        public Status(string message, bool success, dynamic executeResult = null)
        {
            Message = message;
            Success = success;
            ExecuteResult = executeResult;
        }

        public static Status ErrorBuild { get { return new Status("Build error.", false); } }
        public static Status ErrorCompleted { get { return new Status("Errors occurred during program execution.", false); } }
        public static Status SuccessBuild { get { return new Status("Success build.", true); } }
        public static Status SuccessfullyCompleted(dynamic executeResult) => new Status("Successfully completed.", true, executeResult);
    }
}
