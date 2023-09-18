using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TickBased.Utils
{
    public static class DataUtils
    {
        public static T DeepCopy<T>(T source)
        {
            if (source == null)
                return default(T);

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}