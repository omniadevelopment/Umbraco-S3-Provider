using System.Configuration;
using Amazon.S3;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Storage.STC.S3.Services;

namespace Umbraco.Storage.STC.S3.Media
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class BucketMediaFileSystemComposer : IComposer
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


                composition.SetMediaFileSystem((f) => new BucketFileSystem(
                    config: config,
                    mimeTypeResolver: f.GetInstance<IMimeTypeResolver>(),
                    fileCacheProvider: null,
                    logger: f.GetInstance<ILogger>(),
                    s3Client: new AmazonS3Client(amazonS3Config)
                ));

                composition.Components().Append<BucketMediaFileSystemComponent>();

            }

        }

        private BucketFileSystemConfig CreateConfiguration()
        {
            var bucketName = ConfigurationManager.AppSettings[$"{AppSettingsKey}:BucketName"];
            var bucketPrefix = ConfigurationManager.AppSettings[$"{AppSettingsKey}:MediaPrefix"];
            var serviceURL = ConfigurationManager.AppSettings[$"{AppSettingsKey}:ServiceURL"];
            
            if (string.IsNullOrEmpty(bucketName))
                throw new ArgumentNullOrEmptyException("BucketName", $"The AWS S3 Bucket File System (Media) is missing the value '{AppSettingsKey}:BucketName' from AppSettings");

            if (string.IsNullOrEmpty(bucketPrefix))
                throw new ArgumentNullOrEmptyException("BucketPrefix", $"The AWS S3 Bucket File System (Media) is missing the value '{AppSettingsKey}:MediaPrefix' from AppSettings");

            if (string.IsNullOrEmpty(serviceURL))
                throw new ArgumentNullOrEmptyException("ServiceURL", $"The AWS S3 Bucket File System (Media) is missing the value '{AppSettingsKey}:ServiceURL' from AppSettings");

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
