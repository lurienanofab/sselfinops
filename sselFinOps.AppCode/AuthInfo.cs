using LNF.Models.Data;
using System;

namespace sselFinOps.AppCode
{
    [Obsolete]
    public class AuthInfo
    {
        private string _Location;
        private bool _ShowButton;
        private ClientPrivilege _AuthTypes;

        public string Location { get { return _Location; } }
        public bool ShowButton { get { return _ShowButton; } }
        public ClientPrivilege AuthTypes { get { return _AuthTypes; } }

        private AuthInfo() { }

        public static AuthInfo Create(string location, bool showButton, ClientPrivilege authTypes)
        {
            return new AuthInfo()
            {
                _Location = location,
                _AuthTypes = authTypes,
                _ShowButton = showButton
            };
        }
    }
}
