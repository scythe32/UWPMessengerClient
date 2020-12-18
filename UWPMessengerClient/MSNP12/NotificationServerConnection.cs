﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Windows.UI.Core;
using System.Collections.ObjectModel;

namespace UWPMessengerClient.MSNP12
{
    public partial class NotificationServerConnection
    {
        private SocketCommands NSSocket;
        private HttpClient httpClient;
        //notification server(escargot) address and nexus address
        private string NSaddress = "m1.escargot.log1p.xyz";
        private string nexus_address = "https://m1.escargot.log1p.xyz/nexus-mock";
        //local addresses are 127.0.0.1 for NSaddress and http://localhost/nexus-mock for nexus_address
        private readonly int port = 1863;
        private string email;
        private string password;
        private string token;
        private bool _UsingLocalhost = false;
        public int ContactIndexToChat { get; set; }
        public string CurrentUserPresenceStatus { get; set; }
        public bool UsingLocalhost { get => _UsingLocalhost; }
        public UserInfo userInfo { get; set; } = new UserInfo();

        public NotificationServerConnection(string messenger_email, string messenger_password, bool use_localhost)
        {
            email = messenger_email;
            password = messenger_password;
            _UsingLocalhost = use_localhost;
            if (_UsingLocalhost)
            {
                NSaddress = "127.0.0.1";
                nexus_address = "http://localhost/nexus-mock";
                //setting local addresses
            }
        }

        public async Task LoginToMessengerAsync()
        {
            httpClient = new HttpClient();
            NSSocket = new SocketCommands(NSaddress, port);
            Action loginAction = new Action(() =>
            {
                //sequence of commands to login to escargot
                NSSocket.ConnectSocket();
                //begin receiving from escargot
                NSSocket.BeginReceiving(received_bytes, new AsyncCallback(ReceivingCallback), this);
                NSSocket.SendCommand("VER 1 MSNP12 CVR0\r\n");//send msnp version
                NSSocket.SendCommand("CVR 2 0x0409 winnt 10 i386 UWPMESSENGER 0.6 msmsgs\r\n");//send client information
                NSSocket.SendCommand($"USR 3 TWN I {email}\r\n");//sends email to get a string for use in authentication
                userInfo.Email = email;
                Task<string> token_task = GetNexusTokenAsync(httpClient);
                token = token_task.Result;
                NSSocket.SendCommand($"USR 4 TWN S t={token}\r\n");//sending authentication token
                NSSocket.SendCommand("SYN 5 0 0\r\n");//sync contact list
                NSSocket.SendCommand("CHG 6 NLN 0\r\n");//set presence as available
            });
            await Task.Run(loginAction);
            CurrentUserPresenceStatus = "NLN";
        }

        public async Task<string> GetNexusTokenAsync(HttpClient httpClient)
        {
            //makes a request to the nexus and gets the Www-Authenticate header
            HttpResponseMessage response = await httpClient.GetAsync(nexus_address);
            response.EnsureSuccessStatusCode();
            HttpResponseHeaders responseHeaders = response.Headers;
            //parsing the response headers to extract the login server adress
            string headersString = responseHeaders.ToString();
            string[] SplitHeadersString = headersString.Split("DALogin=");
            string DALogin = SplitHeadersString[1];
            DALogin = DALogin.Remove(DALogin.IndexOf("\r"));
            if (_UsingLocalhost)
            {
                DALogin = "http://localhost/login";
            }
            string email_encoded = HttpUtility.UrlEncode(email);
            string password_encoded = HttpUtility.UrlEncode(password);
            //makes a request to the login address and gets the from-PP header
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Passport1.4 OrgVerb=GET,OrgUrl=http%3A%2F%2Fmessenger%2Emsn%2Ecom,sign-in={email_encoded},pwd={password_encoded},ct=1,rver=1,wp=FS_40SEC_0_COMPACT,lc=1,id=1");
            response = await httpClient.GetAsync(DALogin);
            response.EnsureSuccessStatusCode();
            responseHeaders = response.Headers;
            //parsing the response headers to extract the token
            headersString = responseHeaders.ToString();
            string[] fromPP_split = headersString.Split("from-PP='");
            string fromPP = fromPP_split[1];
            fromPP = fromPP.Remove(fromPP.IndexOf("'\r"));
            return fromPP;
        }

        public async Task ChangePresence(string status)
        {
            if (status == "") { throw new ArgumentNullException("Status is empty"); }
            Action changePresence = new Action(() =>
            {
                NSSocket.SendCommand($"CHG 7 {status} 0\r\n");
            });
            CurrentUserPresenceStatus = status;
            await Task.Run(changePresence);
        }

        public async Task ChangeUserDisplayName(string newDisplayName)
        {
            if (newDisplayName == "") { throw new ArgumentNullException("Display name is empty"); }
            string urlEncodedNewDisplayName = Uri.EscapeUriString(newDisplayName);
            await Task.Run(() => NSSocket.SendCommand($"PRP 8 MFN {urlEncodedNewDisplayName}\r\n"));
        }

        public async Task SendUserPersonalMessage(string newPersonalMessage)
        {
            Action psm_action = new Action(() =>
            {
                string encodedPersonalMessage = newPersonalMessage.Replace("&", "&amp;");
                string psm_payload = $@"<Data><PSM>{encodedPersonalMessage}</PSM><CurrentMedia></CurrentMedia></Data>";
                int payload_length = Encoding.UTF8.GetBytes(psm_payload).Length;
                NSSocket.SendCommand($"UUX 12 {payload_length}\r\n" + psm_payload);
                Windows.Foundation.IAsyncAction task = Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    userInfo.personalMessage = newPersonalMessage;
                });
            });
            await Task.Run(psm_action);
        }

        public async Task AddContact(string newContactEmail, string newContactDisplayName = "")
        {
            if (newContactEmail == "") { throw new ArgumentNullException("Contact email is empty"); }
            if (newContactDisplayName == "")
            {
                newContactDisplayName = newContactEmail;
            }
            await Task.Run(() => NSSocket.SendCommand($"ADC 11 FL N={newContactEmail} F={newContactDisplayName}\r\n"));
        }

        public async Task RemoveContact(Contact contactToRemove)
        {
            await Task.Run(() => NSSocket.SendCommand($"REM 11 FL {contactToRemove.GUID}\r\n"));
            contact_list.Remove(contactToRemove);
            contacts_in_forward_list.Remove(contactToRemove);
        }

        public async Task InitiateSB()
        {
            SwitchboardConnection switchboardConnection = new SwitchboardConnection(email, userInfo.displayName);
            SBConnection = switchboardConnection;
            await Task.Run(() => NSSocket.SendCommand("XFR 10 SB\r\n"));
        }

        public void Exit()
        {
            NSSocket.SendCommand("OUT\r\n");
            NSSocket.CloseSocket();
        }

        ~NotificationServerConnection()
        {
            Exit();
        }
    }
}