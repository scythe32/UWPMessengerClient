﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.IO;
using Windows.UI.Core;

namespace UWPMessengerClient.MSNP15
{
    public partial class NotificationServerConnection
    {
        private SocketCommands NSSocket;
        public SwitchboardConnection SBConnection { get; set; }
        //notification server(escargot) address and address for SSO auth
        private string NSaddress = "m1.escargot.log1p.xyz";
        private string RST_address = "https://m1.escargot.log1p.xyz/RST.srf";
        //local addresses are 127.0.0.1 for NSaddress and http://localhost/RST.srf for RST_address
        private readonly int port = 1863;
        private string email;
        private string password;
        private bool _UsingLocalhost = false;
        public int ContactIndexToChat { get; set; }
        public string CurrentUserPresenceStatus { get; set; }
        public bool UsingLocalhost { get => _UsingLocalhost; }

        public NotificationServerConnection(string messenger_email, string messenger_password, bool use_localhost)
        {
            email = messenger_email;
            password = messenger_password;
            _UsingLocalhost = use_localhost;
            if (_UsingLocalhost)
            {
                NSaddress = "127.0.0.1";
                RST_address = "http://localhost/RST.srf";
                SharingService_url = "http://localhost/abservice/SharingService.asmx";
                abservice_url = "http://localhost/abservice/abservice.asmx";
                //setting local addresses
            }
        }

        public static HttpWebRequest CreateSOAPRequest(string soap_action, string address)
        {
            HttpWebRequest request = WebRequest.CreateHttp(address);
            request.Headers.Add($@"SOAPAction:{soap_action}");
            request.ContentType = "text/xml;charset=\"utf-8\"";
            request.Accept = "text/xml";
            request.Method = "POST";
            return request;
        }

        public static string MakeSOAPRequest(string SOAP_body, string address, string soap_action)
        {
            HttpWebRequest SOAPRequest = CreateSOAPRequest(soap_action, address);
            XmlDocument SoapXMLBody = new XmlDocument();
            SoapXMLBody.LoadXml(SOAP_body);
            using (Stream stream = SOAPRequest.GetRequestStream())
            {
                SoapXMLBody.Save(stream);
            }
            using (WebResponse webResponse = SOAPRequest.GetResponse())
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    var result = rd.ReadToEnd();
                    return result;
                }
            }
        }

        public static byte[] JoinBytes(byte[] first, byte[] second)
        {
            return first.Concat(second).ToArray();
        }

        public void FillForwardListCollection()
        {
            foreach (Contact contact in contact_list)
            {
                if (contact.onForward == true)
                {
                    Windows.Foundation.IAsyncAction task = Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        contacts_in_forward_list.Add(contact);
                    });
                }
            }
        }

        public async Task ChangePresence(string status)
        {
            if (status == "") { throw new ArgumentNullException("Status is empty"); }
            Action changePresence = new Action(() =>
            {
                NSSocket.SendCommand($"CHG 9 {status} 0\r\n");
            });
            CurrentUserPresenceStatus = status;
            await Task.Run(changePresence);
        }

        public async Task ChangeUserDisplayName(string newDisplayName)
        {
            if (newDisplayName == "") { throw new ArgumentNullException("Display name is empty"); }
            string ab_display_name_change_xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
                           xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                           xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                           xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">
                <soap:Header>
                    <ABApplicationHeader xmlns=""http://www.msn.com/webservices/AddressBook"">
                        <ApplicationId>CFE80F9D-180F-4399-82AB-413F33A1FA11</ApplicationId>
                        <IsMigration>false</IsMigration>
                        <PartnerScenario>Timer</PartnerScenario>
                    </ABApplicationHeader>
                    <ABAuthHeader xmlns=""http://www.msn.com/webservices/AddressBook"">
                        <ManagedGroupRequest>false</ManagedGroupRequest>
                        <TicketToken>{TicketToken}</TicketToken>
                    </ABAuthHeader>
                </soap:Header>
                <soap:Body>
                    <ABContactUpdate xmlns=""http://www.msn.com/webservices/AddressBook"">
                        <abId>00000000-0000-0000-0000-000000000000</abId>
                        <contacts>
                            <Contact xmlns=""http://www.msn.com/webservices/AddressBook"">
                                <contactInfo>
                                    <contactType>Me</contactType>
                                    <displayName>{newDisplayName}</displayName>
                                </contactInfo>
                                <propertiesChanged>DisplayName</propertiesChanged>
                            </Contact>
                        </contacts>
                    </ABContactUpdate>
                </soap:Body>
            </soap:Envelope>";
            MakeSOAPRequest(ab_display_name_change_xml, abservice_url, "http://www.msn.com/webservices/AddressBook/ABContactUpdate");
            string urlEncodedNewDisplayName = Uri.EscapeUriString(newDisplayName);
            await Task.Run(() => NSSocket.SendCommand($"PRP 9 MFN {urlEncodedNewDisplayName}\r\n"));
        }

        public async Task InitiateSB()
        {
            SwitchboardConnection switchboardConnection = new SwitchboardConnection(email, userInfo.displayName);
            SBConnection = switchboardConnection;
            await Task.Run(() => NSSocket.SendCommand("XFR 9 SB\r\n"));
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
