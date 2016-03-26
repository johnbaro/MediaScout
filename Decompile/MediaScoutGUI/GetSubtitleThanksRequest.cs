using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "GetSubtitleThanks", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class GetSubtitleThanksRequest
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid session;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public Guid subtitleId;

	public GetSubtitleThanksRequest()
	{
	}

	public GetSubtitleThanksRequest(Guid session, Guid subtitleId)
	{
		this.session = session;
		this.subtitleId = subtitleId;
	}
}
