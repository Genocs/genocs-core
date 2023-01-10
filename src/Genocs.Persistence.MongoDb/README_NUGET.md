# .NET Core Persistence MongoDB library

This package contains a repository pattern implementation using MongoDB. The library is designed by Genocs.
The libraries are built using .NET standard 2.1.


## Description

Persistence MongoDB Core NuGet package contains general purpose functionalities to be used on DDD service.


## Support

Please check the GitHub repository getting more info.


### DataProvider Settings
Following is about how to setup **MongoDbSettings**

``` json
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost",
    "Database": "demo_database",
    "EnableTracing": "false"
  }
```

## Release notes

### [2023-01-10] 3.0.1
- Added Service collection extension


### [2023-01-07] 3.0.0
- Changed DBSettings to MongoDbSettings