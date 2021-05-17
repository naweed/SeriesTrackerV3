using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGENO.Framework.DataModels
{
    public class StateItems : Dictionary<string, object>
    {
        public T Get<T>(string key)
        {
            object tryGetValue;

            if (TryGetValue(key, out tryGetValue))
            {
                return (T)tryGetValue;
            }

            throw new KeyNotFoundException();
        }

        public bool TryGet<T>(string key, out T value)
        {
            object tryGetValue;
            bool success = false;

            if (success = TryGetValue(key, out tryGetValue))
            {
                value = (T)tryGetValue;
            }
            else
            {
                value = default(T);
            }

            return success;
        }
    }}
