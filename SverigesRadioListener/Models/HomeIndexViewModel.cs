using System.Collections.Generic;

namespace SverigesRadioListener.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Channel> Channels { get; set; }
        public bool HasPrograms { get; set; }

        public class PodFile
        {
            public string Title { get; set; }
            public string Date { get; set; }
            public string Length { get; set; }
            public string Url { get; set; }
        }

        public class Program
        {
            public string Title { get; set; }
            public string ImageUrl { get; set; }
            public string Description { get; set; }
            public IEnumerable<PodFile> PodFiles { get; set; }
        }

        public class Channel
        {
            public string Title { get; set; }
            public IEnumerable<Program> Programs { get; set; }
        }
    }
}
