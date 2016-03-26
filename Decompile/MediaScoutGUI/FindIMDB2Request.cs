using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "FindIMDB2", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class FindIMDB2Request
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public Guid sessionId;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public string keyword;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2), XmlElement(IsNullable = true)]
	public int? year;

	public FindIMDB2Request()
	{
	}

	public FindIMDB2Request(Guid sessionId, string keyword, int? year)
	{
		this.sessionId = sessionId;
		this.keyword = keyword;
		this.year = year;
	}
}
