---
services: storage
platforms: dotnet
author: michaelhauss
---

# Azure Storage Blob Soft Delete

This project uploads, overwrites, snapshots, deletes, and restores a blob named “HelloWorld” when soft delete is turned on.

## Features

This project framework provides the following features:

* Blob soft delete

## Getting Started

### Prerequisites

- .NET Framework

### Running this sample

By default, this sample is configured to run against the storage emulator. You can also modify it to run against your Azure Storage account.

To run the sample using the storage emulator (default option):
1. Start the Azure storage emulator (once only) by pressing the Start button or the Windows key and searching for it by typing "Azure storage emulator". Select it from the list of applications to start it.
2. Set breakpoints and run the project using F10.

To run the sample using a storage account
1. Create a storage account through the Azure Portal
2. Open the app.config file and change the value of the "StorageConnectionString" key (currently "UseDevelopmentStorage=True;") to the connection string for your storage account.
 https://docs.microsoft.com/en-us/azure/storage/common/storage-create-storage-account for more information
3. Set breakpoints and run the project using F10.

**Currently the storage emulator does not support soft delete. You will need to run the sample using a storage account**

The sample is configured to clean up resources after running. If you'd like to inspect the results, set breakpoints or comment


## Demo

The application should produce output similar to the following:

```bash
Upload:
- HelloWorld (is soft deleted: False, is snapshot: False)

Overwrite:
- HelloWorld (is soft deleted: True, is snapshot: True)
- HelloWorld (is soft deleted: False, is snapshot: False)

Snapshot:
- HelloWorld (is soft deleted: True, is snapshot: True)
- HelloWorld (is soft deleted: False, is snapshot: True)
- HelloWorld (is soft deleted: False, is snapshot: False)

Delete (including snapshots):
- HelloWorld (is soft deleted: True, is snapshot: True)
- HelloWorld (is soft deleted: True, is snapshot: True)
- HelloWorld (is soft deleted: True, is snapshot: False)

Undelete:
- HelloWorld (is soft deleted: False, is snapshot: True)
- HelloWorld (is soft deleted: False, is snapshot: True)
- HelloWorld (is soft deleted: False, is snapshot: False)

Copy a snapshot over the base blob:
- HelloWorld (is soft deleted: False, is snapshot: True)
- HelloWorld (is soft deleted: False, is snapshot: True)
- HelloWorld (is soft deleted: True, is snapshot: True)
- HelloWorld (is soft deleted: False, is snapshot: False)
```

## Resources

- https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-soft-delete
