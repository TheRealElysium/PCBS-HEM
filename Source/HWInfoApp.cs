using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class HWInfoApp : MonoBehaviour
{
	public HWInfoApp()
	{
	}
	
	private void Start()
	{
		this.m_computer = base.GetComponentInParent<VirtualComputer>().GetComputer();
		this.m_sim = base.GetComponentInParent<VirtualComputer>().GetComputerSim();
		this.AddComponent(this.m_computer.cpuID);
		this.AddComponent(this.m_computer.motherboardID);
		this.AddComponents(this.m_computer.pciSlots);
		this.AddComponents(this.m_computer.ramSlots);
		this.AddComponents(this.m_computer.allStorageSlots);
		PartDescCPU partDescCPU = this.m_computer.cpuID.GetPart() as PartDescCPU;
		HWInfoRow hwinfoRow = this.AddRow(this.m_sensorTitle, this.m_sensors.content, partDescCPU.m_uiTypeName, partDescCPU.m_uiName);
		hwinfoRow.SetExpanded(true);
		hwinfoRow.m_icon.sprite = PartsDatabase.GetSprite(PartDesc.ShopCategory.CPU);
		for (int i = 0; i < partDescCPU.m_cores; i++)
		{
			hwinfoRow.AddChild(this.AddSensor(true, string.Format("HWInfo/Core".Localized(true), i), () => this.m_sim.GetCpuInternalTemp().ToCelsius()));
		}
		int num = 0;
		foreach (PartInstance partInstance in this.m_computer.pciSlots)
		{
			if (partInstance != null && partInstance.IsPowered())
			{
				hwinfoRow = this.AddRow(this.m_sensorTitle, this.m_sensors.content, partInstance.GetPart().m_uiTypeName, partInstance.GetPart().m_uiName);
				hwinfoRow.m_icon.sprite = PartsDatabase.GetSprite(PartDesc.ShopCategory.GPU);
				hwinfoRow.SetExpanded(true);
				int thisGpuId = num;
				hwinfoRow.AddChild(this.AddSensor(true, "HWInfo/GPUTemp".Localized(true), () => this.m_sim.GetGpuInternalTemp(thisGpuId).ToCelsius()));
				num++;
			}
		}
		hwinfoRow = this.AddRow(this.m_sensorTitle, this.m_sensors.content, "HWInfo/OperatingSystem".Localized(true), "Omega OS");
		hwinfoRow.m_icon.sprite = PartsDatabase.GetSprite(PartDesc.ShopCategory.Motherboard);
		hwinfoRow.SetExpanded(true);
		hwinfoRow.AddChild(this.AddSensor(false, "HWInfo/Wattage".Localized(true), () => ((int)this.m_sim.GetWattage()).ToWattage()));
		hwinfoRow.AddChild(this.AddSensor(false, "HWInfo/MaxWattage".Localized(true), () => (this.m_computer.psuID.GetPart() as PartDescPSU).m_wattage.ToWattage()));
		hwinfoRow.AddChild(this.AddSensor(true, "HWInfo/CaseTemp".Localized(true), () => this.m_sim.GetCaseInternalTemp().ToCelsius()));
		hwinfoRow.AddChild(this.AddSensor(false, "HWInfo/MemoryClock".Localized(true), () => this.m_computer.GetMemoryClock().ToHZ()));
		hwinfoRow.AddChild(this.AddSensor(false, "HWInfo/PhysicalMemoryUsed".Localized(true), () => this.m_sim.GetPhysicalUsage().ToGB()));
		hwinfoRow.AddChild(this.AddSensor(false, "HWInfo/PhysicalMemoryAvailable".Localized(true), () => this.m_computer.GetRAM().ToGB()));
		hwinfoRow.AddChild(this.AddSensor(false, "HWInfo/PhysicalMemoryLoad".Localized(true), () => (this.m_sim.GetPhysicalUsage() / (float)this.m_computer.GetRAM() * 100f).ToString("N2") + "%"));
		hwinfoRow.AddChild(this.AddSensor(false, "HWInfo/VirtualMemoryCommited".Localized(true), () => this.m_sim.GetVirtualUsage().ToGB()));
		// CHANGE: Added sensors
		if (this.m_computer.GetCryptoMiningState())
		{
			hwinfoRow.AddChild(this.AddSensor(false, "EHM Mined", () => (this.m_computer.GetCryptoMiningRate() * Mathf.Clamp(this.m_computer.GetTimeMiningCrypto(), 0f, 10000f) / 1000f * 1.4f).ToString("N3") + " EHM"));
			hwinfoRow.AddChild(this.AddSensor(false, "Power Used", () => ((float)this.m_computer.GetPeakConsumption() * Mathf.Clamp(this.m_computer.GetTimeMiningCrypto(), 0f, 10000f) / 3600f).ToString("N3") + " Wh"));
		}
	}

	private void AddComponents(IEnumerable<PartInstance> pis)
	{
		foreach (PartInstance partInstance in pis)
		{
			this.AddComponent(partInstance);
		}
	}

	private void AddComponent(PartInstance pi)
	{
		if (pi == null)
		{
			return;
		}
		PartDesc part = pi.GetPart();
		if (part.m_subPart != null || !pi.IsActive())
		{
			return;
		}
		HWInfoRow hwinfoRow = this.AddRow(this.m_componentTitle, this.m_summary.content, part.m_uiTypeName, part.m_uiName);
		hwinfoRow.m_icon.sprite = PartsDatabase.GetSprite(part.m_shopCategory);
		if (pi.GetPart().m_type == PartDesc.Type.CPU)
		{
			hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, ScriptLocalization.ShopProp.CPU_Freq, this.m_computer.GetBiosConfig().GetCPUSpeed().ToHZ()));
			if (this.m_computer.GetCryptoMiningState())
			{
				hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, "EHM Mining Rate", () => this.m_computer.GetCPUMiningRate().ToString("N5")));
			}
		}
		else if (pi.GetPart().m_type == PartDesc.Type.GPU || pi.GetPart().m_type == PartDesc.Type.WATER_COOLED_GPU)
		{
			BiosConfig bc = this.m_computer.GetBiosConfig();
			hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, ScriptLocalization.ShopProp.GPU_VRAM, (part as PartDescGPU).m_vramGb.ToGB()));
			hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, ScriptLocalization.ShopProp.GPU_CoreFreq, () => bc.m_gpuClock.ToHZ()));
			hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, ScriptLocalization.ShopProp.GPU_MemFreq, () => bc.m_gpuMemClock.ToHZ()));
			if (this.m_computer.GetCryptoMiningState())
			{
				hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, "EHM Mining Rate", () => this.m_computer.GetGPUMiningRate().ToString("N5")));
			}
		}
		else if (pi.GetPart().m_type == PartDesc.Type.RAM)
		{
			BiosConfig biosConfig = this.m_computer.GetBiosConfig();
			hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, ScriptLocalization.ShopProp.RAM_Freq, biosConfig.GetRAMSpeed().ToHZ()));
		}
		else if (pi.GetPart().m_type == PartDesc.Type.MOTHERBOARD && this.m_computer.GetCryptoMiningState())
		{
			// CHANGE: Added summary for storage and efficiency(EHM/Wh) // Normalized output to match
			float miningRate = this.m_computer.GetCryptoMiningRate() / 1000f * 1.4f;
			hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, "Data EHM Mining Rate", () => this.m_computer.GetStorageMiningRate().ToString("N5")));
			hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, "Total EHM Mining Rate", () => miningRate.ToString("N5")));
			hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, "Mining Efficiency", () => (miningRate / ((float)this.m_computer.GetPeakConsumption() / 3600f)).ToString("N3")));
		}
		List<string> list = new List<string>();
		part.GetHWInfoProps(list, ShopPropsPurpose.PartSpecs);
		for (int i = 1; i < list.Count; i += 2)
		{
			hwinfoRow.AddChild(this.AddRow(this.m_componentProp, this.m_summary.content, list[i - 1], list[i]));
		}
	}

	private HWInfoRow AddSensor(bool temp, string key, Func<string> value)
	{
		HWInfoRow hwinfoRow = UnityEngine.Object.Instantiate<HWInfoRow>((!temp) ? this.m_sensorTiming : this.m_sensorTemperature, this.m_sensors.content);
		hwinfoRow.Init(key, value);
		return hwinfoRow;
	}

	private HWInfoRow AddRow(HWInfoRow pref, Transform parent, string key, string value)
	{
		HWInfoRow hwinfoRow = UnityEngine.Object.Instantiate<HWInfoRow>(pref, parent);
		hwinfoRow.Init(key, value);
		return hwinfoRow;
	}

	private HWInfoRow AddRow(HWInfoRow pref, Transform parent, string key, Func<string> value)
	{
		HWInfoRow hwinfoRow = UnityEngine.Object.Instantiate<HWInfoRow>(pref, parent);
		hwinfoRow.Init(key, value);
		return hwinfoRow;
	}

	public HWInfoRow m_componentTitle;

	public HWInfoRow m_componentProp;

	public HWInfoRow m_sensorTitle;

	public HWInfoRow m_sensorTiming;

	public HWInfoRow m_sensorTemperature;

	public ScrollRect m_summary;

	public ScrollRect m_sensors;

	private ComputerSave m_computer;

	private ComputerSim m_sim;
}
