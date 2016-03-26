using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "GetReleasesResponse", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class GetReleasesResponse
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool GetReleasesResult;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlArrayItem(IsNullable = false)]
	public Release[] releases;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string error;

	public GetReleasesResponse()
	{
	}

	public GetReleasesResponse(bool GetReleasesResult, Release[] releases, string error)
	{
		this.GetReleasesResult = GetReleasesResult;
		this.releases = releases;
		this.error = error;
	}
}
