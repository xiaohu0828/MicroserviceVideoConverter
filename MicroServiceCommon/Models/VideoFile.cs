using System;

namespace MicroServiceCommon.Models
{
    [Serializable]
    public class VideoFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
