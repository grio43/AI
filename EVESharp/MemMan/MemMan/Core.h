#pragma once
#include "pch.h"
class Core {

private:
	Core(); // Disallow instantiation outside of the class.
	~Core();

public:

	Core(const Core&) = delete;
	Core& operator=(const Core&) = delete;
	Core(Core&&) = delete;
	Core& operator=(Core&&) = delete;
	static auto CoreInst()->Core&;	
	void* GetPointerForImport(LPCWSTR moduleName, const char* funcName);

	
};
