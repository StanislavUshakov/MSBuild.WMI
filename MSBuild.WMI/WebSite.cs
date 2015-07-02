using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSBuild.WMI
{
    /// <summary>
    /// This class is used for operations with IIS Web Site.
    /// Possible actions:
    ///   "CheckExists" - check if the web site with the name specified in "SiteName" exists, result is accessible through field "Exists"
    ///   "Create" - create a web site with the name specified in "SiteName"
    ///   "Start" = starts web site
    ///   "Stop" - stops web site
    /// Note: bindings with hostnames, IPs should be done manually, site will be created only with custom (specified) port.
    /// </summary>
    public class WebSite : BaseWMITask
    {
        #region Public Properties

        /// <summary>
        /// Web Site name
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Web Site physical path (not a UNC path)
        /// </summary>
        public string PhysicalPath { get; set; }

        /// <summary>
        /// Port (it's better if it's custom)
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// Name of the Application Pool that will be used for this Web Site
        /// </summary>
        public string AppPoolName { get; set; }

        [Output]
        public bool Exists { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the task
        /// </summary>
        /// <returns>True, is task has been executed successfully; False - otherwise</returns>
        public override bool Execute()
        {
            try
            {
                Log.LogMessage("WebSite task, action = {0}", Action);
                switch (Action)
                {
                    case WMI.TaskAction.CheckExists:
                        Exists = GetWebSite() != null;
                        break;

                    case WMI.TaskAction.Create:
                        CreateWebSite();
                        break;

                    case WMI.TaskAction.Start:
                        StartWebSite();
                        break;

                    case WMI.TaskAction.Stop:
                        StopWebSite();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            //WMI tasks are execute asynchronously, wait to completing
            Thread.Sleep(1000);

            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates web site with the specified name and port. Bindings must be confgiured after manually.
        /// </summary>
        private void CreateWebSite()
        {
            var path = new ManagementPath(@"BindingElement");
            var mgmtClass = new ManagementClass(WMIScope, path, null);

            var binding = mgmtClass.CreateInstance();

            binding["BindingInformation"] = ":" + Port + ":";
            binding["Protocol"] = "http";

            path = new ManagementPath(@"Site");
            mgmtClass = new ManagementClass(WMIScope, path, null);

            // Obtain in-parameters for the method
            var inParams = mgmtClass.GetMethodParameters("Create");

            // Add the input parameters.
            inParams["Bindings"] = new ManagementBaseObject[] { binding };
            inParams["Name"] = SiteName;
            inParams["PhysicalPath"] = PhysicalPath;
            inParams["ServerAutoStart"] = true;

            // Execute the method and obtain the return values.
            mgmtClass.InvokeMethod("Create", inParams, null);

            WaitTill(() => GetApp("/") != null);
            var rootApp = GetApp("/");

            rootApp["ApplicationPool"] = AppPoolName;
            rootApp.Put();
        }

        /// <summary>
        /// Gets Web Site by name
        /// </summary>
        /// <returns>ManagementObject representing Web Site or null</returns>
        private ManagementObject GetWebSite()
        {
            return GetObjectByQuery(string.Format("select * from Site where Name = '{0}'", SiteName));
        }

        /// <summary>
        /// Get Virtual Application by path 
        /// </summary>
        /// <param name="path">Path of virtual application (if path == "/" - gets root application)</param>
        /// <returns>ManagementObject representing Virtual Application or null</returns>
        private ManagementObject GetApp(string path)
        {
            return GetObjectByQuery(string.Format("select * from Application where SiteName = '{0}' and Path='{1}'", SiteName, path));
        }

        /// <summary>
        /// Stop Web Site
        /// </summary>
        private void StopWebSite()
        {
            GetWebSite().InvokeMethod("Stop", null);
        }

        /// <summary>
        /// Start Web Site
        /// </summary>
        private void StartWebSite()
        {
            GetWebSite().InvokeMethod("Start", null);
        }

        #endregion
    }
}
