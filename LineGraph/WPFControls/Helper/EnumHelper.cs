using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WPFControls
{
    public static class EnumHelper
    {
        public static T FindEnumValue<T>(this T value, int index)
        {
            return (T)Enum.ToObject(typeof(T), index);
        }

        public static T FindEnumValue<T>(this T value, string str)
        {
            string[] enums = Enum.GetNames(typeof(T));

            T result = (T)Enum.ToObject(typeof(T), 0);

            for (int i = 0; i < enums.Length; i++)
            {
                if (str == enums[i])
                    result = (T)Enum.ToObject(typeof(T), i);
            }

            return result;
        }

        public static string ToDescription<T>(this T value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
                return value.ToString();
            else
            {
                DescriptionAttribute attrib = null;

                foreach (var att in attribArray)
                {
                    if (att is DescriptionAttribute)
                        attrib = att as DescriptionAttribute;
                }

                if (attrib != null)
                    return attrib.Description;

                return value.ToString();
            }
        }
    }
}
