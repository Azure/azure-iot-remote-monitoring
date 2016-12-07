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
		if (strcmp(_version, "downloadFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'configurationUpdate': { 'status': 'download failed', 'downloadFailTime': '%s' } } }",
				FormatTime(&now));
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'configurationUpdate': { 'status': 'downloading', 'downloadStartTime': '%s' } } }",
				FormatTime(&now));
		}
		break;

	case CU_APPLYING:
		if (strcmp(_version, "applyFail") == 0)
		{
			succeeded = FALSE;

			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'configurationUpdate': { 'status': 'apply failed', 'applyFailTime': '%s' } } }",
				FormatTime(&now));
		}
		else
		{
			AllocAndPrintf(
				&report,
				&size,
				"{ 'iothubDM': { 'configurationUpdate': { 'status': 'applying', 'applyStartTime': '%s' } } }",
				FormatTime(&now));
		}
		break;

	case DM_IDLE:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'configurationUpdate': { 'status': 'idle', 'lastConfigurationUpdateTime': '%s' } }, 'ConfigurationVersion': '%s' }",
			FormatTime(&now),
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
			"{ 'iothubDM': { 'configurationUpdate': { 'status': 'download completed', 'downloadCompleteTime': '%s' } } }",
			FormatTime(&now));
		break;

	case CU_APPLYING:
		AllocAndPrintf(
			&report,
			&size,
			"{ 'iothubDM': { 'configurationUpdate': { 'status': 'apply completed', 'applyCompleteTime': '%s' } } }",
			FormatTime(&now));
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