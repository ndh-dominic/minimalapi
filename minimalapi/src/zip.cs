using System.IO.Compression;

static class Zip
{
    // Write a zip file with specified number of files of 1 MiB each with random content.
    public static async Task WriteAsync(Stream stream, int files, CancellationToken cancellationToken)
    {
        var seed = 123;
        var rnd = new Random(seed);
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            var buffer = new byte[0x100000];
            for (var i = 0; i < files; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException(); // Cancel in between iteration

                rnd.NextBytes(buffer);
                var name = i.ToString();
                var entry = archive.CreateEntry(name);
                using (var entryStream = entry.Open())
                {
                    await entryStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                }
            }
        }
    }
}