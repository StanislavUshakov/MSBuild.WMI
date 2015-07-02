﻿using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

namespace MSBuild.WMI
{
    /// <summary>
    /// This class is used for operations with IIS ApplicationPool.
    /// Possible actions:
    ///   "CheckExists" - check if the pool with the name specified in "AppPoolName" exists, result is accessible through field "Exists"
    ///   "Create" - create an application pool with the name specified in "AppPoolName"
    /// </summary>
    public class AppPool : BaseWMITask
    {
        #region Public Properties

        public string AppPoolName { get; set; }

        [Output]
        public bool Exists { get; set; }

        #endregion

        #region Public Methods

        public override bool Execute()
        {
            try
            {
                Log.LogMessage("AppPool task, action = {0}", Action);
                switch (Action)
                {
                    case WMI.TaskAction.CheckExists:
                        Exists = GetAppPool() != null;
                        break;

                    case WMI.TaskAction.Create:
                        CreateAppPool();
                        break;

                    case WMI.TaskAction.Start:
                        StartAppPool();
                        break;

                    case WMI.TaskAction.Stop:
                        StopAppPool();
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
        /// Gets ApplicationPool with name AppPoolName
        /// </summary>
        /// <returns>ManagementObject representing ApplicationPool or null</returns>
        private ManagementObject GetAppPool()
        {
            return GetObjectByQuery(string.Format("select * from ApplicationPool where Name = '{0}'", AppPoolName));
        }

        /// <summary>
        /// Creates ApplicationPool with name AppPoolName, Integrated pipeline mode and ApplicationPoolIdentity (default)
        /// Calling code (MSBuild script) must first call CheckExists, in this method there's no checks
        /// </summary>
        private void CreateAppPool()
        {
            var path = new ManagementPath(@"ApplicationPool");
            var mgmtClass = new ManagementClass(WMIScope, path, null);

            //obtain in-parameters for the method
            var inParams = mgmtClass.GetMethodParameters("Create");

            //add the input parameters.
            inParams["AutoStart"] = true;
            inParams["Name"] = AppPoolName;

            //execute the method and obtain the return values.
            mgmtClass.InvokeMethod("Create", inParams, null);

            //wait till pool is created
            var appPool = GetAppPool();
            while (appPool == null)
            {
                System.Threading.Thread.Sleep(250);
                appPool = GetAppPool();
            }

            //set pipeline mode (default is Classic)
            appPool["ManagedPipelineMode"] = (int)ManagedPipelineMode.Integrated;
            appPool.Put();
        }

        /// <summary>
        /// Starts Application Pool
        /// </summary>
        private void StartAppPool()
        {
            GetAppPool().InvokeMethod("Start", null);
        }

        /// <summary>
        /// Stops Application Pool
        /// </summary>
        private void StopAppPool()
        {
            GetAppPool().InvokeMethod("Stop", null);
        }

        #endregion
    }
}