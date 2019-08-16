using System;
using Bilibili.Live;

public interface ILiveConnection
{
    void Connect(int roomID);
    void Disconnect();
    void RegisterMessageHandler(EventHandler<LiveMessage> handler);
    void UnregisterMessageHandler(EventHandler<LiveMessage> handler);
}