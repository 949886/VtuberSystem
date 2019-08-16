//
//  LiveMessageNotificationManager.cs
//  VupSystem
//
//  Created by LunarEclipse on 2019-08-08 08:39:47.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using Sora.Core.Event;

/// <summary>
/// Live message dispatcher.
/// </summary>
public class LiveMessageNotificationManager
{
    #region Singleton

    public static LiveMessageNotificationManager Instance
    {
        get
        {
            if (instance == null)
                instance = new LiveMessageNotificationManager();
            return instance;
        }
    }

    private static LiveMessageNotificationManager instance;

    #endregion

    #region Property
    
    /// <summary>
    /// Bind a live connection by assigning this property.
    /// Unbind live connection by setting this property to null.
    /// </summary>
    public ILiveConnection Connection
    {
        get { return connection?.Target as ILiveConnection; }
        set
        {
            if (connection == null ||
                connection?.Target != value)
            {
                var newConnection = value;
                var oldConnection = connection?.Target as ILiveConnection;
                newConnection?.RegisterMessageHandler(DanmakuMessageHandler);
                oldConnection?.UnregisterMessageHandler(DanmakuMessageHandler);
                connection = new WeakReference(value);
            }
        }
    }
    private WeakReference connection = null;

    #endregion
    

    private void DanmakuMessageHandler(object sender, LiveMessage message)
    {
        try
        {
            // Dispatch live message with event bus.
            EventBus.Default.Post(message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
    
}