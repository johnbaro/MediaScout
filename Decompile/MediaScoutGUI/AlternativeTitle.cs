using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class AlternativeTitle
{
	private Guid idField;

	private string titleField;

	private string languageField;

	[XmlElement(Order = 0)]
	public Guid ID
	{
		get
		{
			return this.idField;
		}
		set
		{
			this.idField = value;
		}
	}

	[XmlElement(Order = 1)]
	public string Title
	{
		get
		{
			return this.titleField;
		}
		set
		{
			this.titleField = value;
		}
	}

	[XmlElement(Order = 2)]
	public string Language
	{
		get
		{
			return this.languageField;
		}
		set
		{
			this.languageField = value;
		}
	}
}
