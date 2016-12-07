#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "jsmn/jsmn.h"
#include "Utilities.h"

#define MAX_TOKENS 32

char* GetTopLevelNodeValue(const char* json, size_t jsonLen, const char* name)
{
	jsmn_parser parser;
	jsmntok_t tokens[MAX_TOKENS];

	jsmn_init(&parser);
	int rst = jsmn_parse(&parser, (char*)json, jsonLen, tokens, MAX_TOKENS);

	int startMin = 0;
	for (jsmntok_t* t = tokens + 1; t < tokens + rst; t++)
	{
		if (t->start <= startMin)
		{
			continue;
		}

		if (t->type != JSMN_STRING)
		{
			return NULL;
		}

		t++;
		if (t >= tokens + rst)
		{
			return NULL;
		}

		if (t->type == JSMN_STRING && strncmp(json + t->start, name, t->size) == 0)
		{
			int tokenLen = t->end - t->start;
			char* buffer = malloc(tokenLen + 1);
			memcpy(buffer, json + t->start, tokenLen);
			buffer[tokenLen] = 0;
			return buffer;
		}

		startMin = t->end;
	}

	return NULL;
}

void AllocAndPrintf(unsigned char** buffer, size_t* size, const char* format, ...)
{
	va_list args;
	va_start(args, format);
	*size = vsnprintf(NULL, 0, format, args);
	va_end(args);

	*buffer = malloc(*size + 1);
	va_start(args, format);
	vsprintf((char*)*buffer, format, args);
	va_end(args);
}

char* FormatTime(time_t* time)
{
	static char buffer[128];

	struct tm* p = gmtime(time);

	sprintf(buffer, "%04d-%02d-%02dT%02d:%02d:%02dZ",
		p->tm_year + 1900,
		p->tm_mon + 1,
		p->tm_mday,
		p->tm_hour,
		p->tm_min,
		p->tm_sec);

	return buffer;
}