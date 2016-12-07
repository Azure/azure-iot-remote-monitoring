#pragma once

#include <time.h>

typedef enum _DMTaskState
{
	DM_NULL,
	DM_IDLE,

	FU_PENDING,
	FU_DOWNLOADING,
	FU_APPLYING,
	FU_REBOOTING,

	CU_PENDING,
	CU_DOWNLOADING,
	CU_APPLYING
} DMTaskState;

typedef struct _DMTaskStep
{
	DMTaskState CurrentState;
	time_t ExecuteTimeInSeconds;
	DMTaskState NextState;
} DMTaskStep;

typedef BOOL(*OnEnterStateProc)(DMTaskState, time_t);
typedef void(*OnLeaveStateProc)(DMTaskState, time_t);