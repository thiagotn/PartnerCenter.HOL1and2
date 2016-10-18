namespace PartnerCenter.HOL
{
    using System.Configuration;

    public static class Configuration
    {
        public static string AuthenticationAuthorityEndpoint
        {
            get
            {
                return ConfigurationManager.AppSettings["AuthenticationAuthorityEndpoint"];
            }
        }

        public static string GraphEndpoint
        {
            get
            {
                return ConfigurationManager.AppSettings["GraphEndpoint"];
            }
        }

        public static string CommonDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["CommonDomain"];
            }
        }

        public static string ResourceUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["ResourceUrl"];
            }
        }

        public static string RedirectUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["RedirectUrl"];
            }
        }

        public static string ApplicationId
        {
            get
            {
                return ConfigurationManager.AppSettings["ApplicationId"];
            }
        }

        public static string UserName
        {
            get
            {
                return ConfigurationManager.AppSettings["UserName"];
            }
        }

        public static string Password
        {
            get
            {
                return ConfigurationManager.AppSettings["Password"];
            }
        }
    }
}
