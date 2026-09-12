using System;
using System.IO;
using System.IO.Compression;

namespace YAFC.Audio
{
    public sealed class AudioManager : IDisposable
    {
        private readonly TrackerPlayer _player;

        private bool _initialized;
        private bool _disposed;

        public AudioManager()
        {
            _player = new TrackerPlayer();
        }

        public void Initialize()
        {
            ThrowIfDisposed();

            if (_initialized)
                return;

            Raylib_cs.Raylib.InitAudioDevice();

            _initialized = true;
        }

        public int LoadXm(Stream stream)
        {
            EnsureInitialized();

            return _player.Load(stream);
        }

        public int LoadXm(byte[] data)
        {
            EnsureInitialized();

            return _player.Load(data);
        }

        public void PlayXm(int id)
        {
            _player.Play(id);
        }

        public void StopXm(int id)
        {
            _player.Stop(id);
        }

        public void PauseXm(int id)
        {
            _player.Pause(id);
        }

        public void ResumeXm(int id)
        {
            _player.Resume(id);
        }

        public void SetXmVolume(
            int id,
            float volume)
        {
            _player.SetVolume(id, volume);
        }

        public float GetXmVolume(int id)
        {
            return _player.GetVolume(id);
        }

        public bool IsXmPlaying(int id)
        {
            return _player.IsPlaying(id);
        }

        public bool IsXmPaused(int id)
        {
            return _player.IsPaused(id);
        }

        public void UnloadXm(int id)
        {
            _player.Unload(id);
        }

        public void Update()
        {
            EnsureInitialized();

            _player.Update();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _player.Dispose();

            if (_initialized)
            {
                Raylib_cs.Raylib.CloseAudioDevice();
                _initialized = false;
            }

            _disposed = true;
        }

        private void EnsureInitialized()
        {
            ThrowIfDisposed();

            if (!_initialized)
            {
                throw new InvalidOperationException(
                    "AudioManager is not initialized. " +
                    "Call before Initialize().");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(AudioManager));
        }
    }
}
