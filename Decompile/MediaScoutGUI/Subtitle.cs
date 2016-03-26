using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class Subtitle
{
	private string titleField;

	private string iMDBField;

	private SubtitleLanguage languageField;

	private SubtitleType subtitleTypeField;

	private string commentField;

	private MediaType mediaTypeField;

	private byte numberOfDiscsField;

	private string releaseField;

	private FPS fPSField;

	private Genre genreField;

	private Guid subtitleIDField;

	private Guid publisherIDField;

	private string publisherField;

	private int sizeField;

	private int downloadsField;

	private int votesField;

	private int reportsField;

	private float rateField;

	private byte? seasonField;

	private int? episodeField;

	private DateTime createdField;

	private int? yearField;

	private bool isLinkedField;

	private byte statusField;

	private string externalIdField;

	private string nonImdbTitleField;

	private string linkField;

	private bool canDeleteField;

	private Guid parentSubtitleIDField;

	private int subCountField;

	private bool isSynchronizationField;

	private bool isHearingImpairedField;

	[XmlElement(Order = 0)]
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

	[XmlElement(Order = 1)]
	public string IMDB
	{
		get
		{
			return this.iMDBField;
		}
		set
		{
			this.iMDBField = value;
		}
	}

	[XmlElement(Order = 2)]
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

	[XmlElement(Order = 3)]
	public SubtitleType SubtitleType
	{
		get
		{
			return this.subtitleTypeField;
		}
		set
		{
			this.subtitleTypeField = value;
		}
	}

	[XmlElement(Order = 4)]
	public string Comment
	{
		get
		{
			return this.commentField;
		}
		set
		{
			this.commentField = value;
		}
	}

	[XmlElement(Order = 5)]
	public MediaType MediaType
	{
		get
		{
			return this.mediaTypeField;
		}
		set
		{
			this.mediaTypeField = value;
		}
	}

	[XmlElement(Order = 6)]
	public byte NumberOfDiscs
	{
		get
		{
			return this.numberOfDiscsField;
		}
		set
		{
			this.numberOfDiscsField = value;
		}
	}

	[XmlElement(Order = 7)]
	public string Release
	{
		get
		{
			return this.releaseField;
		}
		set
		{
			this.releaseField = value;
		}
	}

	[XmlElement(Order = 8)]
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

	[XmlElement(Order = 9)]
	public Genre Genre
	{
		get
		{
			return this.genreField;
		}
		set
		{
			this.genreField = value;
		}
	}

	[XmlElement(Order = 10)]
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

	[XmlElement(Order = 11)]
	public Guid PublisherID
	{
		get
		{
			return this.publisherIDField;
		}
		set
		{
			this.publisherIDField = value;
		}
	}

	[XmlElement(Order = 12)]
	public string Publisher
	{
		get
		{
			return this.publisherField;
		}
		set
		{
			this.publisherField = value;
		}
	}

	[XmlElement(Order = 13)]
	public int Size
	{
		get
		{
			return this.sizeField;
		}
		set
		{
			this.sizeField = value;
		}
	}

	[XmlElement(Order = 14)]
	public int Downloads
	{
		get
		{
			return this.downloadsField;
		}
		set
		{
			this.downloadsField = value;
		}
	}

	[XmlElement(Order = 15)]
	public int Votes
	{
		get
		{
			return this.votesField;
		}
		set
		{
			this.votesField = value;
		}
	}

	[XmlElement(Order = 16)]
	public int Reports
	{
		get
		{
			return this.reportsField;
		}
		set
		{
			this.reportsField = value;
		}
	}

	[XmlElement(Order = 17)]
	public float Rate
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

	[XmlElement(IsNullable = true, Order = 18)]
	public byte? Season
	{
		get
		{
			return this.seasonField;
		}
		set
		{
			this.seasonField = value;
		}
	}

	[XmlElement(IsNullable = true, Order = 19)]
	public int? Episode
	{
		get
		{
			return this.episodeField;
		}
		set
		{
			this.episodeField = value;
		}
	}

	[XmlElement(Order = 20)]
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

	[XmlElement(IsNullable = true, Order = 21)]
	public int? Year
	{
		get
		{
			return this.yearField;
		}
		set
		{
			this.yearField = value;
		}
	}

	[XmlElement(Order = 22)]
	public bool IsLinked
	{
		get
		{
			return this.isLinkedField;
		}
		set
		{
			this.isLinkedField = value;
		}
	}

	[XmlElement(Order = 23)]
	public byte Status
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

	[XmlElement(Order = 24)]
	public string ExternalId
	{
		get
		{
			return this.externalIdField;
		}
		set
		{
			this.externalIdField = value;
		}
	}

	[XmlElement(Order = 25)]
	public string NonImdbTitle
	{
		get
		{
			return this.nonImdbTitleField;
		}
		set
		{
			this.nonImdbTitleField = value;
		}
	}

	[XmlElement(Order = 26)]
	public string Link
	{
		get
		{
			return this.linkField;
		}
		set
		{
			this.linkField = value;
		}
	}

	[XmlElement(Order = 27)]
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

	[XmlElement(Order = 28)]
	public Guid ParentSubtitleID
	{
		get
		{
			return this.parentSubtitleIDField;
		}
		set
		{
			this.parentSubtitleIDField = value;
		}
	}

	[XmlElement(Order = 29)]
	public int SubCount
	{
		get
		{
			return this.subCountField;
		}
		set
		{
			this.subCountField = value;
		}
	}

	[XmlElement(Order = 30)]
	public bool IsSynchronization
	{
		get
		{
			return this.isSynchronizationField;
		}
		set
		{
			this.isSynchronizationField = value;
		}
	}

	[XmlElement(Order = 31)]
	public bool IsHearingImpaired
	{
		get
		{
			return this.isHearingImpairedField;
		}
		set
		{
			this.isHearingImpairedField = value;
		}
	}
}
