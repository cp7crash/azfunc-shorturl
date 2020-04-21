# overview
A simple url redirector which uses Azure table storage as its db and includes a cache.

# table storage setup
Currently uses a single partition, the key is the shorturl (always lower case) and the * key is the deafult redirect if listed key is not found.
Publicly accessible.

# custom domain
No doubt there are plenty of other ways, but tested wit

# settings.json
``` csharp
...
"Values": {
    "TableUri": "{your table store connection string}"
    "PartitionKey": "{your table store partition key}"
  },
...
```

