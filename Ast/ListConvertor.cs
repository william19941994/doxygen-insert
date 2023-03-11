using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DoxygenInsert.Ast
{
    public class ListConverter : CollectionConverter
    {
        private class ListPropertyDescriptor : SimplePropertyDescriptor
        {
            private int index;

            public ListPropertyDescriptor(Type lstType, Type elementType, int index,int padding)
                : base(lstType, "[" + index.ToString().PadLeft(padding, ' ') + "]", elementType, null)
            {
                this.index = index;
            }

            public override object GetValue(object instance)
            {
                if (instance is IList)
                {
                    IList list = (IList)instance;
                    if (list.Count > index)
                    {
                        return list[index];
                    }
                }

                return null;
            }

            public override void SetValue(object instance, object value)
            {
                if (instance is IList)
                {
                    IList list = (IList)instance;
                    if (list.Count > index)
                    {
                        list[index]=value;
                    }

                    OnValueChanged(instance, EventArgs.Empty);
                }
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(string) && value is IList)
            {
                return value.GetType().Name;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptor[] lst = null;
            //if (value.GetType().IsArray)
            if(value.GetType().IsGenericType)
            {
                IList array2 = (IList)value;
                int length = array2.Count;
                lst = new PropertyDescriptor[length];
                Type type = value.GetType();
                Type elementType = type.GetGenericArguments()[0];
                int padding = length.ToString().Length;
                for (int i = 0; i < length; i++)
                {
                    lst[i] = new ListPropertyDescriptor(type, elementType, i,padding);
                }
            }

            return new PropertyDescriptorCollection(lst);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
