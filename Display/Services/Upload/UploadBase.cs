using System;
using System.Threading.Tasks;

namespace Display.Services.Upload
{
    internal abstract class UploadBase : IUpload<bool>
    {
        private long _position;
        private long _length;
        private UploadState _state;
        private string _content;

        public abstract void Dispose();

        public abstract ValueTask DisposeAsync();


        public long Length
        {
            get => _length;
            set
            {
                if (value == _length) return;
                _length = value;
                LengthCallback?.Invoke(value);
            }
        }

        public long Position
        {
            get => _position;
            set
            {
                if (value == _position) return;

                _position = value;
                PositionCallback?.Invoke(value);
            }
        }

        public string Content
        {
            get => _content;
            protected set
            {
                if (_content == value) return;
                _content = value;
                ContentChanged?.Invoke(_content);
            }
        }

        public UploadState State
        {
            get => _state;
            protected set
            {
                if (_state == value) return;
                _state = value;
                StateChanged?.Invoke(value);
            }
        }

        public abstract Task Init();

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> Start();

        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        public abstract void Pause();

        /// <summary>
        /// 停止
        /// </summary>
        public abstract void Stop();

        public bool Running => State is UploadState.Initializing or UploadState.FastUploading or UploadState.OssUploading;

        public event Action<long> PositionCallback;
        public event Action<int> ProgressChanged;
        public event Action<UploadState> StateChanged;
        public event Action<long> LengthCallback;
        public event Action<string> ContentChanged;

        protected virtual void OnProgressChanged(int obj)
        {
            ProgressChanged?.Invoke(obj);
        }
    }
}
