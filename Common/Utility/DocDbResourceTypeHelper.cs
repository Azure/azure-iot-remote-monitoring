using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility
{
    public static class DocDbResourceTypeHelper
    {
        public static string GetResourceTypeString(DocDbResourceType type)
        {
            switch (type)
            {
                case DocDbResourceType.Database:
                    return "dbs";
                case DocDbResourceType.Collection:
                    return "colls";
                case DocDbResourceType.Document:
                    return "docs";
                default:
                    throw new InvalidOperationException("Unknown DocDbResourceType");
            }
        }

        public static string GetResultSetKey(DocDbResourceType type)
        {
            switch (type)
            {
                case DocDbResourceType.Database:
                    return "Databases";
                case DocDbResourceType.Collection:
                    return "DocumentCollections";
                case DocDbResourceType.Document:
                    return "Documents";
                default:
                    throw new InvalidOperationException("Unknown DocDbResourceType");
            }
        }
    }
}
