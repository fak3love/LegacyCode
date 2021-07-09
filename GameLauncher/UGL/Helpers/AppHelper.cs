using System;
using System.Collections.Generic;
using System.Text;

namespace UGL.Helpers
{
    public static class AppHelper
    {
        public static string AppFolder => AppDomain.CurrentDomain.BaseDirectory.Remove(AppDomain.CurrentDomain.BaseDirectory.Length - @"\bin\Debug\netcoreapp3.1".Length);
    }
}
