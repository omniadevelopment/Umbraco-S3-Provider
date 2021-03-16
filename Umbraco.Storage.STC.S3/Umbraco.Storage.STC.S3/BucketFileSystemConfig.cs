using Amazon.S3;

namespace Umbraco.Storage.STC.S3
{
    public class BucketFileSystemConfig
    {
        public string BucketName { get; set; }
        
        public string BucketPrefix { get; set; }
        
        public S3CannedACL CannedACL { get; set; }

        public ServerSideEncryptionMethod ServerSideEncryptionMethod { get; set; }
        
        public string ServiceURL { get; set; }
    }
}
