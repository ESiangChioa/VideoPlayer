namespace TestDetectionl
{
    public struct VideoInfo
    {
        public string Filename;
        public int FrameCount;
        public int Width;
        public int Height;
        public int CurrentFrame;
        public int Fps;
        public PlaySate PlaySate;
    }

    public enum PlaySate
    {
        None,
        Playing,
        Pause,
        Stop
    }
}