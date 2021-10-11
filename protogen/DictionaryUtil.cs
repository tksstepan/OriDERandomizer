using System;
using System.Collections.Generic;

namespace Protogen
{
    public static class DictionaryUtil
    {
        public static TValue GetOrPut<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> value)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = value.Invoke();
            }

            return dict[key];
        }
    }
}