// ---------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Bundling
{
    using System.IO;
    using System.Web.Optimization;
    using dotless.Core;

    public sealed class LessTransform : IBundleTransform
    {
        private readonly string path;

        public LessTransform(string path)
        {
            this.path = path;
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            var oldPath = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(this.path);

            response.Content = Less.Parse(response.Content);
            Directory.SetCurrentDirectory(oldPath);
            response.ContentType = "text/css";
        }
    }
}