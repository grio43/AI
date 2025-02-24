#pragma once
#include "pch.h"
namespace Utility {

	class Hook
	{
	public:
		Hook(void* fnAddress, void* fnCallback, void** userTrampVar);
		~Hook();
		void DisableHook();
		void EnableHook();
		static void InitMH();
		static void TearDownMH();

	private:
		void* ptr;

	};
}
