using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.IO.Compression;

namespace DictationProcessorLib
{
    public class UploadProcessorOptions
    {
        public string UploadDirectory;
        public string OutputDirectory;
    }

    public class UploadProcessor
    {
        string uploadDirectory;
        string outputDirectory;

        public UploadProcessor(
            UploadProcessorOptions options
        )
        {
            this.uploadDirectory = options.UploadDirectory;
            this.outputDirectory = options.OutputDirectory;
        }

        public List<string> Process()
        {
            List<string> processedFiles = new List<string>();

            this.CleanOutputDirectory();

            foreach (var subDir in Directory.GetDirectories(this.uploadDirectory))
            {
                var metadataText = File.ReadAllText(Path.Combine(subDir, "metadata.json"));
                var metadataCollection = JsonSerializer.Deserialize<List<Metadata>>(metadataText);

                foreach (var metadata in metadataCollection)
                {
                    System.Guid uniqueId = Guid.NewGuid();
                    string audioFileName = metadata.File.FileName;
                    string newAudioFileName = $"{uniqueId}.WAV";
                    string audioFilePath = Path.Combine(subDir, audioFileName);
                    string newAudioFilePath = Path.Combine(this.outputDirectory, newAudioFileName);
                    string md5Checksum = GetChecksum(audioFilePath);

                    Console.WriteLine($"Creating {newAudioFilePath}");

                    metadata.File.FileName = newAudioFileName;

                    processedFiles.Add(audioFilePath);

                    string serializedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    Console.WriteLine($"Creating {newAudioFilePath}.json");

                    File.WriteAllText($"{newAudioFilePath}.json", serializedJson);

                    if (md5Checksum.Replace("-", "").ToLower() != metadata.File.Md5Checksum)
                    {
                        throw new Exception("File checksum does not match metadata");
                    }

                    CreateCompressedFile(audioFilePath, newAudioFilePath);
                }
            }

            return processedFiles;
        }

        public void CleanOutputDirectory()
        {
            Console.WriteLine(this.outputDirectory);

            Directory.CreateDirectory(this.outputDirectory);

            string[] existingFilePatterns = { "*.WAV.gz", "*.WAV.json" };

            ParallelQuery<string> existingFiles = existingFilePatterns.AsParallel()
                .SelectMany(pattern =>
                    Directory.EnumerateFiles(this.outputDirectory, pattern, SearchOption.TopDirectoryOnly));

            foreach (string file in existingFiles)
            {
                Console.WriteLine($"Removing existing file {file}");
                File.Delete(file);
            }
        }

        static List<Metadata> GetMetadataCollection(
            string directory,
            string fileName
        )
        {
            var metadataText = File.ReadAllText(Path.Combine(directory, fileName));
            return JsonSerializer.Deserialize<List<Metadata>>(metadataText);
        }

        static void CreateCompressedFile(
            string inputFilePath,
            string outputFilePath
        )
        {
            outputFilePath += ".gz";

            var inputFileStream = File.Open(inputFilePath, FileMode.Open);
            var outputFileStream = File.Create(outputFilePath);
            var gzipStream = new GZipStream(outputFileStream, CompressionLevel.Optimal);
        }

        static string GetChecksum(
            string filePath
        )
        {
            var fileStream = File.Open(filePath, FileMode.Open);
            var md5 = System.Security.Cryptography.MD5.Create();
            var md5Bytes = md5.ComputeHash(fileStream);

            fileStream.Close();

            return BitConverter.ToString(md5Bytes);
        }
    }
}
