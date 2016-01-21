using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;

using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Crm.Sdk.Helper
{
    /// <summary>
    /// Provides server connection information.
    /// </summary>
    public sealed class ServerConnection
    {
        private static readonly ServerConnection instance = new ServerConnection();

        private ServerConnection() { }

        public static ServerConnection Instance
        {
            get 
            {
                return instance; 
            }
        }

        #region Inner classes
        /// <summary>
        /// Stores Microsoft Dynamics CRM server configuration information.
        /// </summary>
        public class Configuration
        {
            public String ServerAddress;
            public String OrganizationName;
            public Uri DiscoveryUri;
            public Uri OrganizationUri;
            public Uri HomeRealmUri = null;
            public ClientCredentials DeviceCredentials = null;
            public ClientCredentials Credentials = null;
            public AuthenticationProviderType EndpointType;
            public String UserPrincipalName;
            #region internal members of the class
            internal IServiceManagement<IOrganizationService> OrganizationServiceManagement;
            internal SecurityTokenResponse OrganizationTokenResponse;
            internal Int16 AuthFailureCount = 0;
            #endregion

            public override bool Equals(object obj)
            {
                //Check for null and compare run-time types.
                if (obj == null || GetType() != obj.GetType()) return false;

                Configuration c = (Configuration)obj;

                if (!this.ServerAddress.Equals(c.ServerAddress, StringComparison.InvariantCultureIgnoreCase))
                    return false;
                if (!this.OrganizationName.Equals(c.OrganizationName, StringComparison.InvariantCultureIgnoreCase))
                    return false;
                if (this.EndpointType != c.EndpointType)
                    return false;
                if (null != this.Credentials && null != c.Credentials)
                {
                    if (this.EndpointType == AuthenticationProviderType.ActiveDirectory)
                    {

                        if (!this.Credentials.Windows.ClientCredential.Domain.Equals(
                            c.Credentials.Windows.ClientCredential.Domain, StringComparison.InvariantCultureIgnoreCase))
                            return false;
                        if (!this.Credentials.Windows.ClientCredential.UserName.Equals(
                            c.Credentials.Windows.ClientCredential.UserName, StringComparison.InvariantCultureIgnoreCase))
                            return false;

                    }
                    else if (this.EndpointType == AuthenticationProviderType.LiveId)
                    {
                        if (!this.Credentials.UserName.UserName.Equals(c.Credentials.UserName.UserName,
                            StringComparison.InvariantCultureIgnoreCase))
                            return false;
                        if (!this.DeviceCredentials.UserName.UserName.Equals(
                            c.DeviceCredentials.UserName.UserName, StringComparison.InvariantCultureIgnoreCase))
                            return false;
                        if (!this.DeviceCredentials.UserName.Password.Equals(
                            c.DeviceCredentials.UserName.Password, StringComparison.InvariantCultureIgnoreCase))
                            return false;
                    }
                    else
                    {

                        if (!this.Credentials.UserName.UserName.Equals(c.Credentials.UserName.UserName,
                            StringComparison.InvariantCultureIgnoreCase))
                            return false;

                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                int returnHashCode = this.ServerAddress.GetHashCode()
                    ^ this.OrganizationName.GetHashCode()
                    ^ this.EndpointType.GetHashCode();
                if (null != this.Credentials)
                {
                    if (this.EndpointType == AuthenticationProviderType.ActiveDirectory)
                        returnHashCode = returnHashCode
                            ^ this.Credentials.Windows.ClientCredential.UserName.GetHashCode()
                            ^ this.Credentials.Windows.ClientCredential.Domain.GetHashCode();
                    else if (this.EndpointType == AuthenticationProviderType.LiveId)
                        returnHashCode = returnHashCode
                            ^ this.Credentials.UserName.UserName.GetHashCode()
                            ^ this.DeviceCredentials.UserName.UserName.GetHashCode()
                            ^ this.DeviceCredentials.UserName.Password.GetHashCode();
                    else
                        returnHashCode = returnHashCode
                            ^ this.Credentials.UserName.UserName.GetHashCode();
                }
                return returnHashCode;
            }

        }
        #endregion Inner classes

        #region Private properties

        private Configuration config = new Configuration();

        private IConfigurationProvider ConfigurationProvider { get; set; }

        #endregion Private properties

        #region Public methods

        /// <summary>
        /// Get the singleton instance of <c>ServerConnection</c>.
        /// </summary>
        /// <param name="_configurationProvider">Configuration Provider.</param>
        /// <returns></returns>
        public static ServerConnection Get(IConfigurationProvider _configurationProvider)
        {
            ServerConnection.Instance.ConfigurationProvider = _configurationProvider;
            return ServerConnection.Instance;
        }

        /// <summary>
        /// Obtains the server connection information including the target organization's
        /// Uri and user logon credentials from the user.
        /// </summary>
        /// <param name="reAuthenticate">True if the configuration needs to be setup by authenticating again.</param>
        public Configuration GetServerConfiguration(bool reAuthenticate = false)
        {
            // Recreate config if authentication has expired.
            if (reAuthenticate || String.IsNullOrEmpty(config.ServerAddress))
            {
                // Get the server address. If no value is entered, default to Microsoft Dynamics
                // CRM Online in the North American data center.
                config.ServerAddress = "crm.dynamics.com";

                config.DiscoveryUri =
                    new Uri(String.Format("https://disco.{0}/XRMServices/2011/Discovery.svc", config.ServerAddress));

                try
                {
                    // Get the target organization.
                    config.OrganizationUri = GetOrganizationAddress();
                }
                catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                {
                    Trace.TraceError("Server connection terminated with an error.");
                    Trace.TraceError("Timestamp: {0}", ex.Detail.Timestamp);
                    Trace.TraceError("Code: {0}", ex.Detail.ErrorCode);
                    Trace.TraceError("Message: {0}", ex.Detail.Message);
                    Trace.TraceError("Plugin Trace: {0}", ex.Detail.TraceText);
                }
                catch (System.TimeoutException ex)
                {
                    Trace.TraceError("Server connection terminated with an error.");
                    Trace.TraceError("Message: {0}", ex.Message);
                    Trace.TraceError("Stack Trace: {0}", ex.StackTrace);
                    Trace.TraceError("Inner Fault: {0}",
                        null == ex.InnerException.Message ? "No Inner Fault" : ex.InnerException.Message);
                }
                catch (System.Exception ex)
                {
                    Trace.TraceError("Server connection terminated with an error.");
                    Trace.TraceError(ex.Message);

                    // Display the details of the inner exception.
                    if (ex.InnerException != null)
                    {
                        Trace.TraceError(ex.InnerException.Message);

                        FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fe = ex.InnerException
                            as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                        if (fe != null)
                        {
                            Trace.TraceError("Timestamp: {0}", fe.Detail.Timestamp);
                            Trace.TraceError("Code: {0}", fe.Detail.ErrorCode);
                            Trace.TraceError("Message: {0}", fe.Detail.Message);
                            Trace.TraceError("Plugin Trace: {0}", fe.Detail.TraceText);
                        }
                    }
                }
            }

            return config;
        }

        /// <summary>
        /// Generic method to obtain discovery/organization service proxy instance.
        /// </summary>
        /// <typeparam name="TService">
        /// Set IDiscoveryService or IOrganizationService type 
        /// to request respective service proxy instance.
        /// </typeparam>
        /// <typeparam name="TProxy">
        /// Set the return type to either DiscoveryServiceProxy 
        /// or OrganizationServiceProxy type based on TService type.
        /// </typeparam>
        /// <param name="currentConfig">An instance of existing Configuration</param>
        /// <returns>An instance of TProxy 
        /// i.e. DiscoveryServiceProxy or OrganizationServiceProxy</returns>
        public static TProxy GetProxy<TService, TProxy>(ServerConnection.Configuration currentConfig,
                                                        IConfigurationProvider configurationProvider)
            where TService : class
            where TProxy : ServiceProxy<TService>
        {
            // Check if it is organization service proxy request.
            Boolean isOrgServiceRequest = typeof(TService).Equals(typeof(IOrganizationService));

            // Get appropriate Uri from Configuration.
            Uri serviceUri = isOrgServiceRequest ?
                currentConfig.OrganizationUri : currentConfig.DiscoveryUri;

            // Set service management for either organization service Uri or discovery service Uri.
            // For organization service Uri, if service management exists 
            // then use it from cache. Otherwise create new service management for current organization.
            IServiceManagement<TService> serviceManagement =
                (isOrgServiceRequest && null != currentConfig.OrganizationServiceManagement) ?
                (IServiceManagement<TService>)currentConfig.OrganizationServiceManagement :
                ServiceConfigurationFactory.CreateManagement<TService>(
                serviceUri);

            if (isOrgServiceRequest)
            {
                if (currentConfig.OrganizationTokenResponse == null)
                {
                    currentConfig.OrganizationServiceManagement =
                        (IServiceManagement<IOrganizationService>)serviceManagement;
                }
            }
            // Set the EndpointType in the current Configuration object 
            // while adding new configuration using discovery service proxy.
            else
            {
                // Get the EndpointType.
                currentConfig.EndpointType = serviceManagement.AuthenticationType;
                
                ClientCredentials credentials = new ClientCredentials();

                credentials.UserName.UserName = configurationProvider.GetConfigurationSettingValue("CRMUserAccount"); //"william@PSA365.onmicrosoft.com";
                credentials.UserName.Password = configurationProvider.GetConfigurationSettingValue("CRMAccountPwd"); //"pass@word1";

                currentConfig.Credentials = credentials;
            }

            // Set the credentials.
            AuthenticationCredentials authCredentials = new AuthenticationCredentials();

            // If UserPrincipalName exists, use it. Otherwise, set the logon credentials from the configuration.
            if (!String.IsNullOrWhiteSpace(currentConfig.UserPrincipalName))
            {
                // Single sing-on with the Federated Identity organization using current UserPrinicipalName.
                authCredentials.UserPrincipalName = currentConfig.UserPrincipalName;
            }
            else
            {
                authCredentials.ClientCredentials = currentConfig.Credentials;
            }

            Type classType;

            // Obtain discovery/organization service proxy for Federated,
            // Microsoft account and OnlineFederated environments. 
            if (currentConfig.EndpointType !=
                AuthenticationProviderType.ActiveDirectory)
            {
                if (currentConfig.EndpointType == AuthenticationProviderType.LiveId)
                {
                    authCredentials.SupportingCredentials = new AuthenticationCredentials();
                    authCredentials.SupportingCredentials.ClientCredentials =
                        currentConfig.DeviceCredentials;
                }

                AuthenticationCredentials tokenCredentials =
                    serviceManagement.Authenticate(
                        authCredentials);

                if (isOrgServiceRequest)
                {
                    // Set SecurityTokenResponse for the current organization.
                    currentConfig.OrganizationTokenResponse = tokenCredentials.SecurityTokenResponse;
                    // Set classType to ManagedTokenOrganizationServiceProxy.
                    classType = typeof(ManagedTokenOrganizationServiceProxy);

                }
                else
                {
                    // Set classType to ManagedTokenDiscoveryServiceProxy.
                    classType = typeof(ManagedTokenDiscoveryServiceProxy);
                }

                // Invokes ManagedTokenOrganizationServiceProxy or ManagedTokenDiscoveryServiceProxy 
                // (IServiceManagement<TService>, SecurityTokenResponse) constructor.
                return (TProxy)classType
                .GetConstructor(new Type[] 
                    { 
                        typeof(IServiceManagement<TService>), 
                        typeof(SecurityTokenResponse) 
                    })
                .Invoke(new object[] 
                    { 
                        serviceManagement, 
                        tokenCredentials.SecurityTokenResponse 
                    });
            }

            // Obtain discovery/organization service proxy for ActiveDirectory environment.
            if (isOrgServiceRequest)
            {
                classType = typeof(ManagedTokenOrganizationServiceProxy);
            }
            else
            {
                classType = typeof(ManagedTokenDiscoveryServiceProxy);
            }

            // Invokes ManagedTokenDiscoveryServiceProxy or ManagedTokenOrganizationServiceProxy 
            // (IServiceManagement<TService>, ClientCredentials) constructor.
            return (TProxy)classType
                .GetConstructor(new Type[] 
                   { 
                       typeof(IServiceManagement<TService>), 
                       typeof(ClientCredentials)
                   })
               .Invoke(new object[] 
                   { 
                       serviceManagement, 
                       authCredentials.ClientCredentials  
                   });
        }
        #endregion Public methods

        #region Protected methods

        /// <summary>
        /// Obtains the web address (Uri) of the target organization.
        /// </summary>
        /// <returns>Uri of the organization service or an empty string.</returns>
        protected Uri GetOrganizationAddress()
        {
            using (DiscoveryServiceProxy serviceProxy = GetDiscoveryProxy())
            {
                // Obtain organization information from the Discovery service. 
                if (serviceProxy != null)
                {
                    config.OrganizationName = this.ConfigurationProvider.GetConfigurationSettingValue("CRMInstanceName"); //"psareports";
                    // Return the organization Uri.
                    return new System.Uri(String.Format("https://{0}.api.crm.dynamics.com/XRMServices/2011/Organization.svc", config.OrganizationName));
                }
                else
                    throw new InvalidOperationException("An invalid server name was specified.");
            }
        }

        /// <summary>
        /// Get the discovery service proxy based on existing configuration data.
        /// Added new way of getting discovery proxy.
        /// Also preserving old way of getting discovery proxy to support old scenarios.
        /// </summary>
        /// <returns>An instance of DiscoveryServiceProxy</returns>
        private DiscoveryServiceProxy GetDiscoveryProxy()
        {
            try
            {
                // Obtain the discovery service proxy.
                DiscoveryServiceProxy discoveryProxy = GetProxy<IDiscoveryService, DiscoveryServiceProxy>(this.config, this.ConfigurationProvider);
                // Checking authentication by invoking some SDK methods.
                discoveryProxy.Execute(new RetrieveOrganizationsRequest());
                return discoveryProxy;
            }
            catch (System.ServiceModel.Security.SecurityAccessDeniedException ex)
            {
                // If authentication failed using current UserPrincipalName, 
                // request UserName and Password to try to authenticate using user credentials.
                if (!String.IsNullOrWhiteSpace(config.UserPrincipalName) &&
                    ex.Message.Contains("Access is denied."))
                {
                    config.AuthFailureCount += 1;
                }
                else
                {
                    throw ex;
                }
            }
            // You can also catch other exceptions to handle a specific situation in your code, for example, 
            //      System.ServiceModel.Security.ExpiredSecurityTokenException
            //      System.ServiceModel.Security.MessageSecurityException
            //      System.ServiceModel.Security.SecurityNegotiationException                

            // Second trial to obtain the discovery service proxy in case of single sign-on failure.
            return GetProxy<IDiscoveryService, DiscoveryServiceProxy>(this.config, this.ConfigurationProvider);
        }

        #endregion Private methods
    }

    #region Other Classes
    internal sealed class Credential
    {
        private SecureString _userName;
        private SecureString _password;

        internal Credential(CREDENTIAL_STRUCT cred)
        {
            _userName = ConvertToSecureString(cred.userName);
            int size = (int)cred.credentialBlobSize;
            if (size != 0)
            {
                byte[] bpassword = new byte[size];
                Marshal.Copy(cred.credentialBlob, bpassword, 0, size);
                _password = ConvertToSecureString(Encoding.Unicode.GetString(bpassword));
            }
            else
            {
                _password = ConvertToSecureString(String.Empty);
            }
        }

        public Credential(string userName, string password)
        {
            if (String.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException("userName");
            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("password");

            _userName = ConvertToSecureString(userName);
            _password = ConvertToSecureString(password);
        }

        public string UserName
        {
            get { return ConvertToUnsecureString(_userName); }
        }

        public string Password
        {
            get { return ConvertToUnsecureString(_password); }
        }

        /// <summary>
        /// This converts a SecureString password to plain text
        /// </summary>
        /// <param name="securePassword">SecureString password</param>
        /// <returns>plain text password</returns>
        private string ConvertToUnsecureString(SecureString secret)
        {
            if (secret == null)
                return string.Empty;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secret);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        /// <summary>
        /// This converts a string to SecureString
        /// </summary>
        /// <param name="password">plain text password</param>
        /// <returns>SecureString password</returns>
        private SecureString ConvertToSecureString(string secret)
        {
            if (string.IsNullOrEmpty(secret))
                return null;

            SecureString securePassword = new SecureString();
            char[] passwordChars = secret.ToCharArray();
            foreach (char pwdChar in passwordChars)
            {
                securePassword.AppendChar(pwdChar);
            }
            securePassword.MakeReadOnly();
            return securePassword;
        }


        /// <summary>
        /// This structure maps to the CREDENTIAL structure used by native code. We can use this to marshal our values.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CREDENTIAL_STRUCT
        {
            public UInt32 flags;
            public UInt32 type;
            public string targetName;
            public string comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME lastWritten;
            public UInt32 credentialBlobSize;
            public IntPtr credentialBlob;
            public UInt32 persist;
            public UInt32 attributeCount;
            public IntPtr credAttribute;
            public string targetAlias;
            public string userName;
        }

    }

    /// <summary>
    /// Wrapper class for DiscoveryServiceProxy to support auto refresh security token.
    /// </summary>
    internal sealed class ManagedTokenDiscoveryServiceProxy : DiscoveryServiceProxy
    {
        private AutoRefreshSecurityToken<DiscoveryServiceProxy, IDiscoveryService> _proxyManager;

        public ManagedTokenDiscoveryServiceProxy(Uri serviceUri, ClientCredentials userCredentials)
            : base(serviceUri, null, userCredentials, null)
        {
            this._proxyManager = new AutoRefreshSecurityToken<DiscoveryServiceProxy, IDiscoveryService>(this);
        }

        public ManagedTokenDiscoveryServiceProxy(IServiceManagement<IDiscoveryService> serviceManagement,
            SecurityTokenResponse securityTokenRes)
            : base(serviceManagement, securityTokenRes)
        {
            this._proxyManager = new AutoRefreshSecurityToken<DiscoveryServiceProxy, IDiscoveryService>(this);
        }

        public ManagedTokenDiscoveryServiceProxy(IServiceManagement<IDiscoveryService> serviceManagement,
           ClientCredentials userCredentials)
            : base(serviceManagement, userCredentials)
        {
            this._proxyManager = new AutoRefreshSecurityToken<DiscoveryServiceProxy, IDiscoveryService>(this);
        }
        
        protected override void AuthenticateCore()
        {
            this._proxyManager.PrepareCredentials();
            base.AuthenticateCore();
        }

        protected override void ValidateAuthentication()
        {
            this._proxyManager.RenewTokenIfRequired();
            base.ValidateAuthentication();
        }
    }

    /// <summary>
    /// Wrapper class for OrganizationServiceProxy to support auto refresh security token
    /// </summary>
    internal sealed class ManagedTokenOrganizationServiceProxy : OrganizationServiceProxy
    {
        private AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService> _proxyManager;

        public ManagedTokenOrganizationServiceProxy(Uri serviceUri, ClientCredentials userCredentials)
            : base(serviceUri, null, userCredentials, null)
        {
            this._proxyManager = new AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService>(this);
        }

        public ManagedTokenOrganizationServiceProxy(IServiceManagement<IOrganizationService> serviceManagement,
            SecurityTokenResponse securityTokenRes)
            : base(serviceManagement, securityTokenRes)
        {
            this._proxyManager = new AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService>(this);
        }

        public ManagedTokenOrganizationServiceProxy(IServiceManagement<IOrganizationService> serviceManagement,
            ClientCredentials userCredentials)
            : base(serviceManagement, userCredentials)
        {
            this._proxyManager = new AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService>(this);
        }

        protected override void AuthenticateCore()
        {
            this._proxyManager.PrepareCredentials();
            base.AuthenticateCore();
        }

        protected override void ValidateAuthentication()
        {
            this._proxyManager.RenewTokenIfRequired();
            base.ValidateAuthentication();
        }
    }

    /// <summary>
    /// Class that wraps acquiring the security token for a service
    /// </summary>
    public sealed class AutoRefreshSecurityToken<TProxy, TService>
        where TProxy : ServiceProxy<TService>
        where TService : class
    {
        private ClientCredentials _deviceCredentials;
        private TProxy _proxy;

        /// <summary>
        /// Instantiates an instance of the proxy class
        /// </summary>
        /// <param name="proxy">Proxy that will be used to authenticate the user</param>
        public AutoRefreshSecurityToken(TProxy proxy)
        {
            if (null == proxy)
            {
                throw new ArgumentNullException("proxy");
            }

            this._proxy = proxy;
        }

        /// <summary>
        /// Prepares authentication before authen6ticated
        /// </summary>
        public void PrepareCredentials()
        {
            if (null == this._proxy.ClientCredentials)
            {
                return;
            }

            switch (this._proxy.ServiceConfiguration.AuthenticationType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    this._proxy.ClientCredentials.UserName.UserName = null;
                    this._proxy.ClientCredentials.UserName.Password = null;
                    break;
                case AuthenticationProviderType.Federation:
                case AuthenticationProviderType.LiveId:
                    this._proxy.ClientCredentials.Windows.ClientCredential = null;
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Renews the token (if it is near expiration or has expired)
        /// </summary>
        public void RenewTokenIfRequired()
        {
            if (null != this._proxy.SecurityTokenResponse &&
                DateTime.UtcNow.AddMinutes(15) >= this._proxy.SecurityTokenResponse.Response.Lifetime.Expires)
            {
                try
                {
                    this._proxy.Authenticate();
                }
                catch (CommunicationException)
                {
                    if (null == this._proxy.SecurityTokenResponse ||
                        DateTime.UtcNow >= this._proxy.SecurityTokenResponse.Response.Lifetime.Expires)
                    {
                        throw;
                    }

                    // Ignore the exception 
                }
            }
        }
    }
    #endregion

}
