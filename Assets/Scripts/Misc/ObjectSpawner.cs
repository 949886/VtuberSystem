//
//  ObjectSpawner.cs
//  VupSystem
//
//  Created by LunarEclipse on 2019-08-11 05:23:53.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using Bilibili.Live;
using Sora.Core.Dispatch;
using Sora.Core.Event;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectSpawner : MonoBehaviour
{
    public Rigidbody Object;

    void Start()
    {
        EventBus.Default.Register(this);

        DispatchQueue.Main.AsyncAfter(1000, () => {
            Debug.Log($"[AsyncAfter] 1s " + Time.time);
        });
        DispatchQueue.Main.AsyncAfter(2000, () => {
            Debug.Log($"[AsyncAfter] 2s " + Time.time);
        });
        DispatchQueue.Main.AsyncAfter(3000, () => {
            Debug.Log($"[AsyncAfter] 3s " + Time.time);
        });
    }

    void OnDestory()
    {
        EventBus.Default.Unregister(this);
    }

    [Subscribe(ThreadMode.MAIN)]
    public void LiveGiftHandler(GiftMessage gift)
    {
        try
        {
            for (int i = 0; i < gift.Count; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-2f, 2f), Random.Range(1, 10), 0);
                Rigidbody newObject = Instantiate(Object, transform.position + offset, transform.rotation) as Rigidbody;
                Destroy(newObject.gameObject, 50);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

}
