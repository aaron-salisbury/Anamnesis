using System.IO;

namespace Anamnesis.Core.SourceControl
{
    public class Change
    {
        public delegate Stream DownloadFileDelegate();

        public DownloadFileDelegate DownloadFile { get; set; }
        public string RepositoryPath { get; set; }
    }
}
