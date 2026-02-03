namespace SyncChat.API.Features.MediaManagement.InitiateUpload;

public sealed record MediaFileDescriptorDto(string FileName, string MimeType, long SizeBytes);