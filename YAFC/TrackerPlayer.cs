using ImGuiNET;
using Raylib_cs;
using System.Xml.Serialization;

namespace YAFC
{
    public sealed class TrackerPlayer : IDisposable
    {
        private sealed class Track
        { 
            public Music Music;
            public float Volume;
            public bool Playing;
            public bool Paused;
        }

        private readonly Dictionary<int, Track> _tracks = new Dictionary<int, Track>();

        private int _nextId = 1;
        private bool _isDisposed;

        public int Load(Stream stream)
        {
            ThrowIfDisposed();

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            byte[] data;

            using (var memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                data = memory.ToArray();
            }

            return Load(data);
        }

        public int Load(byte[] data)
        {
            ThrowIfDisposed();

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length == 0)
                throw new ArgumentException("XM data is empty.", nameof(data));

            Music music = Raylib.LoadMusicStreamFromMemory(".xm", data);

            if (music.FrameCount <= 0)
            {
                Raylib.UnloadMusicStream(music);

                throw new InvalidOperationException("Raylib was unable to load XM from memory");
            }

            return AddTrack(music);
        }

        private int AddTrack(Music music)
        {
            int id = _nextId++;

            _tracks.Add(id, new Track { Music = music, Volume = 1.0f, Playing = false, Paused = false });

            return id;
        }

        public void Play(int id)
        {
            Track track = GetTrack(id);

            Raylib.SetMusicVolume(track.Music, track.Volume);

            Raylib.PlayMusicStream(track.Music);

            track.Playing = true;
            track.Paused = false;
        }

        public void Stop(int id)
        {
            Track track = GetTrack(id);

            Raylib.StopMusicStream(track.Music);

            track.Playing = false;
            track.Paused = false;
        }

        public void Pause(int id)
        {
            Track track = GetTrack(id);

            if (!track.Playing || track.Paused)
                return;

            Raylib.PauseMusicStream(track.Music);

            track.Paused = true;
        }

        public void Resume(int id)
        {
            Track track = GetTrack(id);

            if (!track.Playing || !track.Paused)
                return;

            Raylib.ResumeMusicStream(track.Music);

            track.Paused = false;
        }

        public void SetVolume(int id, float volume)
        {
            Track track = GetTrack(id);

            volume = Math.Clamp(volume, 0.0f, 1.0f);

            track.Volume = volume;

            Raylib.SetMusicVolume(track.Music, volume);
        }

        public float GetVolume(int id)
        {
            return GetTrack(id).Volume;
        }

        public bool IsPlaying(int id)
        {
            return GetTrack(id).Playing && !GetTrack(id).Paused;
        }

        public bool IsPaused(int id)
        {
            return GetTrack(id).Paused;
        }

        public void Update()
        {
            ThrowIfDisposed();

            foreach (Track track in _tracks.Values)
            {
                if (track.Playing)
                {
                    Raylib.UpdateMusicStream(track.Music);
                }
            }
        }

        public void Unload(int id)
        {
            Track track = GetTrack(id);

            Raylib.UnloadMusicStream(track.Music);

            _tracks.Remove(id);
        }

        public void UnloadAll()
        {
            foreach (Track track in _tracks.Values)
            {
                Raylib.UnloadMusicStream(track.Music);
            }

            _tracks.Clear();
        }

        private Track GetTrack(int id)
        {
            ThrowIfDisposed();

            if (!_tracks.TryGetValue(id, out Track track))
            {
                throw new KeyNotFoundException($"XM track with ID {id} not found");
            }

            return track;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(TrackerPlayer));
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            UnloadAll();

            _isDisposed = true;
        }
    }
}
