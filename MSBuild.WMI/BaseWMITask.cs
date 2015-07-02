using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace MSBuild.WMI
{
    /// <summary>
    /// This class will be used as a base class for all WMI MSBuild tasks.
    /// Contains logic for basic WMI operations as well as some basic properties (connection information, actual task action).
    /// </summary>
    public abstract class BaseWMITask : Task
    {
        #region Private Fields

        private ManagementScope _scope;

        #endregion

        #region Public Properties

        /// <summary>
        /// IP or host name of remote machine or "localhost"
        /// If not set - treated as "localhost"
        /// </summary>
        public string Machine { get; set; }

        /// <summary>
        /// Username for connecting to remote machine
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password for connecting to remote machine
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Specific action to be executed (Start, Stop, etc.)
        /// </summary>
        public string TaskAction { get; set; }

        #endregion

        #region Protected Members

        /// <summary>
        /// Gets WMI ManagementScope object
        /// </summary>
        protected ManagementScope WMIScope
        {
            get
            {
                if (_scope != null)
                    return _scope;

                var wmiScopePath = string.Format(@"\\{0}\root\WebAdministration", Machine);

                //we should pass user as HOST\\USER
                var wmiUserName = UserName;
                if (wmiUserName != null && !wmiUserName.Contains("\\"))
                    wmiUserName = string.Concat(Machine, "\\", UserName);

                var wmiConnectionOptions = new ConnectionOptions()
                {
                    Username = wmiUserName,
                    Password = Password,
                    Impersonation = ImpersonationLevel.Impersonate,
                    Authentication = AuthenticationLevel.PacketPrivacy,
                    EnablePrivileges = true
                };

                //use current user if this is a local machine
                if (Helpers.IsLocalHost(Machine))
                {
                    wmiConnectionOptions.Username = null;
                    wmiConnectionOptions.Password = null;
                }

                _scope = new ManagementScope(wmiScopePath, wmiConnectionOptions);
                _scope.Connect();

                return _scope;
            }
        }

        #endregion
    }
}
