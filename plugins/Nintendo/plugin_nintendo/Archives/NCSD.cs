using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Komponent.IO;
using Komponent.IO.Streams;
using Kontract.Extensions;
using Kontract.Models.Archive;

namespace plugin_nintendo.Archives
{
    public class NCSD
    {
        private const int MediaSize_ = 0x200;
        private const int FirstPartitionOffset_ = 0x4000;

        private NcsdHeader _header;

        public IList<IArchiveFileInfo> Load(Stream input)
        {
            using var br = new BinaryReaderX(input, true);

            // Read header
            _header = ReadHeader(br);

            // Parse NCCH partitions
            var result = new List<IArchiveFileInfo>();
            for (var i = 0; i < 8; i++)
            {
                var partitionEntry = _header.partitionEntries[i];
                if (partitionEntry.length == 0)
                    continue;

                var name = GetPartitionName(i);
                var fileStream = new SubStream(input, (long)partitionEntry.offset * MediaSize_, (long)partitionEntry.length * MediaSize_);

                result.Add(new ArchiveFileInfo(fileStream, name)
                {
                    // Add NCCH plugin
                    PluginIds = new[] { Guid.Parse("7d0177a6-1cab-44b3-bf22-39f5548d6cac") }
                });
            }

            return result;
        }

        public void Save(Stream output, IList<IArchiveFileInfo> files)
        {
            // Update partition entries
            long partitionOffset = FirstPartitionOffset_;
            foreach (var file in files.Cast<ArchiveFileInfo>())
            {
                var partitionIndex = GetPartitionIndex(file.FilePath.GetName());
                var partitionEntry = _header.partitionEntries[partitionIndex];

                partitionEntry.offset = (int)(partitionOffset / MediaSize_);
                partitionEntry.length = (int)(file.FileSize / MediaSize_);

                output.Position = partitionOffset;
                file.SaveFileData(output);

                partitionOffset = output.Position;
            }

            // Store first NCCH header
            var firstNcchHeader = new byte[0x100];
            foreach (var partitionEntry in _header.partitionEntries)
            {
                if (partitionEntry.length != 0)
                {
                    var ncchStream = new SubStream(output, partitionEntry.offset * MediaSize_, partitionEntry.length * MediaSize_);
                    ncchStream.Read(firstNcchHeader, 0, 0x100);
                    break;
                }
            }

            _header.cardHeader.cardInfoHeader.firstNcchHeader = firstNcchHeader;

            output.Position = 0;
            using var bw = new BinaryWriterX(output);

            // Update NCSD size
            _header.ncsdSize = (int)(output.Length / MediaSize_);

            // Write NCSD header
            bw.WriteType(_header);

            // Pad until first partition
            bw.WritePadding(FirstPartitionOffset_ - Tools.MeasureType(typeof(NcsdHeader)), 0xFF);
        }

        private string GetPartitionName(int partitionIndex)
        {
            switch (partitionIndex)
            {
                case 0:
                    return "GameData.cxi";

                case 1:
                    return "Manual.cfa";

                case 2:
                    return "DownloadPlay.cfa";

                case 6:
                    return "New3DSUpdateData.cfa";

                case 7:
                    return "UpdateData.cfa";

                default:
                    throw new InvalidOperationException($"Partition index {partitionIndex} is not associated.");
            }
        }

        private int GetPartitionIndex(string name)
        {
            switch (name)
            {
                case "GameData.cxi":
                    return 0;

                case "Manual.cfa":
                    return 1;

                case "DownloadPlay.cfa":
                    return 2;

                case "New3DSUpdateData.cfa":
                    return 6;

                case "UpdateData.cfa":
                    return 7;

                default:
                    throw new InvalidOperationException($"Partition name {name} is not associated.");
            }
        }

        private NcsdHeader ReadHeader(BinaryReaderX br)
        {
            return new NcsdHeader
            {
                rsa2048 = br.ReadBytes(0x100),
                magic = br.ReadString(4),
                ncsdSize = br.ReadInt32(),
                mediaId = br.ReadInt64(),
                partitionFsType = br.ReadBytes(8),
                partitionCryptType = br.ReadBytes(8),
                partitionEntries = ReadNcsdPartitionEntries(br),
                cardHeader = ReadCardHeader(br)
            };
        }

        private NcsdPartitionEntry[] ReadNcsdPartitionEntries(BinaryReaderX br)
        {
            var result = new NcsdPartitionEntry[8];

            for (var i = 0; i < 8; i++)
                result[i] = ReadNcsdPartitionEntry(br);

            return result;
        }

        private NcsdPartitionEntry ReadNcsdPartitionEntry(BinaryReaderX br)
        {
            return new NcsdPartitionEntry
            {
                offset = br.ReadInt32(),
                length = br.ReadInt32()
            };
        }

        private NcsdCardHeader ReadCardHeader(BinaryReaderX br)
        {
            return new NcsdCardHeader
            {
                exHeaderHash = br.ReadBytes(0x20),
                additionalHeaderSize = br.ReadInt32(),
                sectorZeroOffset = br.ReadInt32(),
                partitionFlags = br.ReadBytes(8),
                partitionIds = new[]
                {
                    br.ReadInt64(),
                    br.ReadInt64(),
                    br.ReadInt64(),
                    br.ReadInt64(),
                    br.ReadInt64(),
                    br.ReadInt64(),
                    br.ReadInt64(),
                    br.ReadInt64()
                },
                reserved1 = br.ReadBytes(0x20),
                reserved2 = br.ReadBytes(0xE),
                unk1 = br.ReadByte(),
                unk2 = br.ReadByte(),
                cardInfoHeader = ReadCardInfoHeader(br)
            };
        }

        private NcsdCardInfoHeader ReadCardInfoHeader(BinaryReaderX br)
        {
            return new NcsdCardInfoHeader
            {
                card2WriteAddress = br.ReadInt32(),
                cardBitMask = br.ReadInt32(),
                reserved1 = br.ReadBytes(0x108),
                titleVersion = br.ReadInt16(),
                cardRevision = br.ReadInt16(),
                reserved2 = br.ReadBytes(0xCEC),
                cardSeedKeyY = br.ReadBytes(0x10),
                encryptedCardSeed = br.ReadBytes(0x10),
                cardSeedAesMac = br.ReadBytes(0x10),
                cardSeedNonce = br.ReadBytes(0xC),
                reserved3 = br.ReadBytes(0xC4),
                firstNcchHeader = br.ReadBytes(0x100)
            };
        }
    }
}
