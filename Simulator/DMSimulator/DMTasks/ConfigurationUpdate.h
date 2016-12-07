#pragma once

#include "DMTaskBase.h"

BOOL BeginConfigurationUpdate(const char* uri, DMTaskStep** steps, OnEnterStateProc* onEnterStateProc, OnLeaveStateProc* onLeaveStateProc);