#pragma once

#include <windows.h>

/*
Find matching device method, then kick-off the asynchornize task

Parameters
	method_name: device method name
	payload: device method parameters in JSON
	size: length of the payload
	response: device method returned message
	resp_size: length of the message

Return
	The status code if matching method found, otherwise -1
*/
int OnDeviceMethod(const char* method_name, const unsigned char* payload, size_t size, unsigned char** response, size_t* resp_size);

/*
Run single step of the current task
*/
void StepDMTask();