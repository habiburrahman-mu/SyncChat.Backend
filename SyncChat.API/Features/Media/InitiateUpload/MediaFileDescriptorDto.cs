namespace SyncChat.API.Features.Media.InitiateUpload;

public sealed record MediaFileDescriptorDto(string FileName, string MimeType, long SizeBytes);