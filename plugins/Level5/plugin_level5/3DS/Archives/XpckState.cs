using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kontract.Interfaces.FileSystem;
using Kontract.Interfaces.Plugins.State;
using Kontract.Interfaces.Plugins.State.Archive;
using Kontract.Models.Archive;
using Kontract.Models.Context;
using Kontract.Models.IO;

namespace plugin_level5._3DS.Archives
{
    public class XpckState : IArchiveState, ILoadFiles, ISaveFiles, IReplaceFiles, IRenameFiles, IRemoveFiles, IAddFiles
    {
        private readonly Xpck _xpck;
        private bool _hasDeletedFiles;
        private bool _hasAddedFiles;

        public IList<IArchiveFileInfo> Files { get; private set; }
        public bool ContentChanged => IsChanged();

        public XpckState()
        {
            _xpck = new Xpck();
        }

        public async Task Load(IFileSystem fileSystem, UPath filePath, LoadContext loadContext)
        {
            var fileStream = await fileSystem.OpenFileAsync(filePath);
            Files = _xpck.Load(fileStream);
        }

        public Task Save(IFileSystem fileSystem, UPath savePath, SaveContext saveContext)
        {
            var output = fileSystem.OpenFile(savePath, FileMode.Create);
            _xpck.Save(output, Files);

            return Task.CompletedTask;
        }

        public void ReplaceFile(IArchiveFileInfo afi, Stream fileData)
        {
            afi.SetFileData(fileData);
        }

        private bool IsChanged()
        {
            return _hasDeletedFiles || _hasAddedFiles || Files.Any(x => x.ContentChanged);
        }

        public void Rename(IArchiveFileInfo afi, UPath path)
        {
            afi.FilePath = path;
        }
        public void RemoveFile(IArchiveFileInfo afi)
        {
            Files.Remove(afi);
            _hasDeletedFiles = true;
        }

        public void RemoveAll()
        {
            Files.Clear();
            _hasDeletedFiles = true;
        }

        public IArchiveFileInfo AddFile(Stream fileData, UPath filePath)
        {
            var newAfi = new XpckArchiveFileInfo(fileData, filePath.FullName, new XpckFileInfo());
            Files.Add(newAfi);

            _hasAddedFiles = true;

            return newAfi;
        }
    }
}
