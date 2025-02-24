#include "pch.h"

struct cmp_str
{
	bool operator()(char const* a, char const* b) const
	{
		return std::strcmp(a, b) < 0;
	}
};


void* Core::GetPointerForImport(LPCWSTR moduleName, const char* funcName)
{
	HMODULE a = LoadLibraryW(moduleName);
	if (a == NULL) {
		return nullptr;
	}
	FARPROC pyObjCallPointer = GetProcAddress(a, funcName);
	if (pyObjCallPointer == NULL) {
		return nullptr;
	}
	return (void*)pyObjCallPointer;
}


Core::Core()
{

	
}


Core::~Core()
{

}


Core& Core::CoreInst() {
	static Core core;
	return core;
}


