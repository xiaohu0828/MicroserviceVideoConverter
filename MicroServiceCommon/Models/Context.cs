using System.Collections.Generic;
using System.ComponentModel;

namespace MicroServiceCommon.Models
{
    public class Context 
    {
        public Context()
        {
            VideoFiles = new List<VideoFile>();
        }

        public List<VideoFile> VideoFiles { get; set; }

        public void Add(VideoFile file)
        {
            file.Id = VideoFiles.Count + 1;
            VideoFiles.Add(file);
        }
    }
}
