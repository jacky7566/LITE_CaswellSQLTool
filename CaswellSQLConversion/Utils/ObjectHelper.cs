using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaswellSQLConversion.Utils
{
    static class ObjectHelper
    {
        public static T Clone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            var serializeSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializeSettings), deserializeSettings);
        }

        public static Double Val(string value)
        {
            String result = String.Empty;
            foreach (char c in value)
            {
                if (Char.IsNumber(c) || (c.Equals('.') && result.Count(x => x.Equals('.')) == 0))
                    result += c;
                else if (!c.Equals(' '))
                    return String.IsNullOrEmpty(result) ? 0 : Convert.ToDouble(result);
            }
            return String.IsNullOrEmpty(result) ? 0 : Convert.ToDouble(result);
        }

        public static DateTime UK2USDate(string ukDate, string ukTime)
        {
            var dd = ukDate.Substring(0, 2);
            var mm = ukDate.Substring(3, 2);
            var yyyy = "20" + ukDate.Substring(6, 2);
            var usDate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", mm, dd, yyyy, ukTime));
            return usDate;
        }

        public static IEnumerable<double> Range(double min, double max, double step)
        {
            double result = min;
            for (int i = 0; result < max; i++)
            {
                result = min + (step * i);
                yield return result;
            }
        }
    }
}
