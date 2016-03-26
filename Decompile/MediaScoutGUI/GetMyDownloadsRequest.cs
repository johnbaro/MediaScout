using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "GetMyDownloads", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class GetMyDownloadsRequest
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid session;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public Guid[] guids;

	public GetMyDownloadsRequest()
	{
	}

	public GetMyDownloadsRequest(Guid session, Guid[] guids)
	{
		this.session = session;
		this.guids = guids;
	}
}
