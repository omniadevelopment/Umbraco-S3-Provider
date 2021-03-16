namespace Umbraco.Storage.STC.S3.Services
{
    public interface IMimeTypeResolver
    {
        string Resolve(string filename);
    }
}
