using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Serialization;

[GeneratedCode("System.ServiceModel", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Advanced), DebuggerStepThrough, MessageContract(WrapperName = "GetReleases2Response", WrapperNamespace = "http://www.sublight.si/", IsWrapped = true)]
public class GetReleases2Response
{
	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 0)]
	public bool GetReleases2Result;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 1), XmlArrayItem(IsNullable = false)]
	public Release[] releases;

	[MessageBodyMember(Namespace = "http://www.sublight.si/", Order = 2)]
	public string error;

	public GetReleases2Response()
	{
	}

	public GetReleases2Response(bool GetReleases2Result, Release[] releases, string error)
	{
		this.GetReleases2Result = GetReleases2Result;
		this.releases = releases;
		this.error = error;
	}
}
