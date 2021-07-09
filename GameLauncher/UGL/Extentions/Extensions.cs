using System;
using System.Collections.Generic;
using System.Text;
using UGL.Helpers;

namespace UGL.Extentions
{
    public static class Extensions
    {
        public static Uri ToAbsolute(this Uri relativeUri)
        {
            return new Uri(AppHelper.AppFolder + relativeUri.OriginalString);
        }
    }
}
