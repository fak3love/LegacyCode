using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace UGL.Helpers
{
    public static class ResourceHelper
    {
        public static Color FindColorByName(string name)
        {
            return (Color)Application.Current.FindResource(name + "Color");
        }

        public static Style FindListBoxStyleByName(string name)
        {
            return (Style)Application.Current.FindResource("listBoxStyle" + name);
        }
    }
}
