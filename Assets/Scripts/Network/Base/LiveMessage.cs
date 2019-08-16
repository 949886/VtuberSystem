//
//  LiveMessage.cs
//
//  Created by LunarEclipse on 8/8/2019 11:56:03 PM.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using System.ComponentModel;


public abstract class LiveMessage
{

    public abstract Type MessageType { get; }
    public enum Type
    {
        [Description("UNKNOW")] Unknow,
        [Description("DANMU_MSG")] Danmaku,
        [Description("LIVE")] LiveStart,
        [Description("PREPARING")] LiveEnd,
        [Description("SEND_GIFT")] Gift,
        [Description("GIFT_TOP")] GiftTop,
        [Description("COMBO_END")] ComboEnd,
        [Description("WELCOME")] Welcome,
        [Description("WELCOME_GUARD")] WelcomeFleet,
        [Description("GUARD_BUY")] BuyFleet,
        [Description("ENTRY_EFFECT")] EntryEffect,
        [Description("ROOM_REAL_TIME_MESSAGE_UPDATE")] RoomInfoUpdate,
        [Description("WEEK_STAR_CLOCK")] WeekStar
    }

}
