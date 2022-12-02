using System;
using System.Collections.Generic;

namespace CSLOXProj
{
    public class HashMap<K, V> : Dictionary<K, V>
    {
        public V Get(K key)
        {
            if (TryGetValue(key, out V value))
            {
                return value;
            }
            Console.WriteLine("Undef " + key);
            return default;

        }

        public V Put(K key, V value)
        {
            V old_value = default;

            //if (TryGetValue(key, out V thing))

            if (ContainsKey(key))
            {
                Console.WriteLine("YOU SHOULD NEVER COME IN HERE");
                old_value = this[key];
                this[key] = value;

            }
            else
            {
                Add(key, value);
            }

            return old_value;
        }
    }

}
