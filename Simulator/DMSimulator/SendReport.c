#include "SendReport.h"

static IOTHUB_CLIENT_LL_HANDLE _iotHubClientHandle = NULL;

static void ReportedStateCallback(int status_code, void* userContextCallback)
{
	(void)userContextCallback;

	printf("\r\nReported state changed\r\n");
	printf("Status code: %d\r\n", status_code);
}

void SetupSendReport(IOTHUB_CLIENT_LL_HANDLE iotHubClientHandle)
{
	_iotHubClientHandle = iotHubClientHandle;
}

void SendReport(unsigned char* report, size_t size)
{
	if (_iotHubClientHandle == NULL)
	{
		printf("Failed to send report. IotHubClientHandle has not been set");
		return;
	}

	IoTHubClient_LL_SendReportedState(_iotHubClientHandle, report, size, ReportedStateCallback, NULL);
	printf("Sent report: %.*s\r\n", size, report);
}