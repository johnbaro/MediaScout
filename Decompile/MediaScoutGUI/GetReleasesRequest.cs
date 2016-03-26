using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "GetReleases", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class GetReleasesRequest
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid[] subtitleIds;

	public GetReleasesRequest()
	{
	}

	public GetReleasesRequest(Guid[] subtitleIds)
	{
		this.subtitleIds = subtitleIds;
	}
}
