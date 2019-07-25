using System;
using System.IO;
using System.Xml.Serialization;

namespace AuxLib.Serialization
{
    public static class XMLFileManager<T> where T : class
    {
        public static void WriteToFile(String filename, T obj)
        {
            try
            {
                var fs = new FileStream(filename, FileMode.Create);
                var xs = new XmlSerializer(obj.GetType());
                xs.Serialize(fs, obj);
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + "File : " + filename + " Errormessage : " + ex.Message, "Error while writing ");
                throw new Exception();
            }
        }

        public static T OpenFromFile(String filename)
        {
            try
            {
                var xs = new XmlSerializer(typeof(T));
                var fs = new FileStream(filename, FileMode.Open);
                try
                {
                    return (T)xs.Deserialize(fs);
                }
                finally
                {
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + "File : " + filename + " Errormessage : " + ex.Message, "Error while reading ");
                throw new Exception();
            }
        }
    }
}
