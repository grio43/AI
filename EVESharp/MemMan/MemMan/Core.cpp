#include "pch.h"

struct cmp_str
{
	bool operator()(char const* a, char const* b) const
	{
		return std::strcmp(a, b) < 0;
	}
};


typedef void* (__fastcall PyObject_Call)(void* a, void* b, void* c);

typedef void(__stdcall* Callback)(void* a, void* b, void* c);

static PyObject_Call* PyObjectCall = nullptr;
static Utility::Hook* hook = nullptr;

static std::unordered_map<std::string, Callback*> dict;

typedef void* (__cdecl PyObject_GetAttrStringDele)(void* pyObj, const char* attr_name);
static PyObject_GetAttrStringDele* PyObject_GetAttrString = nullptr;

typedef int(__cdecl PyObject_HasAttrStringDele)(void* pyObj, const char* attr_name);
static PyObject_HasAttrStringDele* PyObject_HasAttrString = nullptr;

typedef const char* (__cdecl PyString_AsStringDele)(void* pyObj);
static PyString_AsStringDele* PyString_AsString = nullptr;

typedef void(__cdecl Py_DecRefDele)(void* pyObj);
static Py_DecRefDele* Py_DecRef = nullptr;

void* PyStringStructurePtr = nullptr;

void* PyNoneStruct = nullptr;


static bool IsNull(void* pyObj) {
	return pyObj == nullptr;
}


static bool IsNone(void* pyObj) {
	void* pyType = reinterpret_cast<void*>(*(reinterpret_cast<int64_t*>((static_cast<unsigned char*>(pyObj) + 8))));
	return PyStringStructurePtr == PyNoneStruct;
}

static bool IsPyString(void* pyObj) {

	/*if (IsNull(pyObj) || IsNone(pyObj)) {
		return false;
	}*/

	void* pyType = reinterpret_cast<void*>(*(reinterpret_cast<int64_t*>((static_cast<unsigned char*>(pyObj) + 8))));
	return PyStringStructurePtr == pyType;
}

//auto cntTimeStart = std::chrono::high_resolution_clock::now();

static void* PyHook(void* obj, void* b, void* c)
{
	/*cnt++;
	auto begin = std::chrono::high_resolution_clock::now();*/

	if ((*PyObject_HasAttrString)(obj, "__name__") == 1 && (*PyObject_HasAttrString)(obj, "__module__") == 1)
	{

		auto moduleObj = (*PyObject_GetAttrString)(obj, "__module__");
		if (!IsPyString(moduleObj)) {
			return PyObjectCall(obj, b, c);
		}

		auto moduleName = (*PyString_AsString)(moduleObj);
		(*Py_DecRef)(moduleObj);

		auto nameObj = (*PyObject_GetAttrString)(obj, "__name__");
		if (!IsPyString(nameObj)) {
			return PyObjectCall(obj, b, c);
		}

		auto function = (*PyString_AsString)(nameObj);
		(*Py_DecRef)(nameObj);

		/*char* qualifier = new char[strlen(moduleName) + strlen(function) + 1];
		strcpy(qualifier, moduleName);
		strcat(qualifier, function);*/

		auto qualifier = std::string(moduleName) + "." + std::string(function);

		auto mapIter = dict.find(qualifier);
		if (mapIter != dict.end()) {
			//std::cout << "Proc!" << std::endl;
			auto ptr = mapIter->second;
			((Callback)ptr)(obj, b, c);
		}

		//auto end = std::chrono::high_resolution_clock::now();
		//if (cnt % 10000 == 0)
		//	std::cout << std::chrono::duration_cast<std::chrono::nanoseconds>(end - begin).count() << "ns" << std::endl;

	}

	return PyObjectCall(obj, b, c);
}

extern "C" _declspec(dllexport) int RegisterCallback(const char* moduleName, const char* function, void* ptr);
extern "C" _declspec(dllexport) int UnregisterCallback(const char* moduleName, const char* function);
extern "C" _declspec(dllexport) int UnregisterAllCallbacks();

#pragma optimize( "", off )
extern "C" __declspec(dllexport) void RecvPacket(unsigned char* byteArrayPtr, int length)
{
	// This is to make sure the exported function is not optimized away (merged into the same function/pointer)
	if (length == INT_MAX) {
		printf("RecvPacket: %d\n", length);
		static_cast<void> (1);
	}
	static_cast<void> (0);
	static_cast<void> (0);
	static_cast<void> (0);
	static_cast<void> (0);
	static_cast<void> (0);
	static_cast<void> (0);
	static_cast<void> (0);
}
#pragma optimize( "", on )

#pragma optimize( "", off )
extern "C" __declspec(dllexport) void SendPacket(unsigned char* byteArrayPtr, int length)
{
	// This is to make sure the exported function is not optimized away (merged into the same function/pointer)
	if (length == INT_MAX) {
		printf("SendPacket: %d\n", length);
		static_cast<void> (1);
	}
	static_cast<void> (0);
	static_cast<void> (1);
	static_cast<void> (0);
	static_cast<void> (1);
	static_cast<void> (0);
	static_cast<void> (1);
}
#pragma optimize( "", on )


#pragma optimize( "", off )
extern "C" __declspec(dllexport) void RestartEveSharpCore()
{
	OutputDebugStringA("Restarted core (MemMan)");
	static_cast<void> (0);
	static_cast<void> (1);
	static_cast<void> (0);
	static_cast<void> (1);
	static_cast<void> (0);
	static_cast<void> (1);
}
#pragma optimize( "", on )

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

int UnregisterCallback(const char* moduleName, const char* function)
{
	auto modStr = std::string(moduleName);
	auto funcStr = std::string(function);
	auto combined = modStr + "." + funcStr;
	std::cout << "Unregistered callback with func quant: " << combined << std::endl;
	dict.erase(combined);
	return 0;
}

int RegisterCallback(const char* moduleName, const char* function, void* ptr)
{
	auto modStr = std::string(moduleName);
	auto funcStr = std::string(function);
	auto combined = modStr + "." + funcStr;
	dict[combined] = (Callback*)ptr;
	std::cout << "Registered callback with func quant: " << combined << std::endl;
	//((Callback)ptr)(nullptr, nullptr, nullptr);
	return 0;
}

int UnregisterAllCallbacks() {
	dict.clear();
	return 0;
}

Core::Core()
{

	PyString_AsString = (PyString_AsStringDele*)GetPointerForImport(L"Python27.dll", "PyString_AsString");
	PyObject_GetAttrString = (PyObject_GetAttrStringDele*)GetPointerForImport(L"Python27.dll", "PyObject_GetAttrString");
	PyObject_HasAttrString = (PyObject_HasAttrStringDele*)GetPointerForImport(L"Python27.dll", "PyObject_HasAttrString");
	Py_DecRef = (Py_DecRefDele*)GetPointerForImport(L"Python27.dll", "Py_DecRef");
	PyStringStructurePtr = GetPointerForImport(L"Python27.dll", "PyString_Type");
	PyNoneStruct = GetPointerForImport(L"Python27.dll", "_Py_NoneStruct");
	//hook = new Utility::Hook(GetPointerForImport(L"Python27.dll", "PyObject_Call"), (void*)&PyHook, (void**)&PyObjectCall);
}


Core::~Core()
{
	if (hook != nullptr)
		delete hook;
}


Core& Core::CoreInst() {
	static Core core;
	return core;
}


