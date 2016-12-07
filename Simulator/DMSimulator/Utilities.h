#pragma once

#include <windows.h>
#include <time.h>

char* GetTopLevelNodeValue(const char* json, size_t jsonLen, const char* name);

void AllocAndPrintf(unsigned char** buffer, size_t* size, const char* format, ...);

char* FormatTime(time_t* time);