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
		if (strcmp(_version, "downloadFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'download failed', 'downloadFailTime': '%s' } } }",
				FormatTime(&now));
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'downloading', 'downloadStartTime': '%s' } } }",
				FormatTime(&now));
		}
		break;

	case FU_APPLYING:
		if (strcmp(_version, "applyFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'apply failed', 'applyFailTime': '%s' } } }",
				FormatTime(&now));
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'applying', 'applyStartTime': '%s' } } }",
				FormatTime(&now));
		}
		break;

	case FU_REBOOTING:
		if (strcmp(_version, "rebootFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'reboot failed', 'rebootFailTime': '%s' } } }",
				FormatTime(&now));
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'rebooting', 'rebootStartTime': '%s' } } }",
				FormatTime(&now));
		}
		break;

	case DM_IDLE:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'idle', 'lastFirmwareUpdateTime': '%s' } }, 'FirmwareVersion': '%s' }",
			FormatTime(&now),
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
			"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'download completed', 'downloadCompleteTime': '%s' } } }",
			FormatTime(&now));
		break;

	case FU_APPLYING:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'apply completed', 'applyCompleteTime': '%s' } } }",
			FormatTime(&now));
		break;

	case FU_REBOOTING:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'firmwareUpdate': { 'status': 'reboot completed', 'rebootCompleteTime': '%s' } } }",
			FormatTime(&now));
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