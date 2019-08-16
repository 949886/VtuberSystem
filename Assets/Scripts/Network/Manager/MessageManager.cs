//
//  MessageManager.cs
//  VupSystem
//
//  Created by LunarEclipse on 2019-08-09 02:22:48.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Bilibili.Live;
using UnityEngine;

namespace Bilibili.Live
{
    public class MessageManager
    {

        private static MessageManager instance;

        private Queue<LiveMessage> messageQueue = new Queue<LiveMessage>();

        public static MessageManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MessageManager();
                return instance;
            }
        }

        public void Enqueue(LiveMessage newLiveMessage)
        {
            lock (this)
            {
#if DEBUG

#else

#endif
                messageQueue.Enqueue(newLiveMessage);
            }
        }

        public LiveMessage Dequeue()
        {
            lock (this)
            {
                try
                {
                    var Message = messageQueue.Dequeue();
                    return Message;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }
        }

        public LiveMessage[] DequeueAll()
        {
            if (messageQueue.Count == 0)
                return null;

            LiveMessage[] liveMessages = messageQueue.ToArray();
            messageQueue.Clear();

            return liveMessages;
        }

        private void MessageThreadExec()
        {
            while (true)
            {
                Thread.Sleep(20);
            }
        }


    }
}
