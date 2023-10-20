//
//  LiveManagerr.cs
//  VtuberSystem
//
//  Created by LunarEclipse on 2019-08-10 23:29:52.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using UnityEngine;
using Bilibili.Live;

public class LiveManager : MonoBehaviour
{
    [SerializeField] public int RoomID; 

    private ILiveConnection connection = new BilibiliLiveConnection();

    void Start()
    {
        // Connect to live room.
        if (RoomID >= 1000)
            connection.Connect(RoomID);
        else Debug.Log($"Room not exists.");

        // Register live message dispatcher.
        LiveMessageNotificationManager.Instance.Connection = connection;
    }

    private void OnDestroy()
    {
        connection.Disconnect();
    }
}