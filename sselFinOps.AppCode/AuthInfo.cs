using LNF.Data;
using System;

namespace sselFinOps.AppCode
{
    [Obsolete]
    public class AuthInfo
    {
        public string Location { get; private set; }
        public bool ShowButton { get; private set; }
        public ClientPrivilege AuthTypes { get; private set; }

        private AuthInfo() { }

        public static AuthInfo Create(string location, bool showButton, ClientPrivilege authTypes)
        {
            return new AuthInfo()
            {
                Location = location,
                AuthTypes = authTypes,
                ShowButton = showButton
            };
        }
    }
}
