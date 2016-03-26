using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class UserInfo
{
	private string usernameField;

	private string emailField;

	private DateTime createdField;

	private int subtitlesPublishedField;

	private int subtitlesDeletedField;

	private int subtitleThanksField;

	private double pointsField;

	private double averageRateField;

	private int subtitleDownloadsField;

	private int mySubtitleDownloadsField;

	private int totalSubtitleDownloadsField;

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

	[XmlElement(Order = 2)]
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

	[XmlElement(Order = 3)]
	public int SubtitlesPublished
	{
		get
		{
			return this.subtitlesPublishedField;
		}
		set
		{
			this.subtitlesPublishedField = value;
		}
	}

	[XmlElement(Order = 4)]
	public int SubtitlesDeleted
	{
		get
		{
			return this.subtitlesDeletedField;
		}
		set
		{
			this.subtitlesDeletedField = value;
		}
	}

	[XmlElement(Order = 5)]
	public int SubtitleThanks
	{
		get
		{
			return this.subtitleThanksField;
		}
		set
		{
			this.subtitleThanksField = value;
		}
	}

	[XmlElement(Order = 6)]
	public double Points
	{
		get
		{
			return this.pointsField;
		}
		set
		{
			this.pointsField = value;
		}
	}

	[XmlElement(Order = 7)]
	public double AverageRate
	{
		get
		{
			return this.averageRateField;
		}
		set
		{
			this.averageRateField = value;
		}
	}

	[XmlElement(Order = 8)]
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

	[XmlElement(Order = 9)]
	public int MySubtitleDownloads
	{
		get
		{
			return this.mySubtitleDownloadsField;
		}
		set
		{
			this.mySubtitleDownloadsField = value;
		}
	}

	[XmlElement(Order = 10)]
	public int TotalSubtitleDownloads
	{
		get
		{
			return this.totalSubtitleDownloadsField;
		}
		set
		{
			this.totalSubtitleDownloadsField = value;
		}
	}
}
