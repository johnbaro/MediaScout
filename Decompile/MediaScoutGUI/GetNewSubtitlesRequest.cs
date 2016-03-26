using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "GetNewSubtitles", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class GetNewSubtitlesRequest
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid session;

	public GetNewSubtitlesRequest()
	{
	}

	public GetNewSubtitlesRequest(Guid session)
	{
		this.session = session;
	}
}
