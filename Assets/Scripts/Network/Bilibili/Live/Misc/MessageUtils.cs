//
//  MessageUtils.cs
//
//  Created by LunarEclipse on 8/9/2019 3:11:07 AM.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using BilibiliDM_PluginFramework;
using Sora.Utils;

namespace Bilibili.Live
{
    class MessageUtil
    {
        public static LiveMessage Parse(DanmakuModel model)
        {
            var json = model.RawDataJToken;
            var cmd = json["cmd"].ToString();
            var type = (LiveMessage.Type)EnumUtils.FromDescription(typeof(LiveMessage.Type), cmd);

            LiveMessage liveMessage = null;
            switch (type)
            {
//                case LiveMessage.Type.Danmaku: liveMessage = json.ToObject<DanmakuMessage>(); break;
                case LiveMessage.Type.Gift: liveMessage = json["data"].ToObject<GiftMessage>(); break;
                default: break;
            }

            return liveMessage;
        }
    }
}
