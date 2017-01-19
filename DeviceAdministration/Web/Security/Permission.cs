namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security
{
    /// <summary>
    /// Defines various permissions for users (of both the web site and
    /// the REST API).
    /// 
    /// Permissions are assigned to roles, and users are assigned to roles.
    /// If a user has multiple roles, only one role needs to have a 
    /// permission for the user to have the permission.
    /// </summary>
    public enum Permission
    {
        ViewDevices,
        EditDeviceMetadata,
        AddDevices,
        RemoveDevices,
        DisableEnableDevices,
        SendCommandToDevices,
        ViewDeviceSecurityKeys,
        ViewActions,
        AssignAction,
        ViewRules,
        EditRules,
        DeleteRules,
        ViewTelemetry,
        HealthBeat,
        LogicApps,
        CellularConn,
        ViewJobs,
        ManageJobs,
        SaveDeviceListColumnsAsGlobal,
        DeleteSuggestedClauses
    }
}
