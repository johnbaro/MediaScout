using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "GetSubtitleThanksResponse", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class GetSubtitleThanksResponse
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool GetSubtitleThanksResult;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlArrayItem(IsNullable = false)]
	public SubtitleThank[] users;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public bool allowThanks;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 3)]
	public string error;

	public GetSubtitleThanksResponse()
	{
	}

	public GetSubtitleThanksResponse(bool GetSubtitleThanksResult, SubtitleThank[] users, bool allowThanks, string error)
	{
		this.GetSubtitleThanksResult = GetSubtitleThanksResult;
		this.users = users;
		this.allowThanks = allowThanks;
		this.error = error;
	}
}
