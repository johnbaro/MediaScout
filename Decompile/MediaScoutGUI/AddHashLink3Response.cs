using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "AddHashLink3Response", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class AddHashLink3Response
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool AddHashLink3Result;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlElement(IsNullable = true)]
	public double? points;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string error;

	public AddHashLink3Response()
	{
	}

	public AddHashLink3Response(bool AddHashLink3Result, double? points, string error)
	{
		this.AddHashLink3Result = AddHashLink3Result;
		this.points = points;
		this.error = error;
	}
}
