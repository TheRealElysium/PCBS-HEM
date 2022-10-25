using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[Serializable]
public partial class ComputerSave : ISerializationCallbackReceiver
{
	// CHANGE: Fix for incorrect search
	public float CheckGPUWorkstationMiningMult(PartDescGPU gpu)
	{
		float num;
		if (!gpu.m_chipSetBrand.Contains("Phi"))
		{
			string chipSetBrand = gpu.m_chipSetBrand;
			if (chipSetBrand == "NVIDIA Quadro")
			{
				num = 1.1585f;
			}
			else if (chipSetBrand == "NVIDIA Tesla")
			{
				num = 1.17866f;
			}
			else if (chipSetBrand == "NVIDIA TITAN")
			{
				num = 1.07145f;
			}
			else if (chipSetBrand == "AMD Radeon Pro")
			{
				num = 1.25f;
			}
			else if (chipSetBrand == "AMD FirePro")
			{
				num = 1.135f;
			}
			else
			{
				num = ((chipSetBrand == "AMD Radeon Instinct") ? 1.285f : 1f);
			}
		}
		else
		{
			num = 1.102f;
		}
		return num;
	}
	
	public float GetGPUMiningRate()
	{
		float num2;
		if (this.GetGPU() != null)
		{
			int num = 0;
			PartInstance[] array = this.pciSlots;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
					num++;
				}
			}
			// CHANGE: GPU rate multiplier from VRAM // * Max(1, (1 + ((RAM - 128) / 4000))) \\ Added 1.4 multiplier to match cash output
			num2 = ((float)this.Get3dMarkGPUScore() * 0.1f + (float)(this.GetVRAM() / 4) * 0.05f) * Mathf.Max(1f, (1f + (float)((this.GetVRAM() - 128f) / 4000f))) * 0.012f * this.CheckGPUWorkstationMiningMult(this.GetGPU().GetPart() as PartDescGPU) / (float)((num == 0) ? 1 : num) / 1000f * 1.4f;
		}
		else
		{
			num2 = 0f;
		}
		return num2;
	}
	
	public float GetCryptoMiningRate()
	{
		float num;
		if (this.cpuID == null || this.GetGPU() == null || this.GetStorage() == 0 || this.GetRAM() == 0)
		{
			num = 0f;
		}
		else
		{
			PartDescCPU partDescCPU = this.cpuID.GetPart() as PartDescCPU;
			
			// CHANGE: CPU rate multiplier from core count
			// num2 =  (orig equation) * Max(1, (Cores - 64) / 512)
			
			float num2 = ((float)this.Get3dMarkCPUScore() * 0.07f + (float)partDescCPU.m_cores / 8f * 0.1f * (this.GetCPUOverClockFactor() / 2f) + (float)this.GetRAM() / 16f * 0.2f) * Mathf.Max(1f, ((float)partDescCPU.m_cores - 64f) / 512f);
			
			// CHANGE: GPU rate multiplier from VRAM
			// num3 = (orig equation) * Max(1, (1 + ((VRAM - 128) / 4000)))
			
			float num3 = ((float)this.Get3dMarkGPUScore() * 0.1f + (float)(this.GetVRAM() / 4) * 0.05f) * Mathf.Max(1f, (1f + (float)((this.GetVRAM() - 128f) / 4000f)));
			
			float num4 = (float)(this.GetStorage() / 100) * 1.3f;
			foreach (PartInstance partInstance in this.storageSlots)
			{
				if (partInstance != null)
				{
					PartDescStorage partDescStorage = partInstance.GetPart() as PartDescStorage;
					num4 += partDescStorage.m_speedMbs / 310f * 0.87f;
				}
			}
			num = num2 * 0.022f * this.CheckCPUWorkstationMiningMult(partDescCPU) + num3 * 0.012f * this.CheckGPUWorkstationMiningMult(this.GetGPU().GetPart() as PartDescGPU) + num4 * 0.02f;
		}
		return num;
	}
	
	public float GetCPUMiningRate()
	{
		float num;
		if (this.cpuID != null && this.GetRAM() != 0)
		{
			PartDescCPU partDescCPU = this.cpuID.GetPart() as PartDescCPU;
			
			// CHANGE: Rate multiplier from core count \\ Added 1.4 multiplier to match cash output
			// num = (CPUScore * 0.07 + Cores / 8 * 0.1 * (OCFactor / 2) + RAM / 16 * 0.2) * Max(1, (Cores - 64) / 512) * 0.022 * WorkstationMult / 1000 * 1.4;
			num = ((float)this.Get3dMarkCPUScore() * 0.07f + (float)partDescCPU.m_cores / 8f * 0.1f * (this.GetCPUOverClockFactor() / 2f) + (float)this.GetRAM() / 16f * 0.2f) * Mathf.Max(1f, ((float)partDescCPU.m_cores - 64f) / 512f) * 0.022f * this.CheckCPUWorkstationMiningMult(partDescCPU) / 1000f * 1.4f;
		}
		else
		{
			num = 0f;
		}
		return num;
	}
	
	// CHANGE: Add New GetStorageMiningRate method
	public float GetStorageMiningRate()
	{
		float num;
		if (this.storageSlots != null)
		{
			num = (float)(this.GetStorage() / 100) * 1.3f;
			foreach (PartInstance partInstance in this.storageSlots)
			{
				if (partInstance != null)
				{
					PartDescStorage partDescStorage = partInstance.GetPart() as PartDescStorage;
					num += partDescStorage.m_speedMbs / 310f * 0.87f;
				}
			}
		}
		else
		{
			num = 0f;
		}
		return num * 0.02f / 1000f * 1.4f;
	}
}
