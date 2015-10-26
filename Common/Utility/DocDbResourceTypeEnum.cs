using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility
{
    class DocDbResourceTypeEnum
    {
        public static readonly DocDbResourceTypeEnum DATABASES = new DocDbResourceTypeEnum(DATABASE_RESOURCE_TYPE, DATABASE_RESULT_SET_KEY);
        public static readonly DocDbResourceTypeEnum COLLECTIONS = new DocDbResourceTypeEnum(COLLECTION_RESOURCE_TYPE, COLLECTION_RESULT_SET_KEY);
        public static readonly DocDbResourceTypeEnum DOCUMENTS = new DocDbResourceTypeEnum(DOCUMENTS_RESOURCE_TYPE, DOCUMENTS_RESULT_SET_KEY);

        public static IEnumerable<DocDbResourceTypeEnum> VALUES
        {
            get
            {
                yield return DATABASES;
                yield return COLLECTIONS;
                yield return DOCUMENTS;
            }
        }

        private const string DATABASE_RESOURCE_TYPE = "dbs";
        private const string DATABASE_RESULT_SET_KEY = "Databases";
        private const string COLLECTION_RESOURCE_TYPE = "colls";
        private const string COLLECTION_RESULT_SET_KEY = "DocumentCollections";
        private const string DOCUMENTS_RESOURCE_TYPE = "docs";
        private const string DOCUMENTS_RESULT_SET_KEY = "Documents";

        private readonly string _resourceType;
        private readonly string _resultSetKey;

        private DocDbResourceTypeEnum(string resourceType, string resultSetKey)
        {
            _resourceType = resourceType;
            _resultSetKey = resultSetKey;
        }

        public string QueryResourceType { get { return _resourceType; } }
        public string ResultSetResponseKey { get { return _resultSetKey; } }
    }
}
