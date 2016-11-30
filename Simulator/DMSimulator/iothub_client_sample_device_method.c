// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include <stdio.h>
#include <stdlib.h>
#include <windows.h>
#include <TlHelp32.h>

#include "iothub_client.h"
#include "iothub_message.h"
#include "azure_c_shared_utility/threadapi.h"
#include "azure_c_shared_utility/crt_abstractions.h"
#include "azure_c_shared_utility/platform.h"
#include "iothubtransportmqtt.h"

#ifdef MBED_BUILD_TIMESTAMP
#include "certs.h"
#endif // MBED_BUILD_TIMESTAMP

/*String containing Hostname, Device Id & Device Key in the format:                         */
/*  "HostName=<host_name>;DeviceId=<device_id>;SharedAccessKey=<device_key>"                */
/*  "HostName=<host_name>;DeviceId=<device_id>;SharedAccessSignature=<device_sas_token>"    */

static int callbackCounter;
static char msgText[1024];
static char propText[1024];
static bool g_continueRunning;
#define MESSAGE_COUNT 5
#define DOWORK_LOOP_NUM     3

static char connectionString[1024] = { 0 };
static char reportedProperties[4096] = { 0 };

typedef struct EVENT_INSTANCE_TAG
{
	IOTHUB_MESSAGE_HANDLE messageHandle;
	size_t messageTrackingId;  // For tracking the messages within the user callback.
} EVENT_INSTANCE;

static int DeviceMethodCallback(const char* method_name, const unsigned char* payload, size_t size, unsigned char** response, size_t* resp_size, void* userContextCallback)
{
	(void)userContextCallback;

	printf("\r\nDevice Method called\r\n");
	printf("Device Method name:    %s\r\n", method_name);
	printf("Device Method payload: %.*s\r\n", (int)size, (const char*)payload);

	int status = 200;

	//Response from {method} with parameter {request}
	char* RESPONSE_STRING = "Response from  with parameter ";
	*resp_size = strlen(RESPONSE_STRING) + strlen(method_name) + size;
	*response = malloc(*resp_size + 1);
	sprintf((char*)*response, "Response from %s with parameter %.*s", method_name, size, payload);	

	printf("\r\nResponse status: %d\r\n", status);
	printf("Response payload: %.*s\r\n\r\n", (int)*resp_size, *response);

	callbackCounter++;
	return status;
}

static int DeviceTwinCallback(DEVICE_TWIN_UPDATE_STATE update_state, const unsigned char* payLoad, size_t size, void* userContextCallback)
{
	(void)size;
	(void)userContextCallback;

	strncpy(msgText, (const char*)payLoad, size);

	printf("\r\nDevice Twin changed\r\n");
	printf("Update state: %d\r\n", update_state);
	printf("payLoad: %s\r\n", msgText);

	return 200;
}

static void ReportedStateCallback(int status_code, void* userContextCallback)
{
	(void)userContextCallback;

	printf("\r\nReported state callback\r\n");
	printf("status_code: %d\r\n", status_code);
}

void iothub_client_sample_device_method_run(void)
{
	IOTHUB_CLIENT_LL_HANDLE iotHubClientHandle;

	g_continueRunning = true;

	callbackCounter = 0;
	int receiveContext = 0;

	if (platform_init() != 0)
	{
		(void)printf("Failed to initialize the platform.\r\n");
	}
	else
	{
		if ((iotHubClientHandle = IoTHubClient_LL_CreateFromConnectionString(connectionString, MQTT_Protocol)) == NULL)
		{
			(void)printf("ERROR: iotHubClientHandle is NULL!\r\n");
		}
		else
		{
			bool traceOn = true;
			IoTHubClient_LL_SetOption(iotHubClientHandle, "logtrace", &traceOn);

#ifdef MBED_BUILD_TIMESTAMP
			// For mbed add the certificate information
			if (IoTHubClient_LL_SetOption(iotHubClientHandle, "TrustedCerts", certificates) != IOTHUB_CLIENT_OK)
			{
				printf("failure to set option \"TrustedCerts\"\r\n");
			}
#endif // MBED_BUILD_TIMESTAMP

			// Send reported properties as startup telemetry
			IoTHubClient_LL_SendReportedState(iotHubClientHandle, (const unsigned char*)reportedProperties, strlen(reportedProperties), ReportedStateCallback, NULL);

			if (IoTHubClient_LL_SetDeviceMethodCallback(iotHubClientHandle, DeviceMethodCallback, &receiveContext) != IOTHUB_CLIENT_OK)
			{
				(void)printf("ERROR: IoTHubClient_LL_SetDeviceMethodCallback..........FAILED!\r\n");
			}
			else
			{
				(void)printf("IoTHubClient_LL_SetDeviceMethodCallback...successful.\r\n");

				if (IoTHubClient_LL_SetDeviceTwinCallback(iotHubClientHandle, DeviceTwinCallback, &receiveContext) != IOTHUB_CLIENT_OK)
				{
					(void)printf("ERROR: IoTHubClient_LL_SetDeviceTwinCallback..........FAILED!\r\n");
				}

				size_t iterator = 0;
				do
				{
					IoTHubClient_LL_DoWork(iotHubClientHandle);
					ThreadAPI_Sleep(1);

					iterator++;
				} while (g_continueRunning);

				(void)printf("iothub_client_sample_device_method exited, call DoWork %d more time to complete final sending...\r\n", DOWORK_LOOP_NUM);
				for (size_t index = 0; index < DOWORK_LOOP_NUM; index++)
				{
					IoTHubClient_LL_DoWork(iotHubClientHandle);
					ThreadAPI_Sleep(1);
				}
			}
			IoTHubClient_LL_Destroy(iotHubClientHandle);
	}
		platform_deinit();
}
}

void Usage(void)
{
	printf("Usage: /d:<device connect string> /p:<reported properties in JSON>\r\n");
}

DWORD WINAPI WatchDogThread(LPVOID lpThreadParameter)
{
	(void)lpThreadParameter;

	DWORD currentProcessID = GetCurrentProcessId();
	HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

	DWORD parentProcessID = 0;
	PROCESSENTRY32 processEntry;
	processEntry.dwSize = sizeof(processEntry);

	BOOL bContinue = Process32First(hSnapshot, &processEntry);
	while (bContinue)
	{
		if (processEntry.th32ProcessID == currentProcessID)
		{
			parentProcessID = processEntry.th32ParentProcessID;
			break;
		}

		bContinue = Process32Next(hSnapshot, &processEntry);
	}
	CloseHandle(hSnapshot);

	if (parentProcessID != 0)
	{
		printf("Parent process ID: %d\r\n", parentProcessID);
		HANDLE hParent = OpenProcess(SYNCHRONIZE, FALSE, parentProcessID);

		if (hParent != NULL)
		{
			printf("Opened parent process\r\n");
			if (WaitForSingleObject(hParent, INFINITE) == WAIT_OBJECT_0)
			{
				exit(0);
			}
		}
		else
		{
			printf("Failed to open parent process. GetLastError()=%d\r\n", GetLastError());
		}
	}
	else
	{
		printf("Failed to find parent process\r\n");
	}

	return 0;
}

int main(int argc, const char* argv[])
{
	for (int i = 1; i < argc; i++)
	{
		if (strncmp(argv[i], "/d:", 3) == 0)
		{
			strcpy(connectionString, argv[i] + 3);
		}
		else if (strncmp(argv[i], "/p:", 3) == 0)
		{
			strcpy(reportedProperties, argv[i] + 3);
		}
	}

	if (strlen(connectionString) == 0)
	{
		printf("Missing connection string\r\n");
		Usage();
		return -1;
	}

	if (strlen(reportedProperties) == 0)
	{
		printf("Missing connection string\r\n");
		Usage();
		return -1;
	}

	SetConsoleTitle(connectionString);

	HANDLE hProcess = CreateThread(NULL, 0, WatchDogThread, NULL, 0, NULL);
	CloseHandle(hProcess);

	iothub_client_sample_device_method_run();
	return 0;
}