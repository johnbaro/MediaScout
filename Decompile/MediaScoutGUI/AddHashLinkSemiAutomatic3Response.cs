using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "AddHashLinkSemiAutomatic3Response", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class AddHashLinkSemiAutomatic3Response
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool AddHashLinkSemiAutomatic3Result;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlElement(IsNullable = true)]
	public double? points;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string error;

	public AddHashLinkSemiAutomatic3Response()
	{
	}

	public AddHashLinkSemiAutomatic3Response(bool AddHashLinkSemiAutomatic3Result, double? points, string error)
	{
		this.AddHashLinkSemiAutomatic3Result = AddHashLinkSemiAutomatic3Result;
		this.points = points;
		this.error = error;
	}
}
