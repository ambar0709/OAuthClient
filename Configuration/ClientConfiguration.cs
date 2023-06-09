﻿namespace Extoms.OAuth.Configuration
{
    /// <summary>
    /// Runtime client configuration. 
    /// Use instead of <see cref="ClientConfiguration"/> when you need manually create client configuration in runtime.
    /// </summary>
    public class ClientConfiguration : IClientConfiguration
    {
        /// <summary>
        /// Client ID (ID of your application).
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Scope - contains set of permissions which user should give to your application.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Redirect URI (URI user will be redirected to
        /// after authentication using third-party service).
        /// </summary>
        public string RedirectUri { get; set; }
    }
}