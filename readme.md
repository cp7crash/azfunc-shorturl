# overview
A simple url redirector which uses Azure table storage as its db and includes a cache.

# table storage setup
Currently uses a single partition, the key is the (case sensitive) shorturl and the * key is the deafult redirect if listed key is not found. Leading and trailing forward / and backward \ slashes are removed from keys and shoudl not be present in the table.

# custom domain
No doubt there are plenty of other ways, but tested wit

# settings.json
``` csharp
...
"Values": {
    "PartitionKey": "ctlol",
    "AccountName": "ctlol",
    "TableName": "ctlol",
    "AccountKey": "<a storage account key>",
    "DefaultRedirect": "https://cloudthing.com"
  },
...
```

