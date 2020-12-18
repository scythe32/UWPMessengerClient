﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace UWPMessengerClient.MSNP12
{
    public partial class SwitchboardConnection
    {
        private SocketCommands SBSocket;
        private string SBAddress;
        private int SBPort = 0;
        private string UserEmail;
        private string TrID;
        private string SessionID;
        public UserInfo PrincipalInfo { get; set; } = new UserInfo();
        public UserInfo userInfo { get; set; } = new UserInfo();
        public bool connected { get; set; }
        public int principalsConnected { get; set; }
        public string outputString { get; set; }
        public byte[] outputBuffer { get; set; } = new byte[4096];
        private bool waitingTyping = false;

        public SwitchboardConnection(string email, string userDisplayName)
        {
            UserEmail = email;
            userInfo.displayName = userDisplayName;
        }

        public SwitchboardConnection(string address, int port, string email, string trID, string userDisplayName)
        {
            SBAddress = address;
            SBPort = port;
            UserEmail = email;
            TrID = trID;
            userInfo.displayName = userDisplayName;
        }

        public SwitchboardConnection(string address, int port, string email, string trID, string userDisplayName, string principalDisplayName, string sessionID)
        {
            SBAddress = address;
            SBPort = port;
            UserEmail = email;
            TrID = trID;
            SessionID = sessionID;
            userInfo.displayName = userDisplayName;
            PrincipalInfo.displayName = principalDisplayName;
        }

        public void SetAddressPortAndTrID(string address, int port, string trID)
        {
            SBAddress = address;
            SBPort = port;
            TrID = trID;
        }

        public async Task LoginToNewSwitchboardAsync()
        {
            Action sbconnect = new Action(() =>
            {
                SBSocket = new SocketCommands(SBAddress, SBPort);
                SBSocket.ConnectSocket();
                SBSocket.BeginReceiving(outputBuffer, new AsyncCallback(ReceivingCallback), this);
                SBSocket.SendCommand($"USR 1 {UserEmail} {TrID}\r\n");
            });
            await Task.Run(sbconnect);
            connected = true;
        }

        public async Task InvitePrincipal(string principal_email)
        {
            if (connected)
            {
                await Task.Run(() =>
                {
                    SBSocket.SendCommand($"CAL 2 {principal_email}\r\n");
                });
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task InvitePrincipal(string principal_email, string principal_display_name)
        {
            if (connected)
            {
                await Task.Run(() =>
                {
                    SBSocket.SendCommand($"CAL 2 {principal_email}\r\n");
                    Windows.Foundation.IAsyncAction task = Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        PrincipalInfo.displayName = principal_display_name;
                    });
                });
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task SendMessage(string message_text)
        {
            if (connected && principalsConnected > 0)
            {
                await Task.Run(() => 
                {
                    string message = "MIME-Version: 1.0\r\nContent-Type: text/plain; charset=UTF-8\r\nX-MMS-IM-Format: FN=Arial; EF=; CO=0; CS=0; PF=22\r\n\r\n" + message_text;
                    byte[] byte_message = Encoding.UTF8.GetBytes(message);
                    SBSocket.SendCommand($"MSG 3 N {byte_message.Length}\r\n{message}");
                    Windows.Foundation.IAsyncAction task = Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageList.Add(new Message() { message_text = message_text, sender = userInfo.displayName });
                    });
                });
            }
        }

        public async Task SendTypingUser()
        {
            if (connected && principalsConnected > 0 && !waitingTyping)
            {
                await Task.Run(() =>
                {
                    string message = $"MIME-Version: 1.0\r\nContent-Type: text/x-msmsgscontrol\r\nTypingUser: {UserEmail}\r\n\r\n\r\n";
                    byte[] byte_message = Encoding.UTF8.GetBytes(message);
                    SBSocket.SendCommand($"MSG 3 U {byte_message.Length}\r\n{message}");
                });
                waitingTyping = true;
                await Task.Delay(5000);
                waitingTyping = false;
            }
        }

        public async Task AnswerRNG()
        {
            await Task.Run(() =>
            {
                SBSocket = new SocketCommands(SBAddress, SBPort);
                SBSocket.ConnectSocket();
                SBSocket.BeginReceiving(outputBuffer, new AsyncCallback(ReceivingCallback), this);
                SBSocket.SendCommand($"ANS 1 {UserEmail} {TrID} {SessionID}\r\n");
            });
        }

        public void Exit()
        {
            SBSocket.SendCommand("OUT\r\n");
            SBSocket.CloseSocket();
        }

        ~SwitchboardConnection()
        {
            Exit();
        }
    }
}