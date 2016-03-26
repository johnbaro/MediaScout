using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "VoteMovieHash", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class VoteMovieHashRequest
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid session;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public string imdb;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2), XmlElement(IsNullable = true)]
	public byte? season;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 3), XmlElement(IsNullable = true)]
	public int? episode;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 4)]
	public string videoHash;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 5)]
	public MovieHashVote type;

	public VoteMovieHashRequest()
	{
	}

	public VoteMovieHashRequest(Guid session, string imdb, byte? season, int? episode, string videoHash, MovieHashVote type)
	{
		this.session = session;
		this.imdb = imdb;
		this.season = season;
		this.episode = episode;
		this.videoHash = videoHash;
		this.type = type;
	}
}
