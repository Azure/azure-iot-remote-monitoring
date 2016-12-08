#include "../Utilities.h"
#include "../SendReport.h"
#include "FirmwareUpdate.h"

// State switch graph: pending -> downloading -> applying -> rebooting -> idle
static DMTaskStep _steps[] =
{
	{ FU_PENDING, 0, FU_DOWNLOADING },
	{ FU_DOWNLOADING, 10, FU_APPLYING },
	{ FU_APPLYING, 10, FU_REBOOTING },
	{ FU_REBOOTING, 10, DM_IDLE },
	{ DM_NULL, 0, DM_NULL }
};

static char* _version = NULL;
static time_t _downloadStartTime = 0;
static time_t _applyStartTime = 0;
static time_t _rebootStartTime = 0;

static BOOL OnEnterState(DMTaskState state, time_t now)
{
	BOOL succeeded = TRUE;

	unsigned char* report = NULL;
	size_t size = 0;

	switch (state)
	{
	case FU_PENDING:
		// No report for entering pending state
		break;

	case FU_DOWNLOADING:
		_downloadStartTime = now;

		if (strcmp(_version, "downloadFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'download failed', 'log': 'download failed' } } }");
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'downloading', 'log' : 'downloading' } } }");
		}
		break;

	case FU_APPLYING:
		_applyStartTime = now;

		if (strcmp(_version, "applyFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'apply failed', 'log': 'downloaded(%I64ds) -> apply failed' } } }",
				now - _downloadStartTime);
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'applying', 'log': 'downloaded(%I64ds) -> applying' } } }",
				now - _downloadStartTime);
		}
		break;

	case FU_REBOOTING:
		_rebootStartTime = now;

		if (strcmp(_version, "rebootFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'reboot failed', 'log': 'downloaded(%I64ds) -> applied(%I64ds) -> reboot failed' } } }",
				_applyStartTime - _downloadStartTime,
				now - _applyStartTime);
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'rebooting', 'log': 'downloaded(%I64ds) -> applied(%I64ds) -> rebooting' } } }",
				_applyStartTime - _downloadStartTime,
				now - _applyStartTime);
		}
		break;

	case DM_IDLE:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'updated to %s', 'log' : 'downloaded(%I64ds) -> applied(%I64ds) -> rebooted(%I64ds)' } }, 'FirmwareVersion' : '%s' }",
			_version,
			_applyStartTime - _downloadStartTime,
			_rebootStartTime - _applyStartTime,
			now - _rebootStartTime,
			_version);
		break;

	default:
		printf("Unknown FirmwareUpdateState %d", state);
		break;
	}

	if (report != NULL)
	{
		SendReport(report, size);
	}

	return succeeded;
}

static void OnLeaveState(DMTaskState state, time_t now)
{
	unsigned char* report = NULL;
	size_t size = 0;

	switch (state)
	{
	case DM_IDLE:
		// No report for leaving idle state
		break;

	case FU_PENDING:
		// No report for leaving pending state
		break;

	case FU_DOWNLOADING:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'download completed', 'log': 'downloaded(%I64ds)' } } }",
			now - _downloadStartTime);
		break;

	case FU_APPLYING:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'apply completed', 'log': 'downloaded(%I64ds) -> applied(%I64ds)' } } }",
			_applyStartTime - _downloadStartTime,
			now - _applyStartTime);
		break;

	case FU_REBOOTING:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'reboot completed', 'log': 'downloaded(%I64ds) -> applied(%I64ds) -> rebooted(%I64ds)' } } }",
			_applyStartTime - _downloadStartTime,
			_rebootStartTime - _applyStartTime,
			now - _rebootStartTime);
		break;

	default:
		printf("Unknown FirmwareUpdateState %d", state);
		break;
	}

	if (report != NULL)
	{
		SendReport(report, size);
	}
}

BOOL BeginFirmwareUpdate(const char* uri, DMTaskStep** steps, OnEnterStateProc* onEnterStateProc, OnLeaveStateProc* onLeaveStateProc)
{
	// [WORKAROUND] Directly pick the version from the URI
	const char* prefix = "/firmware/";
	char* p = strstr(uri, prefix);
	if (p == NULL)
	{
		return FALSE;
	}
	p += strlen(prefix);

	free(_version);
	_version = _strdup(p);

	*steps = _steps;
	*onEnterStateProc = OnEnterState;
	*onLeaveStateProc = OnLeaveState;
	return TRUE;
}