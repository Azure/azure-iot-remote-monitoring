#include "../SendReport.h"
#include "../Utilities.h"
#include "ConfigurationUpdate.h"

// State switch graph: pending -> downloading -> applying -> idle
static DMTaskStep _steps[] =
{
	{ CU_PENDING, 0, CU_DOWNLOADING },
	{ CU_DOWNLOADING, 10, CU_APPLYING },
	{ CU_APPLYING, 10, DM_IDLE },
	{ DM_NULL, 0, DM_NULL }
};

static char* _version;
static time_t _downloadStartTime = 0;
static time_t _applyStartTime = 0;

static BOOL OnEnterState(DMTaskState state, time_t now)
{
	BOOL succeeded = TRUE;

	unsigned char* report = NULL;
	size_t size = 0;

	switch (state)
	{
	case CU_PENDING:
		// No report for entering pending state
		break;

	case CU_DOWNLOADING:
		_downloadStartTime = now;

		if (strcmp(_version, "downloadFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'configurationUpdate': { 'status': 'download failed', 'log': 'download failed' } } }");
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'configurationUpdate': { 'status': 'downloading', 'log': 'downloading' } } }");
		}
		break;

	case CU_APPLYING:
		_applyStartTime = 0;

		if (strcmp(_version, "applyFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'configurationUpdate': { 'status': 'apply failed', 'log': 'downloaded(%I64ds) -> apply failed' } } }",
				now - _downloadStartTime);
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'configurationUpdate': { 'status': 'applying', 'log': 'downloaded(%I64ds) -> applying' } } }",
				now - _downloadStartTime);
		}
		break;

	case DM_IDLE:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'configurationUpdate': { 'status': 'updated to %s', 'log': 'downloaded(%I64ds) -> applied(%I64ds)' } }, 'ConfigurationVersion': '%s' }",
			_version,
			_applyStartTime - _downloadStartTime,
			now - _applyStartTime,
			_version);
		break;

	default:
		printf("Unknown ConfigurationUpdateState %d", state);
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

	case CU_PENDING:
		// No report for leaving pending state
		break;

	case CU_DOWNLOADING:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'configurationUpdate': { 'status': 'download completed', 'log': 'downloaded(%I64ds)' } } }",
			now - _downloadStartTime);
		break;

	case CU_APPLYING:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'configurationUpdate': { 'status': 'apply completed', 'log': 'downloaded(%I64ds) -> applied(%I64ds)' } } }",
			_applyStartTime - _downloadStartTime,
			now - _applyStartTime);
		break;

	default:
		printf("Unknown ConfigurationUpdateState %d", state);
		break;
	}

	if (report != NULL)
	{
		SendReport(report, size);
	}
}

BOOL BeginConfigurationUpdate(const char* uri, DMTaskStep** steps, OnEnterStateProc* onEnterStateProc, OnLeaveStateProc* onLeaveStateProc)
{
	// [WORKAROUND] Directly pick the version from the URI
	const char* prefix = "/configuration/";
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