using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "AddHashLinkAutomatic3Response", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class AddHashLinkAutomatic3Response
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool AddHashLinkAutomatic3Result;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlElement(IsNullable = true)]
	public double? points;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string error;

	public AddHashLinkAutomatic3Response()
	{
	}

	public AddHashLinkAutomatic3Response(bool AddHashLinkAutomatic3Result, double? points, string error)
	{
		this.AddHashLinkAutomatic3Result = AddHashLinkAutomatic3Result;
		this.points = points;
		this.error = error;
	}
}
