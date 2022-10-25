using System;
using System.Collections.Generic;

[Serializable]
public class EmailMessage : Email
{
	
	public EmailMessage(string id, string fromId, string param, int day, List<string> m_extraParams = null)
		: base(day)
	{
		this.m_id = "Email/" + id;
		this.m_fromId = "Email/" + fromId;
		this.m_params.Add(param);
		if (m_extraParams != null)
		{
			this.m_params.AddRange(m_extraParams);
		}
	}
	
	public override string GetFrom()
	{
		string text;
		if (this.m_fromId.Contains("CRYPTO"))
		{
			text = "StevenB@emh.com";
		}
		else
		{
			text = this.m_fromId.Localized(true);
		}
		return text;
	}
	
	public override string GetSubject()
	{
		string text;
		if (this.m_id.Contains("CRYPTO"))
		{
			text = "Daily crypto payment";
		}
		else
		{
			text = (this.m_id + "_SUBJECT").Localized(true);
		}
		return text;
	}
	
	public override string GetBody()
	{
		string text;
		if (!this.m_id.Contains("CRYPTO"))
		{
			if (this.m_id.Contains("ELECTRICITY_DUE"))
			{
				text = "Thanks for paying your electricity bill. Your account was credited of " + this.m_params[0] + ". \n\nNote: Due to recent progress in technology, we now credit automatically your electricity bill, to reduce fraud.";
			}
			else if (!this.m_id.Contains("RENT_DUE"))
			{
				string text2 = (this.m_id + "_BODY").Localized(true);
				object[] array = this.m_params.ToArray();
				object[] array2 = array;
				text = string.Format(text2, array2);
			}
			else
			{
				text = "Thanks for paying your rent for this month. Your account was credited of " + this.m_params[0] + ". \n\nNote: we automated rent payment to reduce fraud.";
			}
		}
		else
		// CHANGE: Add electricity used line to email. Moved $ to left // TODO: Consider switching to daily utility payments for IT compatability
		{
			string text3 = "Hello!\nYou received a payment of ${1} for selling {2} EMH for ${3} each yesterday.\nThe price of EMH changed by ${4}.\nYou have used ${5} worth of electricity.\n\n\nGood luck on the electricity bill.\nBest regards, and keep minin' !";
			object[] array = this.m_params.ToArray();
			object[] array3 = array;
			text = string.Format(text3, array3);
		}
		return text;
	}
	
	private string m_id;
	
	private string m_fromId;
	
	protected List<string> m_params = new List<string>();
}
