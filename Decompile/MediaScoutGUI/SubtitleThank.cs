using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class SubtitleThank
{
	private string usernameField;

	private DateTime createdField;

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
	public DateTime Created
	{
		get
		{
			return this.createdField;
		}
		set
		{
			this.createdField = value;
		}
	}
}
