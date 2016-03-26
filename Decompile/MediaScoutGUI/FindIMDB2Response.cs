using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "FindIMDB2Response", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class FindIMDB2Response
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool FindIMDB2Result;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public IMDB[] result;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string error;

	public FindIMDB2Response()
	{
	}

	public FindIMDB2Response(bool FindIMDB2Result, IMDB[] result, string error)
	{
		this.FindIMDB2Result = FindIMDB2Result;
		this.result = result;
		this.error = error;
	}
}
