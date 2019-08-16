//
//  LiveGift.cs
//  VupSystem
//
//  Created by LunarEclipse on 2019-08-08 10:06:05.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

public class LiveGift
{
    /* Gift Info */
    public Type type;
    public string name;
    public int price;
    public double startTime;
    public double endTime;

    /* Sender Info */
    public int suid;
    public string senderUsername;
    public string senderFace;

    /* Receiver Info */
    public int ruid;
    public string receiverUsername;

    public enum Type: int
    {
        Unknow = 0,

        辣条 = 1,         // 100
        B坷垃 = 3,          // 1000
        亿元 = 6,          // 1000
        冰阔落 = 20008, //1000
        千纸鹤 = 30274,  // 100
        打榜 = 30046, //2000
        友谊的小船 = 30135, // 5200
        礼花 = 30064,   //2800
        小星星 = 30085,
        么么哒 = 30090,
        铃铛 = 30135,
        御守 = 30136,   //1000
        棋开得胜 = 30153, //1000
        激爽刨冰 = 30275,
        恋恋手账 = 30276,
    }
}