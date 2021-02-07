﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPMessengerClient.MSNP.SOAP
{
    partial class SOAPRequests
    {
        protected string abservice_url = "https://m1.escargot.log1p.xyz/abservice/abservice.asmx";
        //local address is http://localhost/abservice/abservice.asmx for abservice_url

        public string ABFindAll()
        {
            string address_book_xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
                           xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                           xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                           xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">
	            <soap:Header>
		            <ABApplicationHeader xmlns=""http://www.msn.com/webservices/AddressBook"">
			            <ApplicationId>CFE80F9D-180F-4399-82AB-413F33A1FA11</ApplicationId>
			            <IsMigration>false</IsMigration>
			            <PartnerScenario>Initial</PartnerScenario>
		            </ABApplicationHeader>
		            <ABAuthHeader xmlns=""http://www.msn.com/webservices/AddressBook"">
			            <ManagedGroupRequest>false</ManagedGroupRequest>
                        <TicketToken>{TicketToken}</TicketToken>
		            </ABAuthHeader>
	            </soap:Header>
	            <soap:Body>
		            <ABFindAll xmlns=""http://www.msn.com/webservices/AddressBook"">
			            <abId>00000000-0000-0000-0000-000000000000</abId>
			            <abView>Full</abView>
			            <deltasOnly>false</deltasOnly>
			            <lastChange>0001-01-01T00:00:00.0000000-08:00</lastChange>
		            </ABFindAll>
	            </soap:Body>
            </soap:Envelope>";
            return MakeSOAPRequest(address_book_xml, abservice_url, "http://www.msn.com/webservices/AddressBook/ABFindAll");
        }

        public string ABContactAdd(string newContactEmail)
        {
            string add_contact_xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
                           xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                           xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                           xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">
                <soap:Header>
                    <ABApplicationHeader xmlns=""http://www.msn.com/webservices/AddressBook"">
                        <ApplicationId>996CDE1B-AA53-4477-B943-2BB802EA6166</ApplicationId>
                        <IsMigration>false</IsMigration>
                        <PartnerScenario>ContactSave</PartnerScenario>
                    </ABApplicationHeader>
                    <ABAuthHeader xmlns=""http://www.msn.com/webservices/AddressBook"">
                        <ManagedGroupRequest>false</ManagedGroupRequest>
                        <TicketToken>{TicketToken}</TicketToken>
                    </ABAuthHeader>
                </soap:Header>
                <soap:Body>
                    <ABContactAdd xmlns=""http://www.msn.com/webservices/AddressBook"">
                        <abId>00000000-0000-0000-0000-000000000000</abId>
                        <contacts>
                            <Contact xmlns=""http://www.msn.com/webservices/AddressBook"">
                                <contactInfo>
                                    <isMessengerUser>true</isMessengerUser>
                                    <passportName>{newContactEmail}</passportName>
                                </contactInfo>
                            </Contact>
                        </contacts>
                        <options>
                            <EnableAllowListManagement>true</EnableAllowListManagement>
                        </options>
                    </ABContactAdd>
                </soap:Body>
            </soap:Envelope>";
            return MakeSOAPRequest(add_contact_xml, abservice_url, "http://www.msn.com/webservices/AddressBook/ABContactAdd");
        }

        public string ABContactDelete(Contact contact)
        {
            string remove_contact_xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
                           xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                           xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                           xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"">
                <soap:Header>
                    <ABApplicationHeader xmlns=""http://www.msn.com/webservices/AddressBook"">
                        <ApplicationId>996CDE1B-AA53-4477-B943-2BB802EA6166</ApplicationId>
                        <IsMigration>false</IsMigration>
                        <PartnerScenario>Timer</PartnerScenario>
                    </ABApplicationHeader>
                    <ABAuthHeader xmlns=""http://www.msn.com/webservices/AddressBook"">
                        <ManagedGroupRequest>false</ManagedGroupRequest>
                        <TicketToken>{TicketToken}</TicketToken>
                    </ABAuthHeader>
                </soap:Header>
                <soap:Body>
                    <ABContactDelete xmlns=""http://www.msn.com/webservices/AddressBook"">
                        <abId>00000000-0000-0000-0000-000000000000</abId>
                        <contacts>
                            <Contact>
                                <contactId>{contact.contactID}</contactId>
                            </Contact>
                        </contacts>
                    </ABContactDelete>
                </soap:Body>
            </soap:Envelope>";
            return MakeSOAPRequest(remove_contact_xml, abservice_url, "http://www.msn.com/webservices/AddressBook/ABContactDelete");
        }

        public string ChangeUserDisplayNameRequest(string newDisplayName)
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
            return MakeSOAPRequest(ab_display_name_change_xml, abservice_url, "http://www.msn.com/webservices/AddressBook/ABContactUpdate");
        }
    }
}