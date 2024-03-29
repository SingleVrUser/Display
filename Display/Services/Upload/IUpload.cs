using System;
using System.Threading.Tasks;
using Display.Models.Enums;

namespace Display.Services.Upload;

internal interface IUpload<T> : IDisposable, IAsyncDisposable
{
    public UploadState State { get; }

    public string Content
    {
        get;
    }

    public bool Running
    {
        get;
    }

    public long Length
    {
        get;
    }
    public long Position
    {
        get;
    }
    public Task Init();
    public Task<T> Start();
    public void Pause();
    public void Stop();

    public event Action<long> PositionCallback;
    public event Action<int> ProgressChanged;
    public event Action<UploadState> StateChanged;
    public event Action<long> LengthCallback;
    public event Action<string> ContentChanged;
}