using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class SubtitleComment
{
	private Guid idField;

	private SubtitleLanguage languageField;

	private Guid languageIDField;

	private Guid subtitleIDField;

	private Guid userIDField;

	private string userField;

	private int rateField;

	private SubtitleCommentStatus statusField;

	private string messageField;

	private DateTime createdField;

	private bool canDeleteField;

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
	public SubtitleLanguage Language
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

	[XmlElement(Order = 2)]
	public Guid LanguageID
	{
		get
		{
			return this.languageIDField;
		}
		set
		{
			this.languageIDField = value;
		}
	}

	[XmlElement(Order = 3)]
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

	[XmlElement(Order = 4)]
	public Guid UserID
	{
		get
		{
			return this.userIDField;
		}
		set
		{
			this.userIDField = value;
		}
	}

	[XmlElement(Order = 5)]
	public string User
	{
		get
		{
			return this.userField;
		}
		set
		{
			this.userField = value;
		}
	}

	[XmlElement(Order = 6)]
	public int Rate
	{
		get
		{
			return this.rateField;
		}
		set
		{
			this.rateField = value;
		}
	}

	[XmlElement(Order = 7)]
	public SubtitleCommentStatus Status
	{
		get
		{
			return this.statusField;
		}
		set
		{
			this.statusField = value;
		}
	}

	[XmlElement(Order = 8)]
	public string Message
	{
		get
		{
			return this.messageField;
		}
		set
		{
			this.messageField = value;
		}
	}

	[XmlElement(Order = 9)]
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

	[XmlElement(Order = 10)]
	public bool CanDelete
	{
		get
		{
			return this.canDeleteField;
		}
		set
		{
			this.canDeleteField = value;
		}
	}
}
