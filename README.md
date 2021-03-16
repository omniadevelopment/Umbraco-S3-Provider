# Umbraco STC S3 Object Store Provider

[Amazon Web Services S3](http://aws.amazon.com/s3/) IFileSystem provider for Umbraco 8. Used to offload media and/or forms to the cloud! You don't have to be hosting your code in EC2 to get the benefits like handling large media libraries, freeing up disk space and removing static files from your deployment process.

Many thanks must go to [Elijah Glover](https://github.com/ElijahGlover/) for initially creating this project for Umbraco 7 and [Danny Quarton](https://github.com/DannerrQ) for upgrading it to support Umbraco 8. I built this based on their earlier work.

If you encounter any problems feel free to raise an issue, or maybe even a pull request if you're feeling generous!


## Installation & Configuration

Install via NuGet.org
```powershell
Install-Package Our.Umbraco.FileSystemProviders.STC.S3.Media
```
or
```powershell
Install-Package Our.Umbraco.FileSystemProviders.STC.S3.Forms
```

Add the following keys to `~/Web.config`
```xml
<?xml version="1.0"?>
<configuration>
  <appSettings>
    <add key="STC:ObjectStore:BucketName" value="" />
    <add key="STC:ObjectStore:MediaPrefix" value="media" />
    <add key="STC:ObjectStore:FormsPrefix" value="forms" />
    <add key="STC:ObjectStore:ServiceURL" value="" />
    <add key="AWSAccessKey" value="" />
    <add key="AWSSecretKey" value="" />
  </appSettings>
</configuration>
```

| Key | Required | Default | Description
| --- | --- | --- | --- |
| `MediaPrefix` | Sometimes | N/A | The prefix for any media files being added to S3. Essentially a root directory name. Required when using `Umbraco.Storage.S3.Media` |
| `FormsPrefix` | Sometimes | N/A | The prefix for any Umbraco Forms data files being added to S3. Essentially a root directory name. Required when using `Umbraco.Storage.S3.Forms` |
| `BucketName` | Yes | N/A | The name of your S3 bucket. |
| `ServiceUrl` | Yes | N/A | The service URL that points to STC's Object Store. |

You'll need to add the following to `~/Web.config`
```xml
<?xml version="1.0"?>
<configuration>
  <location path="Media">
    <system.webServer>
      <handlers>
        <remove name="StaticFileHandler" />
        <add name="StaticFileHandler" path="*" verb="*" preCondition="integratedMode" type="System.Web.StaticFileHandler" />
      </handlers>
    </system.webServer>
  </location>
</configuration>
```
You also need to add the following to `~/Media/Web.config`
```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <clear />
      <add name="StaticFileHandler" path="*" verb="*" preCondition="integratedMode" type="System.Web.StaticFileHandler" />
      <add name="StaticFile" path="*" verb="*" modules="StaticFileModule,DefaultDocumentModule,DirectoryListingModule" resourceType="Either" requireAccess="Read" />
    </handlers>
  </system.webServer>
</configuration>
```


## AWS Authentication

Ok so where are the [IAM access keys?](http://docs.aws.amazon.com/IAM/latest/UserGuide/ManagingCredentials.html) Depending on how you host your project they already exist if deploying inside an EC2 instance via environment variables specified during deployment and creation of infrastructure.
It's also a good idea to use [AWS best security practices](http://docs.aws.amazon.com/general/latest/gr/aws-access-keys-best-practices.html). Like not using your root access account, use short lived access keys and don't EVER commit them to source control.

You need to put them into `~/Web.config`
```xml
<?xml version="1.0"?>
<configuration>
  <appSettings>
    <add key="AWSAccessKey" value="" />
    <add key="AWSSecretKey" value="" />
  </appSettings>
</configuration>
```

## Using ImageProcessor
Support for remote files has been added to ImageProcessor in version > `2.3.2`. You'll also want to ensure that you are using Virtual Path Provider as ImageProcessor only hijacks requests when parameters are present in the querystring (like width, height, etc).

```powershell
Install-Package ImageProcessor.Web.Config
```

Replace config file located `~/config/imageprocessor/security.config`
```xml
<?xml version="1.0" encoding="utf-8"?>
<security>
  <services>
    <service prefix="media/" name="CloudImageService" type="ImageProcessor.Web.Services.CloudImageService, ImageProcessor.Web">
      <settings>
        <setting key="MaxBytes" value="8194304"/>
        <setting key="Timeout" value="30000"/>
        <setting key="Host" value="http://{Your STC Service URL}/{Your Key Prefix}/"/>
      </settings>
    </service>
  </services>
</security>
```

## Future work on this project
Due to not having enough time to make this package generic, This package only supports connecting to S3 using the service url parameter. If needed in the future I might add more configurations to the package in order for it to be used as a connector to any S3 store.