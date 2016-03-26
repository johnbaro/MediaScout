using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class User
{
	private Guid idField;

	private string usernameField;

	private string displayNameField;

	private bool isBuiltInField;

	private string emailField;

	private DateTime createdField;

	private string passwordField;

	private int subtitleDownloadsField;

	private int externalSubtitleDownloadsField;

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

	[XmlElement(Order = 2)]
	public string DisplayName
	{
		get
		{
			return this.displayNameField;
		}
		set
		{
			this.displayNameField = value;
		}
	}

	[XmlElement(Order = 3)]
	public bool IsBuiltIn
	{
		get
		{
			return this.isBuiltInField;
		}
		set
		{
			this.isBuiltInField = value;
		}
	}

	[XmlElement(Order = 4)]
	public string Email
	{
		get
		{
			return this.emailField;
		}
		set
		{
			this.emailField = value;
		}
	}

	[XmlElement(Order = 5)]
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

	[XmlElement(Order = 6)]
	public string Password
	{
		get
		{
			return this.passwordField;
		}
		set
		{
			this.passwordField = value;
		}
	}

	[XmlElement(Order = 7)]
	public int SubtitleDownloads
	{
		get
		{
			return this.subtitleDownloadsField;
		}
		set
		{
			this.subtitleDownloadsField = value;
		}
	}

	[XmlElement(Order = 8)]
	public int ExternalSubtitleDownloads
	{
		get
		{
			return this.externalSubtitleDownloadsField;
		}
		set
		{
			this.externalSubtitleDownloadsField = value;
		}
	}
}
