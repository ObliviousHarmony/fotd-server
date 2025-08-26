#pragma once

#if defined(_WIN32) || defined(_WIN64)
#define FOMINTERFACE_API __declspec(dllexport)
#else
#define FOMINTERFACE_API __attribute__((visibility("default")))
#endif
