//
//  DanmakuMessage.cs
//  VtuberSystem
//
//  Created by LunarEclipse on 2019-08-08 09:24:33.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Bilibili.Live
{
    public class DanmakuMessage : LiveMessage
    {
        public override Type MessageType => Type.Danmaku;

        public string Comment { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsVIP { get; set; }
        public int FleetLevel { get; set; }


        [JsonExtensionData] private IDictionary<string, JToken> additionalData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            var info = (JArray)additionalData["info"];
;
            Comment = info[1].ToString();
            UserID = info[2][0].ToObject<int>();
            Username = info[2][1].ToString();
            IsAdmin = info[2][2].ToString() == "1";
            IsVIP = info[2][3].ToString() == "1";
            FleetLevel = info[7].ToObject<int>();
        }
    }
}