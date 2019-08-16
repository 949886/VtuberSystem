//
//  DictionaryExtension.cs
//  Sora
//
//  Created by LunarEclipse on 2017-10-23.
//  Copyright © 2017 LunarEclipse. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Sora.Extensions
{
    public static class DictionaryExtension
    {
        public static Value Get<Key, Value>(this Dictionary<Key, Value> dict, Key key)
        {
            if (dict == null || !dict.ContainsKey(key))
                return default(Value);
            return dict[key];
        }
    }
}
