namespace Display.Services.Upload;

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