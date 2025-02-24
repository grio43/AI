#include "pch.h"
#include <iostream>
namespace Utility {

	static int instances;

	Hook::Hook(void* fnAddress, void* fnCallback, void** userTrampVar) {

		this->ptr = fnAddress;

		InitMH();
		auto createHook = MH_CreateHook(fnAddress, fnCallback, userTrampVar); // Create a hook in disabled state
		if (createHook != MH_OK)
		{
			std::cout << "Failed to create the hook. Error: " << createHook << std::endl;
			return;
		}

		EnableHook();
		instances++;
	}

	void Hook::EnableHook() {
		auto enable = MH_EnableHook(this->ptr);  // Enable the hook
		if (enable != MH_OK)
		{
			std::cout << "Failed to enable the hook. Error: " << enable << std::endl;
			return;
		}
	}

	Hook:: ~Hook() {
		instances--;
		DisableHook();
		std::cout << "Hook instance destructed. Remaining instances " << instances << std::endl;
	}

	static bool bMHInitialized = false;

	void Hook::DisableHook() {
		auto disable = MH_DisableHook(ptr) != MH_OK;
		if (disable != MH_OK)
		{
			printf("Failed to disable the hook. \n");
		}
		TearDownMH();
	}

	void Hook::InitMH()
	{
		if (!bMHInitialized) {
			if (MH_Initialize() != MH_OK)
			{
				printf("MH init failed. \n");
			}
			bMHInitialized = true;
			printf("MH initialized. \n");
		}
	}
	void Hook::TearDownMH()
	{
		if (bMHInitialized && instances <= 0) {
			if (MH_Uninitialize() != MH_OK)
			{
				printf("MH teardown failed. \n");
			}
			bMHInitialized = false;
			printf("MH teared down. \n");
		}
	}
}