#include "..\Utilities.h"
#include "FirmwareUpdate.h"
#include "ConfigurationUpdate.h"
#include "DMTaskBase.h"
#include "DMTaskEngine.h"

static DMTaskState _currentState = DM_IDLE;
static time_t _lastStateUpdateTime = 0;
static DMTaskStep* _steps = NULL;
static OnEnterStateProc _onEnterStateProc = NULL;
static OnLeaveStateProc _onLeaveStateProc = NULL;

// Method: firmwareUpdate(fwPackageUri)
static int OnMethodFirmwareUpdate(const unsigned char* payload, size_t size, unsigned char** response, size_t* resp_size)
{
	if (_currentState != DM_IDLE)
	{
		AllocAndPrintf(response, resp_size, "Device is busy", size, payload);
		return 409;
	}

	char* uri = GetTopLevelNodeValue((const char*)payload, size, "FwPackageUri");

	int status;
	if (uri == NULL || !BeginFirmwareUpdate(uri, &_steps, &_onEnterStateProc, &_onLeaveStateProc))
	{
		_currentState = DM_IDLE;
		AllocAndPrintf(response, resp_size, "Bad parameter: %.*s", size, payload);
		status = 400;
	}
	else
	{
		_currentState = _steps->CurrentState;
		time(&_lastStateUpdateTime);
		AllocAndPrintf(response, resp_size, "Firmware updating accepted, uri = %s", uri);
		status = 200;
	}

	free(uri);
	return status;
}

// Method: configurationUpdate(configUri)
static int OnMethodConfigurationUpdate(const unsigned char* payload, size_t size, unsigned char** response, size_t* resp_size)
{
	if (_currentState != DM_IDLE)
	{
		AllocAndPrintf(response, resp_size, "Device is busy", size, payload);
		return 409;
	}

	char* uri = GetTopLevelNodeValue((const char*)payload, size, "ConfigUri");

	int status;
	if (uri == NULL || !BeginConfigurationUpdate(uri, &_steps, &_onEnterStateProc, &_onLeaveStateProc))
	{
		_currentState = DM_IDLE;
		AllocAndPrintf(response, resp_size, "Bad parameter: %.*s", size, payload);
		status = 400;
	}
	else
	{
		_currentState = _steps->CurrentState;
		time(&_lastStateUpdateTime);
		AllocAndPrintf(response, resp_size, "Configuration updating accepted, uri = %s", uri);
		status = 200;
	}

	free(uri);
	return status;
}

// Method: ChangeDeviceState(...)
static int OnMethodChangeDeviceState(const unsigned char* payload, size_t size, unsigned char** response, size_t* resp_size)
{
	// Return the whole parameter as output
	*resp_size = size;
	*response = malloc(*resp_size);
	memcpy(*response, payload, size);

	_steps = NULL;
	_currentState = DM_IDLE;
	return 200;
}

int OnDeviceMethod(const char* method_name, const unsigned char* payload, size_t size, unsigned char** response, size_t* resp_size)
{
	if (strcmp(method_name, "FirmwareUpdate") == 0)
	{
		return OnMethodFirmwareUpdate(payload, size, response, resp_size);
	}
	else if (strcmp(method_name, "ConfigurationUpdate") == 0)
	{
		return OnMethodConfigurationUpdate(payload, size, response, resp_size);
	}
	else if (strcmp(method_name, "ChangeDeviceState") == 0)
	{
		return OnMethodChangeDeviceState(payload, size, response, resp_size);
	}
	else
	{
		return -1;
	}
}

void StepDMTask()
{
	if (_currentState == DM_IDLE)
	{
		return;
	}

	time_t now;
	time(&now);

	for (DMTaskStep* p = _steps; p->CurrentState != DM_NULL; p++)
	{
		if (p->CurrentState == _currentState)
		{
			if (now >= _lastStateUpdateTime + p->ExecuteTimeInSeconds)
			{
				// Switch state according to the graph
				_onLeaveStateProc(_currentState, now);

				_currentState = p->NextState;
				_lastStateUpdateTime = now;

				BOOL succeeded = _onEnterStateProc(_currentState, now);
				if (!succeeded)
				{
					_currentState = DM_IDLE;
				}
			}

			break;
		}
	}
}