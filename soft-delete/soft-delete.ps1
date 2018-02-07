# Specify storage account information
$StorageAccountName = "<storage-account-name>"
$StorageAccountKey = "<storage-account-key>"
$StorageContainerName = "<storage-container-name>"

# Create a context by specifying storage account name and key
$ctx = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey

# Turn on soft delete
Enable-AzureStorageDeleteRetentionPolicy -RetentionDays 7 -context $ctx

# Show soft delete is turned on
Get-AzureStorageServiceProperty -ServiceType "Blob" -context $ctx

# Get the blobs in the container and show their properties
$Blobs = Get-AzureStorageBlob -Container $StorageContainerName -Context $ctx -IncludeDeleted
$Blobs

# Observe the new properties for soft deleted data
$Blobs.ICloudBlob.Properties

# Undelete the blobs in the container and show their properties
$Blobs.ICloudBlob.Undelete()
$Blobs = Get-AzureStorageBlob -Container $StorageContainerName -Context $ctx
$Blobs
