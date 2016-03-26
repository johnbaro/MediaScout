using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "GetMyUploadsResponse", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class GetMyUploadsResponse
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool GetMyUploadsResult;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlArrayItem(IsNullable = false)]
	public Subtitle[] subtitles;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2), XmlArrayItem(IsNullable = false)]
	public Release[] releases;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 3)]
	public SubtitleActions[] actions;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 4)]
	public string error;

	public GetMyUploadsResponse()
	{
	}

	public GetMyUploadsResponse(bool GetMyUploadsResult, Subtitle[] subtitles, Release[] releases, SubtitleActions[] actions, string error)
	{
		this.GetMyUploadsResult = GetMyUploadsResult;
		this.subtitles = subtitles;
		this.releases = releases;
		this.actions = actions;
		this.error = error;
	}
}
