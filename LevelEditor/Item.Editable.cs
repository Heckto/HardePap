using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using CustomUITypeEditors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Forms = System.Windows.Forms;
using System.Windows.Forms;

namespace LevelEditor
{
    //public class ItemTypeConverter : TypeConverter
    //{
    //    public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
    //    {
    //        if (destinationType == typeof(Item)) return true;

    //        return base.CanConvertTo(context, destinationType);
    //    }

    //    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
    //    {
    //        if (destinationType == typeof(string) && value is Item)
    //        {
    //            var result = (Item)value;
    //            return result.Name;
    //        }
    //        return base.ConvertTo(context, culture, value, destinationType);
    //    }

    //}
}
