// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// IClientMessageInspector declares both members with a 'ref Message' parameter so that an
// inspector can substitute the message. The signature is imposed by WCF and cannot be changed
// without breaking the interface implementation.
[assembly: SuppressMessage(
	"Major Code Smell",
	"S3874:\"out\" and \"ref\" parameters should not be used",
	Justification = "Signature is mandated by System.ServiceModel.Dispatcher.IClientMessageInspector.",
	Scope = "member",
	Target = "~M:AutoTask.Api.AutoTaskLogger.AfterReceiveReply(System.ServiceModel.Channels.Message@,System.Object)")]
[assembly: SuppressMessage(
	"Major Code Smell",
	"S3874:\"out\" and \"ref\" parameters should not be used",
	Justification = "Signature is mandated by System.ServiceModel.Dispatcher.IClientMessageInspector.",
	Scope = "member",
	Target = "~M:AutoTask.Api.AutoTaskLogger.BeforeSendRequest(System.ServiceModel.Channels.Message@,System.ServiceModel.IClientChannel)")]
