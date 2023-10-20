//
//  BilibiliLiveConnection.cs
//  VtuberSystem
//
//  Created by LunarEclipse on 2019-08-08 11:17:05.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using BilibiliDM_PluginFramework;
using BiliDMLib;
using Debug = UnityEngine.Debug;

namespace Bilibili.Live
{
    public class BilibiliLiveConnection : ILiveConnection
    {
        private readonly DanmakuLoader loader = new DanmakuLoader();
        private event EventHandler<LiveMessage> danmakuHandler;

        private bool isConnected = false;

        private int roomID;
        public int RoomID
        {
            get { return roomID; }
            set
            {
                // Reconnect after resetting room id.
                if (value != roomID && isConnected)
                {
                    roomID = value;
                    Disconnect();
                    Connect(roomID);
                }
            }
        }

        public BilibiliLiveConnection()
        {
            loader.ReceivedDanmaku += ReceiveDanmakuHandler;
            loader.ReceivedRoomCount += ReceiveRoomInfoHandler;
            loader.Disconnected += DisconnectedHandler;
            loader.LogMessage += LogMessageHandler;
        }

        public BilibiliLiveConnection(int roomID) : this()
        {
            this.roomID = roomID;
        }

        #region Public

        public async void Connect(int roomID)
        {
            if (roomID > 0)
            {
                var connectresult = false;
                var trytime = 0;

                Debug.Log($"正在连接...");

                connectresult = await loader.ConnectAsync(roomID);

                if (!connectresult && loader.Error != null)// 如果连接不成功并且出错了
                    Debug.Log($"连接失败...");

                while (!connectresult)
                {
                    if (trytime > 5) break;
                    else trytime++;

                    await Task.Delay(1000); // 稍等一下
                    Debug.Log($"重新连接...");
                    connectresult = await loader.ConnectAsync(roomID);
                }

                if (connectresult)
                {
                    Debug.Log($"连接成功");
                    //Ranking.Clear();
                    //SaveRoomId(roomID);
                    this.isConnected = true;
                }
                else Debug.Log($"连接失败...");
            }
            else Debug.Log($"非法ID");
        }

        public void Disconnect()
        {
            loader.Disconnect();
            this.isConnected = false;

            Debug.Log($"断开连接...");
        }

        public void RegisterMessageHandler(EventHandler<LiveMessage> handler)
        {
            danmakuHandler += handler;
        }

        public void UnregisterMessageHandler(EventHandler<LiveMessage> handler)
        {
            danmakuHandler -= handler;
        }

        #endregion

        #region Private

        private void ReceiveDanmakuHandler(object sender, ReceivedDanmakuArgs args)
        {
            Debug.Log($@"
Danmaku received 
MsgType: {args.Danmaku.MsgType}
UserName: {args.Danmaku.UserName}
CommentText: {args.Danmaku.CommentText}
");
            var message = MessageUtil.Parse(args.Danmaku);
            if (message != null)
                danmakuHandler?.Invoke(sender, message);

            //switch (args.Danmaku.MsgType)
            //{
            //    case MsgType.Comment:
            //    case MsgType.GiftSend:
            //    case MsgType.GiftTop:
            //    case MsgType.Welcome:
            //    case MsgType.LiveStart:
            //    case MsgType.LiveEnd:
            //    case MsgType.Unknown:
            //    case MsgType.WelcomeGuard:
            //    case MsgType.GuardBuy:
            //    default: break; ;
            //}
        }

        private void ReceiveRoomInfoHandler(object sender, ReceivedRoomCountArgs args)
        {

        }

        private void DisconnectedHandler(object sender, DisconnectEvtArgs args)
        {
            Debug.Log($"連接被斷開...");

            if (args.Error != null)
            {
                Debug.Log($"正在自动重连...");
                Connect(roomID);
            }
        }

        private void LogMessageHandler(object sender, LogMessageArgs args)
        {

        }

        #endregion
        
    }

}
