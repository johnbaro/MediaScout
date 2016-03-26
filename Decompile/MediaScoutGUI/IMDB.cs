using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), DesignerCategory("code"), DebuggerStepThrough, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public class IMDB
{
	private string idField;

	private string titleField;

	private int? yearField;

	private int? yearToField;

	private int? seasonField;

	private int? episodeField;

	private string posterUrlField;

	private float? userRatingField;

	private DateTime? syncDateField;

	private string tagField;

	private Genre? genreField;

	[XmlElement(Order = 0)]
	public string Id
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

	[XmlElement(IsNullable = true, Order = 2)]
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

	[XmlElement(IsNullable = true, Order = 3)]
	public int? YearTo
	{
		get
		{
			return this.yearToField;
		}
		set
		{
			this.yearToField = value;
		}
	}

	[XmlElement(IsNullable = true, Order = 4)]
	public int? Season
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

	[XmlElement(IsNullable = true, Order = 5)]
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

	[XmlElement(Order = 6)]
	public string PosterUrl
	{
		get
		{
			return this.posterUrlField;
		}
		set
		{
			this.posterUrlField = value;
		}
	}

	[XmlElement(IsNullable = true, Order = 7)]
	public float? UserRating
	{
		get
		{
			return this.userRatingField;
		}
		set
		{
			this.userRatingField = value;
		}
	}

	[XmlElement(IsNullable = true, Order = 8)]
	public DateTime? SyncDate
	{
		get
		{
			return this.syncDateField;
		}
		set
		{
			this.syncDateField = value;
		}
	}

	[XmlElement(Order = 9)]
	public string Tag
	{
		get
		{
			return this.tagField;
		}
		set
		{
			this.tagField = value;
		}
	}

	[XmlElement(IsNullable = true, Order = 10)]
	public Genre? Genre
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
}
