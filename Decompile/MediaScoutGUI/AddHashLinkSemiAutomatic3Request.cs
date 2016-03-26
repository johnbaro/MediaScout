using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "AddHashLinkSemiAutomatic3", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class AddHashLinkSemiAutomatic3Request
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid session;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public Guid subtitleID;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string videoHash;

	public AddHashLinkSemiAutomatic3Request()
	{
	}

	public AddHashLinkSemiAutomatic3Request(Guid session, Guid subtitleID, string videoHash)
	{
		this.session = session;
		this.subtitleID = subtitleID;
		this.videoHash = videoHash;
	}
}
