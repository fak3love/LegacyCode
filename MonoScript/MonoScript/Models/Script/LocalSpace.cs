using MonoScript.Analytics;
using MonoScript.Models.Analytics;
using MonoScript.Script.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoScript.Models.Script
{
    public class LocalSpace
    {
        public int FreeMonoSysValue { get => GetMaxMonoSysValue() + 1; }
        public LocalSpace ParentSpace { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
        public LocalSpace(LocalSpace parentSpace) => ParentSpace = parentSpace;

        public Field Find(string name)
        {
            var field = Fields.FirstOrDefault(var => var.Name == name);

            if (field == null && ParentSpace != null)
                return ParentSpace.Find(name);

            return field;
        }
        public List<Field> FindStartsWith(string value)
        {
            var fields = Fields.Where(var => var.Name.StartsWith(value)).ToList();

            if (ParentSpace != null)
                fields.AddRange(ParentSpace.FindStartsWith(value));

            return fields;
        }

        public bool Add(Field field)
        {
            if (Find(field.Name) != null)
            {
                MLog.AppErrors.Add(new AppMessage("A variable with the same name has already been found.", $"Object {field.FullPath}"));
                return false;
            }

            Fields.Add(field);

            return true;
        }
        public bool Remove(string name)
        {
            var field = Fields.FirstOrDefault(var => var.Name == name);

            if (field == null && ParentSpace != null)
                return ParentSpace.Remove(name);

            if (field != null)
                return Fields.Remove(field);

            return false;
        }
        public int GetMaxMonoSysValue()
        {
            var fields = FindStartsWith("monosys_");
            int maxValue = 0;

            foreach (var field in fields)
            {
                int currentValue = int.Parse(field.Name.Substring(field.Name.IndexOf("_") + 1));

                if (currentValue > maxValue)
                    maxValue = currentValue;
            }

            return maxValue;
        }

        public static LocalSpace LoadFromMethod(Method method)
        {
            LocalSpace localSpace = new LocalSpace(null);

            foreach (var field in method.Parameters)
                localSpace.Add(field);

            return localSpace;
        }
    }
}
