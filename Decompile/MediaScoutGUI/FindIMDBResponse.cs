using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "FindIMDBResponse", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class FindIMDBResponse
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool FindIMDBResult;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public IMDB[] result;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string error;

	public FindIMDBResponse()
	{
	}

	public FindIMDBResponse(bool FindIMDBResult, IMDB[] result, string error)
	{
		this.FindIMDBResult = FindIMDBResult;
		this.result = result;
		this.error = error;
	}
}
