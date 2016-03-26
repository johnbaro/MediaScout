using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class ClientInfo
{
	private string clientIdField;

	private string apiKeyField;

	[XmlElement(Order = 0)]
	public string ClientId
	{
		get
		{
			return this.clientIdField;
		}
		set
		{
			this.clientIdField = value;
		}
	}

	[XmlElement(Order = 1)]
	public string ApiKey
	{
		get
		{
			return this.apiKeyField;
		}
		set
		{
			this.apiKeyField = value;
		}
	}
}
