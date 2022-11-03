using Azure.Storage.Blobs;
using System.Diagnostics;
using System.Threading;

namespace StorageListBlobs
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING") ??
                throw new InvalidOperationException("Env var STORAGE_CONNECTION_STRING not set");

            var serviceClient = new BlobServiceClient(connectionString);

            var containerName = Guid.NewGuid().ToString();
            var containerClient = serviceClient.GetBlobContainerClient(containerName);

            Console.WriteLine($"Creating container {containerName}...");
            await containerClient.CreateAsync();
            Console.WriteLine();

            for (var i=0; i < 5; i++)
            {
                var blobName = Guid.NewGuid().ToString();
                Console.WriteLine($"Uploading blob {blobName}...");
                await containerClient.UploadBlobAsync(blobName, Stream.Null);
            }
            Console.WriteLine();

            try
            {
                Console.WriteLine("Listing blobs and printing latency.  Press any key to quit...");
                Console.WriteLine();
                
                var sw = new Stopwatch();
                while (!Console.KeyAvailable)
                {
                    sw.Restart();
                    // Enumerate collection to ensure all BlobItems are downloaded
                    await foreach (var _ in containerClient.GetBlobsAsync())
                    {
                    }
                    sw.Stop();

                    Console.Write(sw.ElapsedMilliseconds);
                    Console.Write(" ");
                }                
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine($"Deleting container {containerName}...");
                await containerClient.DeleteAsync();
            }
        }
    }
}