using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class HistoryItem
{
	private string usernameField;

	private HistoryType typeField;

	private DateTime changedField;

	[XmlElement(Order = 0)]
	public string Username
	{
		get
		{
			return this.usernameField;
		}
		set
		{
			this.usernameField = value;
		}
	}

	[XmlElement(Order = 1)]
	public HistoryType Type
	{
		get
		{
			return this.typeField;
		}
		set
		{
			this.typeField = value;
		}
	}

	[XmlElement(Order = 2)]
	public DateTime Changed
	{
		get
		{
			return this.changedField;
		}
		set
		{
			this.changedField = value;
		}
	}
}
