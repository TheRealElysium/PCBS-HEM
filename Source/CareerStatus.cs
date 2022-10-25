using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PCBS;
using UnityEngine;

public partial class CareerStatus : MonoBehaviour
{
	public float GetCryptoPrice(out float changeAmount)
	{
		float num = 0.3f;
		float num2 = 4f;
		float num3 = 1.1f;
		float num4 = 30f;
		float num5 = 5f;
		float num6 = 3f;
		float num7 = UnityEngine.Random.Range(0f, 1f);
		float num8 = 2f * num * num7;
		if (this.m_cryptoPrice == 0f)
		{
			this.m_cryptoPrice = 15f;
		}
		if (num8 > num)
		{
			num8 -= 2f * num;
		}
		changeAmount = this.m_cryptoPrice * num8;
		float num9 = this.m_cryptoPrice + changeAmount;
		if (num9 < 0f)
		{
			num9 = UnityEngine.Random.Range(0f, 5f);
		}
		if (Math.Abs(num4 - num9) < num6)
		{
			num9 -= (float)Math.Pow(1.20000004768372, (double)Math.Abs(num6 - (num4 - num9)));
		}
		while (num9 > num4)
		{
			num9 -= UnityEngine.Random.Range(1f, 2f) * num5;
		}
		if (num9 < num2)
		{
			num9 += UnityEngine.Random.Range(1f, 2f) * num3;
		}
		
		// CHANGE: Decreased value for crypto by 10x, but can mine 10x longer
		float priceMulti = 0.1f;
		num9 *= priceMulti;
		changeAmount = num9 - this.m_cryptoPrice * priceMulti;
		
		this.m_cryptoPrice = num9;
		return this.m_cryptoPrice;
	}
	
	public void GetCryptoPayAndEmail()
	{
		WorkshopController workshopController = WorkshopController.Get();
		float pay = 0f;
		float changeAmount;
		float cryptoPrice = this.GetCryptoPrice(out changeAmount);
		
		// CHANGE: Obsolete variable
		// float cryptoMined = 0f;
		
		// CHANGE: New powerUsed variable to seperate lights cost with hardware use
		float powerUsed = 0f;
		
		int lights = ((!this.m_state.m_lightOn) ? 6 : 12);
		foreach (BenchSlot benchSlot in workshopController.slsys.benchSlots)
		{
			if (benchSlot.GetComputer() != null)
			{
				if (benchSlot.GetComputer().GetTimeMiningCrypto() > 0f)
				{
					ComputerSave computer = benchSlot.GetComputer();
					if (computer != null)
					{
						float cryptoMiningRate = computer.GetCryptoMiningRate();
						int cryptoValue = (int)Math.Round(cryptoMiningRate * cryptoPrice);
						
						// CHANGE: Increased ceiling for mining time by 10x, but crypto is worth 10x less
						float timeMiningCrypto = Mathf.Clamp(computer.GetTimeMiningCrypto(), 0f, 10000f);
						
						float valueInc = (float)cryptoValue / 1000f;
						float currentValue = valueInc + valueInc * timeMiningCrypto * 1.4f;
						pay += ((currentValue > (float)cryptoValue * 2f) ? ((float)cryptoValue * 2f) : currentValue);
						
						// CHANGE: Obsolete variable
						// cryptoMined += cryptoMiningRate / 1000f * timeMiningCrypto + valueInc;
						
						// CHANGE: Power cost scaled down with crypto value
						powerUsed += (float)computer.GetPeakConsumption() * timeMiningCrypto / 15000f;
						
						if (benchSlot.GetComponentInParent<WorkStation>() != null && benchSlot.GetComponentInParent<WorkStation>().GetComponentInChildren<VirtualComputer>() != null && benchSlot.GetComponentInParent<WorkStation>().GetComponentInChildren<VirtualComputer>().GetComputerSim() != null)
						{
							benchSlot.GetComponentInParent<WorkStation>().GetComponentInChildren<VirtualComputer>().GetComputerSim()
								.CryptoBreakComponents();
						}
						computer.ResetTimeMining();
						
						// CHANGE: Add powerUsed with lights cost
						this.m_state.m_utilityCost += (int)(Mathf.Round(powerUsed)) + lights;
					}
				}
				if (benchSlot.GetComputer() != null && benchSlot.GetComponentInParent<WorkStation>() != null && benchSlot.GetComponentInParent<WorkStation>().GetComponentInChildren<VirtualComputer>() != null && benchSlot.GetComponentInParent<WorkStation>().GetComponentInChildren<VirtualComputer>().GetComputerSim() != null)
				{
					benchSlot.GetComponentInParent<WorkStation>().GetComponentInChildren<VirtualComputer>().GetComputerSim()
						.CrashIfNonECCRam();
				}
			}
		}
		if (pay > 0f)
		{
			this.AddCash((int)Math.Round((double)pay));
			
			// CHANGE: Fix for inconsistent output with pay
			float _cryptoMined = pay / cryptoPrice;
			
			List<string> list = new List<string>
			{
				// CHANGE: CultureInfo "N2" instead of "N1"
				pay.ToString("N2"),
				
				_cryptoMined.ToString("N3"),
				cryptoPrice.ToString("N2"),
				changeAmount.ToString("N2"),
				
				// CHANGE: New powerUsed output
				powerUsed.ToString("N2")
			};
			this.AddEmailMessage(new EmailMessage("CRYPTO_PAY", "CRYPTO_UNKNOWN", string.Empty, this.GetToday(), list));
		}
	}
}
