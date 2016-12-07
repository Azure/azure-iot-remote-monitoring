#pragma once

#include "iothub_client.h"

void SetupSendReport(IOTHUB_CLIENT_LL_HANDLE iotHubClientHandle);
void SendReport(unsigned char* report, size_t size);