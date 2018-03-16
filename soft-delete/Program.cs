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
    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

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
            CloudBlobClient blobClient = GetCloudBlobClient();
            try
            {
                ServiceProperties serviceProperties = blobClient.GetServiceProperties();
                serviceProperties.DeleteRetentionPolicy.Enabled = true;
                serviceProperties.DeleteRetentionPolicy.RetentionDays = RetentionDays;
                blobClient.SetServiceProperties(serviceProperties);
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
                throw;
            }

            // Create a container
            CloudBlobContainer container = blobClient.GetContainerReference("softdelete-" + System.Guid.NewGuid().ToString());
            try
            {
                await container.CreateIfNotExistsAsync();
            }
            catch (StorageException)
            {
                Console.WriteLine("If you are using the storage emulator, please make sure you have started it. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            try
            {
                // Upload
                Console.WriteLine("\nUpload:");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference("HelloWorld");
                await blockBlob.UploadFromFileAsync(ImageToUpload);
                PrintBlobsInContainer(container, BlobListingDetails.All);

                // Overwrite
                Console.WriteLine("\nOverwrite:");
                await blockBlob.UploadFromFileAsync(TextToUpload);
                PrintBlobsInContainer(container, BlobListingDetails.All);

                // Snapshot
                Console.WriteLine("\nSnapshot:");
                await blockBlob.SnapshotAsync();
                PrintBlobsInContainer(container, BlobListingDetails.All);

                // Delete (including snapshots)
                Console.WriteLine("\nDelete (including snapshots):");
                blockBlob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
                PrintBlobsInContainer(container, BlobListingDetails.All);

                // Undelete
                Console.WriteLine("\nUndelete:");
                await blockBlob.UndeleteAsync();
                PrintBlobsInContainer(container, BlobListingDetails.All);

                // Recover
                Console.WriteLine("\nCopy the most recent snapshot over the base blob:");
                IEnumerable<IListBlobItem> allBlobVersions = container.ListBlobs(
                    prefix: blockBlob.Name, useFlatBlobListing: true, blobListingDetails: BlobListingDetails.Snapshots);
                CloudBlockBlob copySource = allBlobVersions.First(version => ((CloudBlockBlob)version).IsSnapshot && 
                    ((CloudBlockBlob)version).Name == blockBlob.Name) as CloudBlockBlob;
                blockBlob.StartCopy(copySource);
                PrintBlobsInContainer(container, BlobListingDetails.All);
            }
            catch (StorageException ex)
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
                    ServiceProperties serviceProperties = blobClient.GetServiceProperties();
                    serviceProperties.DeleteRetentionPolicy.Enabled = false;
                    blobClient.SetServiceProperties(serviceProperties);
                }
                catch (StorageException ex)
                {
                    Console.WriteLine("Error returned from the service: {0}", ex.Message);
                    throw;
                }
            }
        }

        // Helper method to retrieve retrieve the CloudBlobClient object in order to interact with the storage account.
        // The retry policy on the CloudBlobClient object is set to an Exponential retry policy with a back off of 2 seconds
        // and a max attempts of 10 times.
        private static CloudBlobClient GetCloudBlobClient()
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 10);
                blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;
                return blobClient;
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
                throw;
            }
        }

        static void PrintBlobsInContainer(CloudBlobContainer container, BlobListingDetails blobListingDetails)
        {
            foreach (CloudBlockBlob blob in container.ListBlobs(useFlatBlobListing: true, blobListingDetails: blobListingDetails))
            {
                Console.WriteLine("- {0} (is soft deleted: {1}, is snapshot: {2})", blob.Name, blob.IsDeleted, blob.IsSnapshot);
            }
        }
    }
}