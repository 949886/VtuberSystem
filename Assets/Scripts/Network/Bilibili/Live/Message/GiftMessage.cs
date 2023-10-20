//
//  GiftMessage.cs
//  VtuberSystem
//
//  Created by LunarEclipse on 2019-08-08 23:55:23.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bilibili.Live
{
    //[JsonConverter(typeof(JsonPathConverter))]
    public class GiftMessage : LiveMessage
    {
        public override Type MessageType => Type.Gift;

        /* Gift Info */

        [JsonProperty("giftId")] public int ID { get; set; }
        [JsonProperty("giftName")] public string Name { get; set; }
        [JsonProperty("giftType")] public int GiftType { get; set; }
        [JsonProperty("num")] public int Count { get; set; }
        [JsonProperty("price")] public int Price { get; set; }  //单位：瓜子
        [JsonProperty("timestamp")] public double Timestamp { get; set; }
        [JsonProperty("action")] public string Action { get; set; }
        [JsonProperty("super")] public int IsSuper { get; set; }
        [JsonProperty("super_gift_num")] public int SuperGiftCount { get; set; }
        [JsonProperty("metadata")] public string Metadata { get; set; }
        [JsonProperty("combo_send")] public Combo combo { get; set; }

        /* Sender Info */

        [JsonProperty("uid")] public int UserID { get; set; }
        [JsonProperty("uname")] public string Username { get; set; }
        [JsonProperty("face")] public string Avatar { get; set; }
        [JsonProperty("guard_level")] public int FleetLevel { get; set; }

        /* Receiver Info */

        [JsonProperty("rcost")] public int RoomGiftPoint { get; set; }
        [JsonProperty("rnd")] public string Rnd { get; set; }

        /* Misc */
        //[JsonProperty("rnd")] public List<string> TopList { get; set; }

        
        public class Combo
        {
            [JsonProperty("combo_id")] public string ID { get; set; }
            [JsonProperty("combo_num")] public int ComboCount { get; set; }
            [JsonProperty("gift_id")] public int GiftID { get; set; }
            [JsonProperty("gift_name")] public string GiftName { get; set; }
            [JsonProperty("action")] public string Action { get; set; }
            [JsonProperty("uid")] public int UserID { get; set; }
            [JsonProperty("uname")] public string Username { get; set; }
        }
    }
}