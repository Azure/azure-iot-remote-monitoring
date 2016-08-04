using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Ploeh.AutoFixture;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.TestStubs
{
    public class AlertHistoryBlobReaderStub : IBlobStorageReader
    {
        private readonly Fixture fixture;
        private readonly int _year;
        private readonly int _month;
        private readonly int _date;
        private readonly string _value;

        public AlertHistoryBlobReaderStub(int year, int month, int date, string value)
        {
            fixture = new Fixture();
            _year = year;
            _month = month;
            _date = date;
            _value = value;
        }

        public IEnumerator<BlobContents> GetEnumerator()
        {
            fixture.Customize<AlertHistoryItemModel>(
                ob =>
                    ob.With(x => x.Timestamp, new DateTime(_year, _month, _date))
                        .With(x => x.Value, _value.ToString()));
            var alertItems = fixture.CreateMany<AlertHistoryItemModel>();
            var blobData = AlertsRepository.DEVICE_ID_COLUMN_NAME + "," + AlertsRepository.READING_VALUE_COLUMN_NAME +
                          "," + AlertsRepository.RULE_OUTPUT_COLUMN_NAME + "," + AlertsRepository.TIME_COLUMN_NAME +
                          Environment.NewLine;
            blobData = alertItems.Aggregate(blobData,
                (current, item) =>
                    current +
                    (item.DeviceId + "," + item.Value + "," + item.RuleOutput + "," + item.Timestamp +
                     Environment.NewLine));

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            var blobContents = new BlobContents() { Data = stream, LastModifiedTime = DateTime.Now };
            yield return blobContents;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}