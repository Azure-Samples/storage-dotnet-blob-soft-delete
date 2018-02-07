# Azure Storage Blob Soft Delete

This project uploads, overwrites, snapshots, deletes, and restores a blob named “HelloWorld” when soft delete is turned on. It also includes two PowerShell scripts to demonstrate soft delete functionalities.

## Features

This project framework provides the following features:

* Blob soft delete

## Getting Started

### Prerequisites

- Azure Storage client library version 9.0.0 or later
- .NET Framework

### Running this sample

By default, this sample is configured to run against the storage emulator. You can also modify it to run against your Azure Storage account.

To run the sample using the storage emulator (default option):
1. Start the Azure storage emulator (once only) by pressing the Start button or the Windows key and searching for it by typing "Azure storage emulator". Select it from the list of applications to start it.
2. Set breakpoints and run the project using F10.

To run the sample using a storage account
1. Open the app.config file and comment out the connection string for the emulator (UseDevelopmentStorage=True) and uncomment the connection string for the storage service (AccountName=[]...)
2. Create a storage account through the Azure Portal and provide your [AccountName] and [AccountKey] in the App.Config file. See https://docs.microsoft.com/en-us/azure/storage/common/storage-create-storage-accountfor more information
3. Set breakpoints and run the project using F10.


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

- https://review.docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-soft-delete
