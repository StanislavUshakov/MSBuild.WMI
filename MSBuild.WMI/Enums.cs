﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSBuild.WMI
{
    /// <summary>
    /// Enum represents possible Managed Pipeline Modes for IIS Application Pool
    /// </summary>
    public enum ManagedPipelineMode
    {
        Integrated = 0,
        Classic = 1
    }

    /// <summary>
    /// Enum represents possible task actions
    /// </summary>
    public enum TaskAction
    {
        CheckExists = 0,
        Create = 1,
        Start = 2,
        Stop = 3
    }
}
