using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Libraries
{
    public class AllowedNameAttribute : Attribute
    {
        public string[] Names { get; }
        public bool UseHowExtension { get; set; } = true;

        public AllowedNameAttribute(params string[] names)
        {
            Names = names;
        }
    }
}
