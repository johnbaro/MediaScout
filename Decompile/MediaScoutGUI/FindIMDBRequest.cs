using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "FindIMDB", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class FindIMDBRequest
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public string keyword;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlElement(IsNullable = true)]
	public int? year;

	public FindIMDBRequest()
	{
	}

	public FindIMDBRequest(string keyword, int? year)
	{
		this.keyword = keyword;
		this.year = year;
	}
}
