using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "AddHashLinkSemiAutomatic4Response", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class AddHashLinkSemiAutomatic4Response
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool AddHashLinkSemiAutomatic4Result;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlElement(IsNullable = true)]
	public double? points;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string error;

	public AddHashLinkSemiAutomatic4Response()
	{
	}

	public AddHashLinkSemiAutomatic4Response(bool AddHashLinkSemiAutomatic4Result, double? points, string error)
	{
		this.AddHashLinkSemiAutomatic4Result = AddHashLinkSemiAutomatic4Result;
		this.points = points;
		this.error = error;
	}
}
