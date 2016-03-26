using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class Release
{
	private Guid subtitleIDField;

	private Guid releaseIDField;

	private string nameField;

	private FPS fPSField;

	[XmlElement(Order = 0)]
	public Guid SubtitleID
	{
		get
		{
			return this.subtitleIDField;
		}
		set
		{
			this.subtitleIDField = value;
		}
	}

	[XmlElement(Order = 1)]
	public Guid ReleaseID
	{
		get
		{
			return this.releaseIDField;
		}
		set
		{
			this.releaseIDField = value;
		}
	}

	[XmlElement(Order = 2)]
	public string Name
	{
		get
		{
			return this.nameField;
		}
		set
		{
			this.nameField = value;
		}
	}

	[XmlElement(Order = 3)]
	public FPS FPS
	{
		get
		{
			return this.fPSField;
		}
		set
		{
			this.fPSField = value;
		}
	}
}
