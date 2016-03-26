using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "GetReleases2", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class GetReleases2Request
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid session;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public Guid[] subtitleIds;

	public GetReleases2Request()
	{
	}

	public GetReleases2Request(Guid session, Guid[] subtitleIds)
	{
		this.session = session;
		this.subtitleIds = subtitleIds;
	}
}
