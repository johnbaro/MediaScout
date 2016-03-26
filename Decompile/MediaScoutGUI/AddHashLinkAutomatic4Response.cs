using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "AddHashLinkAutomatic4Response", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class AddHashLinkAutomatic4Response
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool AddHashLinkAutomatic4Result;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlElement(IsNullable = true)]
	public double? points;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string error;

	public AddHashLinkAutomatic4Response()
	{
	}

	public AddHashLinkAutomatic4Response(bool AddHashLinkAutomatic4Result, double? points, string error)
	{
		this.AddHashLinkAutomatic4Result = AddHashLinkAutomatic4Result;
		this.points = points;
		this.error = error;
	}
}
