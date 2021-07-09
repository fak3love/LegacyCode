using MonoScript.Analytics;
using MonoScript.Collections;
using MonoScript.Libraries;
using MonoScript.Libraries.IO;
using MonoScript.Models.Application;
using MonoScript.Models.Contexts;
using MonoScript.Runtime;
using System;
using System.Collections.Generic;

namespace MSConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Application application = new Application(BehaviorModeCollection.ThrowException);
            application.RunApplication(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\test.txt");

            Console.ReadKey();
        }
    }
}
