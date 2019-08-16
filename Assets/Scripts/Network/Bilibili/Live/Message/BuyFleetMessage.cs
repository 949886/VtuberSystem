//
//  BuyFleetMessage.cs
//  VupSystem
//
//  Created by LunarEclipse on 2019-08-10 23:46:42.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

namespace Bilibili.Live
{
    public class BuyFleetMessage: LiveMessage
    {
        public override Type MessageType => Type.BuyFleet;
    }
}