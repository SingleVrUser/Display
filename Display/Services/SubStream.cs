using System;
using System.IO;

namespace Display.Services;

internal class SubStream : Stream
{
    private readonly Stream _baseStream;
    private readonly long _length;
    private readonly IProgress<long> _progress;

    private long _position;

    public SubStream(Stream baseStream, long offset, long length, IProgress<long> progress)
    {
        if (baseStream == null) throw new ArgumentNullException(nameof(baseStream));
        if (!baseStream.CanRead) throw new ArgumentException("can't read base stream");
        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

        _baseStream = baseStream;
        _length = length;
        _progress = progress;

        if (baseStream.CanSeek)
        {
            baseStream.Seek(offset, SeekOrigin.Current);
        }
        else
        { // read it manually...
            const int bufferSize = 512;
            byte[] buffer = new byte[bufferSize];
            while (offset > 0)
            {
                int read = baseStream.Read(buffer, 0, offset < bufferSize ? (int)offset : bufferSize);
                offset -= read;
            }
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        CheckDisposed();
        var remaining = _length - _position;
        if (remaining <= 0) return 0;
        if (remaining < count) count = (int)remaining;

        var read = _baseStream.Read(buffer, offset, count);
        _position += read;

        _progress.Report(_position);
        return read;
    }
    private void CheckDisposed()
    {
        if (_baseStream == null) throw new ObjectDisposedException(GetType().Name);
    }
    public override long Length
    {
        get { CheckDisposed(); return _length; }
    }
    public override bool CanRead
    {
        get { CheckDisposed(); return true; }
    }
    public override bool CanWrite
    {
        get { CheckDisposed(); return false; }
    }
    public override bool CanSeek
    {
        get { CheckDisposed(); return false; }
    }
    public override long Position
    {
        get
        {
            CheckDisposed();
            return _position;
        }
        set => throw new NotSupportedException();
    }
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }
    public override void Flush()
    {
        CheckDisposed(); _baseStream.Flush();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}