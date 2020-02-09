using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelEditor
{
    public class ValueTypeTypeConverter : System.ComponentModel.ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return base.GetProperties(context, value, attributes);
        }

        public override bool GetCreateInstanceSupported(System.ComponentModel.ITypeDescriptorContext context)
        {
            return true;
        }

        public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
        {
            if (propertyValues == null)
                throw new ArgumentNullException("propertyValues");

            var boxed = Activator.CreateInstance(context.PropertyDescriptor.PropertyType);
            foreach (System.Collections.DictionaryEntry entry in propertyValues)
            {
                var pi = context.PropertyDescriptor.PropertyType.GetProperty(entry.Key.ToString());
                if ((pi != null) && (pi.CanWrite))
                {
                    pi.SetValue(boxed, Convert.ChangeType(entry.Value, pi.PropertyType), null);
                }
            }
            return boxed;
        }
    }
}
