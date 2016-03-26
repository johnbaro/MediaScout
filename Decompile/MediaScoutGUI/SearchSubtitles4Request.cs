using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "SearchSubtitles4", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class SearchSubtitles4Request
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid session;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public string[] videoHashes;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string title;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 3), XmlElement(IsNullable = true)]
	public int? year;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 4), XmlElement(IsNullable = true)]
	public byte? season;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 5), XmlElement(IsNullable = true)]
	public int? episode;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 6)]
	public SubtitleLanguage[] languages;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 7)]
	public Genre[] genres;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 8)]
	public string sender;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 9), XmlElement(IsNullable = true)]
	public float? rateGreaterThan;

	public SearchSubtitles4Request()
	{
	}

	public SearchSubtitles4Request(Guid session, string[] videoHashes, string title, int? year, byte? season, int? episode, SubtitleLanguage[] languages, Genre[] genres, string sender, float? rateGreaterThan)
	{
		this.session = session;
		this.videoHashes = videoHashes;
		this.title = title;
		this.year = year;
		this.season = season;
		this.episode = episode;
		this.languages = languages;
		this.genres = genres;
		this.sender = sender;
		this.rateGreaterThan = rateGreaterThan;
	}
}
