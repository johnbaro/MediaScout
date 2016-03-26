using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class SubtitleActions
{
	private Guid idField;

	private bool enableVotingField;

	private bool enableReportingField;

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
	public bool EnableVoting
	{
		get
		{
			return this.enableVotingField;
		}
		set
		{
			this.enableVotingField = value;
		}
	}

	[XmlElement(Order = 2)]
	public bool EnableReporting
	{
		get
		{
			return this.enableReportingField;
		}
		set
		{
			this.enableReportingField = value;
		}
	}
}
