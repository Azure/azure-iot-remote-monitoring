#pragma once

#include "DMTaskBase.h"

BOOL BeginFirmwareUpdate(const char* uri, DMTaskStep** steps, OnEnterStateProc* onEnterStateProc, OnLeaveStateProc* onLeaveStateProc);