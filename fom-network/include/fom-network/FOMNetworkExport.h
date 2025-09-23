#pragma once

#if defined(_WIN32) || defined(_WIN64)
#define FOM_API __declspec(dllexport)
#else
#define FOM_API __attribute__((visibility("default")))
#endif
