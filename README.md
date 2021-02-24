# MongoFs

MongoFs is an application that mounts MongoDB as read-only filesystem. It uses
[Dokany](https://github.com/dokan-dev/dokany) to handle low level driver stuff.

MongoFs is more of proof-of-concept prototype than something usable.

## Installation

1. Install [Dokany](https://github.com/dokan-dev/dokany#installation) version
   1.4.1 or greater.

2. Clone this repository.

3. Issue a command
```bash
dotnet build .\MongoFs\MongoFs.csproj -p:Configuration=Release;Platform=x64
```

4. Executable will be at `\MongoFs\bin\x64\Release\net5.0\MongoFs.exe`

## Usage

```
MongoFs 1.0.0
Copyright (C) 2021 MongoFs

  --log-file                 Path to a log file.

  --log-console              (Default: false) Enable console logging.

  --log-level                (Default: Information) Log level.

  -c, --connection-string    Required. Connection string to MongoDb (eg. mongodb://localhost:27017)

  -n, --name                 Required. Name of a MongoDb instance.

  -p, --path                 Required. Mount point (M:\) or mount path (C:\mongodb). Note the ending slash.

  -t, --threads              (Default: 2) Number of threads to be used internally by Dokan library. More thread will handle more event at the same time.

  --help                     Display this help screen.

  --version                  Display version information.
```

For example,

```bash
mongofs -c mongodb://localhost:27017 -n "My Localhost mongodb" -p M:\
```

This command connects to mongodb instance at *localhost:27017* and mounts it at
*M:\\* named *My Localhost mongodb*

PS. Ending slash at mount point is mandatory.

## Path strings to MongoDB mappings

### \

| Path | Description |
| -------------------|------------------------------------------|
| \                  | Lists databases |
| \buildInfo.json    | Result of *buildInfo* command as json |
| \currentOp.json    | Result of *currentOp* command as json |
| \hostInfo.json     | Result of *hostInfo* command as json  |
| \listCommands.json | Result of *listCommands* command as json |
| \serverStatus.json | Result of *serverStatus* command as json |

### \database

| Path      | Description |
|-----------|-----------------------------------------|
| \database | List collections in *database* database |

### \database\collection

All paths below are related to *collection* collection in *database* database.

| Path          | Description                   |
|---------------|-------------------------------|
| \indexes.json | Collection indexes            |
| \stats.json   | Collection stats              |
| \data         | All documents of collection   |
| \query        | Folder for querying documents |

**WARNING**: Be extra carefull with *data\\* directory as opening it will cause
enumeration of *all documents* in collection. And if something on your pc decides to
index newly mounted drive...oh boy.

### \database\collection\query

To query(filter) documents you have to manually type path in Windows file
explorer or whatever file explorer you use.

| Path | Description |
|------|-------------|
| *value* | Filters documents where *_id* equals any of all possible BSON values *value* is convertible to. |
| *field*\\*value* | Filters documents where *field* equals any of all possible BSON value *value* is convertible to. |

#### Examples

1. `\logs\backend\query\507f191e810c19729de860ea` is converted to

```json
{$or:[
    {"_id":ObjectId("507f191e810c19729de860ea")},
    {"_id":"507f191e810c19729de860ea"}
]}
```

and that filter is sent to *backend* collection in *logs* database.

2. `\logs\backend\query\Level\Error` is converted to

```json
{$or:[
    {"Level":"Error"}
]}
```

3. `\logs\backend\query\IsRequestHalted\true` is converted to

```json
{$or:[
    {"IsRequestHalted":true}
]}
```

4. `\logs\backend\query\_id\3456` is converted to

```json
{$or:[
    {"_id":NumberInt(3456)},
    {"_id":NumberLong(3456)},
    {"_id":NumberDecimal(3456)},
    {"_id":"3456"}
]}
```
