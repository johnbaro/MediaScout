using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "AddHashLinkAutomatic4", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class AddHashLinkAutomatic4Request
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid session;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public Guid subtitleID;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string[] hashes;

	public AddHashLinkAutomatic4Request()
	{
	}

	public AddHashLinkAutomatic4Request(Guid session, Guid subtitleID, string[] hashes)
	{
		this.session = session;
		this.subtitleID = subtitleID;
		this.hashes = hashes;
	}
}
