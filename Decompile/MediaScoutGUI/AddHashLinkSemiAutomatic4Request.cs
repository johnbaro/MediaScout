using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "AddHashLinkSemiAutomatic4", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class AddHashLinkSemiAutomatic4Request
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid session;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public Guid subtitleID;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string[] hashes;

	public AddHashLinkSemiAutomatic4Request()
	{
	}

	public AddHashLinkSemiAutomatic4Request(Guid session, Guid subtitleID, string[] hashes)
	{
		this.session = session;
		this.subtitleID = subtitleID;
		this.hashes = hashes;
	}
}
