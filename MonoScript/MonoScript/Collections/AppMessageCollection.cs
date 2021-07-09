
using MonoScript.Libraries.IO;
using MonoScript.Models.Analytics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Collections
{
    public class AppMessageCollection : List<AppMessage>
    {
        public AppMessageOptions AppMessageOptions { get; set; } = new AppMessageOptions();
        public new void Add(AppMessage message)
        {
            if (AppMessageOptions.ThrowException)
                throw new Exception(message.Message) { Source = message.Source };

            base.Add(message);

            MonoConsole.WriteAppMessage(message);
        }
    }
}
