//----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//----------------------------------------------------------------------------------

namespace soft_delete
{
    using Azure;
    using Azure.Core;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using System;
    using System.Threading.Tasks;

    public class Program
    {
        const string ImageToUpload = "../../HelloWorld.png";
        const string TextToUpload = "../../HelloWorld.txt";
        const int RetentionDays = 14;

        static void Main(string[] args)
        {
            Console.WriteLine("Testing soft delete");
            SoftDeleteTest().GetAwaiter().GetResult();

            Console.WriteLine("\nPress any key to exit");
            Console.ReadLine();
        }

        private static async Task SoftDeleteTest()
        {

            // Retrieve a CloudBlobClient object and enable soft delete
            BlobServiceClient blobClient = GetCloudBlobClient();
            try
            {
                BlobServiceProperties serviceProperties = blobClient.GetProperties();
                serviceProperties.DeleteRetentionPolicy.Enabled = true;
                serviceProperties.DeleteRetentionPolicy.Days = RetentionDays;
                blobClient.SetProperties(serviceProperties);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
                throw;
            }

            // Create a container
            BlobContainerClient container = blobClient.GetBlobContainerClient("softdelete-" + System.Guid.NewGuid().ToString());
            try
            {
                await container.CreateIfNotExistsAsync();
            }
            catch (RequestFailedException)
            {
                Console.WriteLine("If you are using the storage emulator, please make sure you have started it. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            try
            {
                // Upload
                Console.WriteLine("\nUpload:");
                BlobClient blockBlob = container.GetBlobClient("HelloWorld");
                await blockBlob.UploadAsync(ImageToUpload, overwrite: true);
                PrintBlobsInContainer(container, BlobTraits.All);

                // Overwrite
                Console.WriteLine("\nOverwrite:");
                await blockBlob.UploadAsync(TextToUpload, overwrite: true);
                PrintBlobsInContainer(container, BlobTraits.All);

                // Snapshot
                Console.WriteLine("\nSnapshot:");
                await blockBlob.CreateSnapshotAsync();
                PrintBlobsInContainer(container, BlobTraits.All);

                // Delete (including snapshots)
                Console.WriteLine("\nDelete (including snapshots):");
                blockBlob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
                PrintBlobsInContainer(container, BlobTraits.All);

                // Undelete
                Console.WriteLine("\nUndelete:");
                await blockBlob.UndeleteAsync();
                PrintBlobsInContainer(container, BlobTraits.All);

                // Recover
                Console.WriteLine("\nCopy the most recent snapshot over the base blob:");
                blockBlob.StartCopyFromUri(blockBlob.Uri);
                PrintBlobsInContainer(container, BlobTraits.All);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("\nDone.\n\nEnter 'd' to cleanup resources. Doing so will also turn off the soft delete feature. Enter any other key to leave the container intact.");
            String cleanup = Console.ReadLine();
            if (cleanup == "d")
            {
                try
                {
                    // Delete the container
                    await container.DeleteIfExistsAsync();
                    Console.WriteLine("\nContainer deleted.");

                    // Turn off soft delete
                    BlobServiceProperties serviceProperties = blobClient.GetProperties();
                    serviceProperties.DeleteRetentionPolicy.Enabled = false;
                    blobClient.SetProperties(serviceProperties);
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine("Error returned from the service: {0}", ex.Message);
                    throw;
                }
            }
        }

        // Helper method to retrieve retrieve the CloudBlobClient object in order to interact with the storage account.
        // The retry policy on the CloudBlobClient object is set to an Exponential retry policy with a back off of 2 seconds
        // and a max attempts of 10 times.
        private static BlobServiceClient GetCloudBlobClient()
        {
            try
            {
                BlobClientOptions blobClientOptions = new BlobClientOptions();
                blobClientOptions.Retry.Delay = TimeSpan.FromSeconds(1);
                blobClientOptions.Retry.MaxRetries = 1;
                BlobServiceClient blobClient = new BlobServiceClient("StorageConnectionString", blobClientOptions);
                return blobClient;
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
                throw;
            }
        }

        static void PrintBlobsInContainer(BlobContainerClient container, BlobTraits blobListingDetails)
        {
            foreach (BlobHierarchyItem blob in container.GetBlobsByHierarchy(blobListingDetails, BlobStates.All))
            {
                Console.WriteLine("- {0} (is soft deleted: {1}, is snapshot: {2})", blob.Blob.Name, blob.Blob.Deleted, String.IsNullOrEmpty(blob.Blob.Snapshot) ? false : true);
            }
        }
    }
}