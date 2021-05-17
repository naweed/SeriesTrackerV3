using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters;

namespace XGENO.Framework.Helpers
{
    public static class SerializationHelper
    {
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);

            if (typeof(T).Name == "Boolean")
            {
                json = json.ToLower();

            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Serialize<T>(T item)
        {
            if (item == null)
                return null;

            if (item as string == string.Empty)
                return string.Empty;


            return JsonConvert.SerializeObject(item, Formatting.None, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                //PreserveReferencesHandling = PreserveReferencesHandling.All,
                //ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //ObjectCreationHandling = ObjectCreationHandling.Auto,
                //ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });
        }

    }
}
