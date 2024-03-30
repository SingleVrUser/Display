namespace Display.Models.Enums;

internal enum UploadState
{
    None,
    Initializing,
    Initialized,

    FastUploading,
    OssUploading,

    Paused,
    Faulted,
    Canceled,
    Succeed
}