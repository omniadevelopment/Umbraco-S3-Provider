using System.Configuration;
using Amazon.S3;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Forms.Core.Components;
using Umbraco.Forms.Data.FileSystem;
using Umbraco.Storage.STC.S3.Services;

namespace Umbraco.Storage.STC.S3.Forms
{

    [ComposeAfter(typeof(UmbracoFormsComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class BucketFormsFileSystemComposer : IComposer
    {
        private const string AppSettingsKey = "STC:ObjectStore";
        private readonly char[] Delimiters = "/".ToCharArray();

        public void Compose(Composition composition)
        {

            var bucketName = ConfigurationManager.AppSettings[$"{AppSettingsKey}:BucketName"];
            if (bucketName != null)
            {
                var config = CreateConfiguration();

                composition.RegisterUnique(config);
                composition.Register<IMimeTypeResolver>(new DefaultMimeTypeResolver());

                AmazonS3Config amazonS3Config = new AmazonS3Config()
                {
                    ServiceURL = config.ServiceURL
                };

                composition.RegisterUniqueFor<IFileSystem, FormsFileSystemForSavedData>(f => new BucketFileSystem(
                    config: config,
                    mimeTypeResolver: f.GetInstance<IMimeTypeResolver>(),
                    fileCacheProvider: null,
                    logger: f.GetInstance<ILogger>(),
                    s3Client: new AmazonS3Client(amazonS3Config)
                ));
            }

        }

        private BucketFileSystemConfig CreateConfiguration()
        {
            var bucketName = ConfigurationManager.AppSettings[$"{AppSettingsKey}:BucketName"];
            var bucketPrefix = ConfigurationManager.AppSettings[$"{AppSettingsKey}:FormsPrefix"];
            var serviceURL = ConfigurationManager.AppSettings[$"{AppSettingsKey}:ServiceURL"];
          
            if (string.IsNullOrEmpty(bucketName))
                throw new ArgumentNullOrEmptyException("BucketName", $"The AWS S3 Bucket File System (Forms) is missing the value '{AppSettingsKey}:BucketName' from AppSettings");

            if (string.IsNullOrEmpty(bucketPrefix))
                throw new ArgumentNullOrEmptyException("BucketPrefix", $"The AWS S3 Bucket File System (Forms) is missing the value '{AppSettingsKey}:FormsPrefix' from AppSettings");

            if (string.IsNullOrEmpty(serviceURL))
                throw new ArgumentNullOrEmptyException("ServiceURL", $"The AWS S3 Bucket File System (Forms) is missing the value '{AppSettingsKey}:ServiceURL' from AppSettings");

            return new BucketFileSystemConfig
            {
                BucketName = bucketName,
                BucketPrefix = bucketPrefix.Trim(Delimiters),
                CannedACL = new S3CannedACL("public-read"),
                ServerSideEncryptionMethod = "",
                ServiceURL = serviceURL
            };        
        }
    }
}
