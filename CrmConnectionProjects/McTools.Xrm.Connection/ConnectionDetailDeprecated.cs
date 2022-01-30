﻿using CrmWebResourcesUpdater.DataModels;
using System;
using System.ComponentModel;
using System.ServiceModel.Description;
using System.Xml.Serialization;


namespace McTools.Xrm.Connection.Deprecated
{
    /// <summary>
    /// Stores data regarding a specific connection to Crm server
    /// </summary>
    public class ConnectionDetail : IComparable
    {
        #region Properties

        public AuthenticationProviderType AuthType { get; set; }

        /// <summary>
        /// Gets or sets the connection unique identifier
        /// </summary>
        public Guid? ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection
        /// </summary>
        public string ConnectionName { get; set; }


        /// <summary>
        /// Get or set flag to know if custom authentication
        /// </summary>
        public bool IsCustomAuth { get; set; }

        /// <summary>
        /// Get or set flag to know if we use IFD
        /// </summary>
        public bool UseIfd { get; set; }

        /// <summary>
        /// Get or set flag to know if we use CRM Online
        /// </summary>
        public bool UseOnline { get; set; }

        /// <summary>
        /// Get or set flag to know if we use Online Services
        /// </summary>
        public bool UseOsdp { get; set; }

        /// <summary>
        /// Get or set the Crm Ticket
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public string CrmTicket { get; set; }

        /// <summary>
        /// Get or set the user domain name
        /// </summary>
        public string UserDomain { get; set; }

        /// <summary>
        /// Get or set user login
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Get or set the user password
        /// </summary>
        //[System.Xml.Serialization.XmlIgnore]
        public string UserPassword { get; set; }

        public bool SavePassword { get; set; }

        /// <summary>
        /// Get or set the use of SSL connection
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Get or set the server name
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Get or set the server port
        /// </summary>
        [DefaultValue(80)]
        public int ServerPort { get; set; }

        /// <summary>
        /// Get or set the organization Id
        /// </summary>
        public string OrganizationId { get; set; }
        /// <summary>
        /// Get or set the organization name
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// Get or set the organization name
        /// </summary>
        public string OrganizationUrlName { get; set; }

        /// <summary>
        /// Get or set the organization friendly name
        /// </summary>
        public string OrganizationFriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the Crm Service Url
        /// </summary>
        public string OrganizationServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the Home realm url for ADFS authentication
        /// </summary>
        public string HomeRealmUrl { get; set; }

        public string OrganizationVersion { get; set; }

        public string Solution { get; set; }
        public string SolutionId { get; set; }
        public string SolutionFriendlyName { get; set; }

        public string PublisherPrefix { get; set; }


        [XmlIgnore]
        public TimeSpan Timeout { get; set; }

        [XmlElement("Timeout")]
        public long TimeoutTicks
        {
            get { return Timeout.Ticks; }
            set { Timeout = new TimeSpan(value); }
        }

        public int OrganizationMajorVersion
        {
            get
            {
                return OrganizationVersion != null ? int.Parse(OrganizationVersion.Split('.')[0]) : -1;
            }
        }

        public int OrganizationMinorVersion
        {
            get
            {
                return OrganizationVersion != null ? int.Parse(OrganizationVersion.Split('.')[1]) : -1;
            }
        }

        public string WebApplicationUrl { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return ConnectionName;
        }

        public string GetDiscoveryCrmConnectionString()
        {
            var connectionString = string.Format("Url={0}://{1}:{2};",
                UseSsl ? "https" : "http",
                UseIfd ? ServerName : UseOsdp ? "disco." + ServerName : UseOnline ? "dev." + ServerName : ServerName,
                ServerPort == 0 ? (UseSsl ? 443 : 80) : ServerPort);

            if (IsCustomAuth)
            {
                if (!UseIfd)
                {
                    if (!string.IsNullOrEmpty(UserDomain))
                    {
                        connectionString += string.Format("Domain={0};", UserDomain);
                    }
                }

                string username = UserName;
                if (UseIfd)
                {
                    if (!string.IsNullOrEmpty(UserDomain))
                    {
                        username = string.Format("{0}\\{1}", UserDomain, UserName);
                    }
                }

                connectionString += string.Format("Username={0};Password={1};", username, UserPassword);
            }

            if (UseOnline && !UseOsdp)
            {
                ClientCredentials deviceCredentials;

                do
                {
                    deviceCredentials = DeviceIdManager.LoadDeviceCredentials() ??
                                        DeviceIdManager.RegisterDevice();
                } while (deviceCredentials.UserName.Password.Contains(";")
                         || deviceCredentials.UserName.Password.Contains("=")
                         || deviceCredentials.UserName.Password.Contains(" ")
                         || deviceCredentials.UserName.UserName.Contains(";")
                         || deviceCredentials.UserName.UserName.Contains("=")
                         || deviceCredentials.UserName.UserName.Contains(" "));

                connectionString += string.Format("DeviceID={0};DevicePassword={1};",
                                                  deviceCredentials.UserName.UserName,
                                                  deviceCredentials.UserName.Password);
            }

            if (UseIfd && !string.IsNullOrEmpty(HomeRealmUrl))
            {
                connectionString += string.Format("HomeRealmUri={0};", HomeRealmUrl);
            }

            return connectionString;
        }

        public string GetOrganizationCrmConnectionString()
        {
            var currentServerName = string.Empty;
            var authType = "AD";

            if (UseOsdp || UseOnline)
            {
                currentServerName = string.Format("{0}.{1}", OrganizationUrlName, ServerName);
            }
            else if (UseIfd)
            {
                var serverNameParts = ServerName.Split('.');

                serverNameParts[0] = OrganizationUrlName;


                currentServerName = string.Format("{0}:{1}",
                                                  string.Join(".", serverNameParts),
                                                  ServerPort == 0 ? (UseSsl ? 443 : 80) : ServerPort);
            }
            else
            {
                currentServerName = string.Format("{0}:{1}/{2}",
                                                  ServerName,
                                                  ServerPort == 0 ? (UseSsl ? 443 : 80) : ServerPort,
                                                  Organization);
            }

            var connectionString = string.Format("Url={0}://{1};",
                                                 UseSsl ? "https" : "http",
                                                 currentServerName);

            //var connectionString = string.Format("Url={0};", OrganizationServiceUrl.Replace("/XRMServices/2011/Organization.svc", ""));

            if (IsCustomAuth)
            {
                if (!UseIfd)
                {
                    if (!string.IsNullOrEmpty(UserDomain))
                    {
                        connectionString += string.Format("Domain={0};", UserDomain);
                    }
                }

                string username = UserName;
                if (UseIfd)
                {
                    if (!string.IsNullOrEmpty(UserDomain))
                    {
                        username = string.Format("{0}\\{1}", UserDomain, UserName);
                    }
                }

                connectionString += string.Format("Username={0};Password={1};", username, UserPassword);
            }

            if (UseOnline)
            {
                ClientCredentials deviceCredentials;

                do
                {
                    deviceCredentials = DeviceIdManager.LoadDeviceCredentials() ??
                                        DeviceIdManager.RegisterDevice();
                } while (deviceCredentials.UserName.Password.Contains(";")
                         || deviceCredentials.UserName.Password.Contains("=")
                         || deviceCredentials.UserName.Password.Contains(" ")
                         || deviceCredentials.UserName.UserName.Contains(";")
                         || deviceCredentials.UserName.UserName.Contains("=")
                         || deviceCredentials.UserName.UserName.Contains(" "));

                connectionString += string.Format("DeviceID={0};DevicePassword={1};",
                                                  deviceCredentials.UserName.UserName,
                                                  deviceCredentials.UserName.Password);

                authType = "Office365";
            }

            if (UseIfd && !string.IsNullOrEmpty(HomeRealmUrl))
            {
                connectionString += string.Format("HomeRealmUri={0};", HomeRealmUrl);
            }
            if (UseIfd)
            {
                authType = "IFD";
            }
            connectionString += string.Format("AuthType={0};", authType);

            //append timeout in seconds to connectionstring
            connectionString += string.Format("Timeout={0};", Timeout.ToString(@"hh\:mm\:ss"));
            return connectionString;
        }

        public string GetConnectionLongName()
        {
            return ConnectionName + " - " + OrganizationFriendlyName + " - " + SolutionFriendlyName;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return String.CompareOrdinal(ConnectionName, ((ConnectionDetail)obj).ConnectionName);
        }




        #endregion

        public string GetDiscoveryServiceUrl()
        {
            var url = string.Format("{0}://{1}:{2}/XRMServices/2011/Discovery.svc",
                UseSsl ? "https" : "http",
                UseIfd ? ServerName : UseOsdp ? "disco." + ServerName : UseOnline ? "dev." + ServerName : ServerName,
                ServerPort == 0 ? UseSsl ? 443 : 80 : ServerPort); //"" : !UseSsl && ServerPort == 80 ? "" : UseSsl && ServerPort == 443 ? "" : ":" + ServerPort.ToString());
            return url;
        }
    }
}