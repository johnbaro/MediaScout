using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "VoteMovieHashResponse", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class VoteMovieHashResponse
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool VoteMovieHashResult;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1)]
	public string error;

	public VoteMovieHashResponse()
	{
	}

	public VoteMovieHashResponse(bool VoteMovieHashResult, string error)
	{
		this.VoteMovieHashResult = VoteMovieHashResult;
		this.error = error;
	}
}
