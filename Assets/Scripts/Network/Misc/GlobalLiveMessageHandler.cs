//
//  GlobalLiveMessageHandler.cs
//  VtuberSystem
//
//  Created by LunarEclipse on 2019-08-11 04:47:54.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using UnityEngine;
using Bilibili.Live;
using Sora.Core.Event;

public class GlobalLiveMessageHandler : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"Register to event bus.");
        EventBus.Default.Register(this);
    }
    
    void OnDestroy()
    {
        Debug.Log($"Unregister from event bus.");
        EventBus.Default.Unregister(this);
    }


    [Subscribe]
    private void HandleEvent(DanmakuMessage danmaku)
    {
        Debug.Log($"[Danmaku received] {danmaku.Username}: {danmaku.Comment}");
    }

    [Subscribe]
    private void HandleEvent(GiftMessage gift)
    {
        Debug.Log($"[Gift received] {gift.Username}: {gift.Count} * {gift.Name}");
    }
}
