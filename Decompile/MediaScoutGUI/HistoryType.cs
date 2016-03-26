using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

[GeneratedCode("svcutil", "4.0.30319.1"), Flags, XmlType(Namespace = "http://www.sublight.si/")]
[Serializable]
public enum HistoryType
{
	None = 1,
	Published = 2,
	StatusChanged = 4,
	StatusChangedDeleted = 8,
	StatusChangedAuthorized = 16,
	SubtitleUpdated = 32,
	ImdbUpdated = 64
}
