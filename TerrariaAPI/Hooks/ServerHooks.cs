﻿using System;
using System.Text;
using Terraria;

namespace TerrariaAPI.Hooks
{
    public static class ServerHooks
    {
        static ServerHooks()
        {
            NetHooks.OnPreGetData += NetHooks_OnPreGetData;
        }

        static void NetHooks_OnPreGetData(GetDataEventArgs e)
        {
            if (e.MsgID == 0x1)
            {
                e.Handled = !Join(e.Msg.whoAmI);
                if (e.Handled)
                    Netplay.serverSock[e.Msg.whoAmI].kill = true;
            }
            else if (e.MsgID == 0x19)
            {
                string str = Encoding.ASCII.GetString(e.Msg.readBuffer, e.Index + 0x4, e.Length - 0x5);
                e.Handled = Chat(e.Msg.whoAmI, str);
            }
        }

        /// <summary>
        /// arg1 = WhoAmI
        /// </summary>
        public static event Action<int, AllowEventArgs> OnJoin;

        public static bool Join(int whoami)
        {
            var args = new AllowEventArgs();
            if (OnJoin != null)
                OnJoin(whoami, args);
            return args.Allow;
        }

        /// <summary>
        /// arg1 = WhoAmI
        /// </summary>
        public static event Action<int> OnLeave;

        public static void Leave(int whoami)
        {
            if (OnLeave != null)
                OnLeave(whoami);
        }

        /// <summary>
        /// arg1 = WhoAmI, arg2 = Message
        /// </summary>
        public static event Action<int, string, HandledEventArgs> OnChat;

        public static bool Chat(int whoami, string msg)
        {
            var args = new HandledEventArgs();
            if (OnChat != null)
                OnChat(whoami, msg, args);
            return args.Handled;
        }
    }
}