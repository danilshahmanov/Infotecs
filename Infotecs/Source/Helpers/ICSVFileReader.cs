namespace Infotecs.Source.Helpers
{
    public interface ICSVFileReader
    {
        Task ReadValuesWithBufferAsync(
            IFormFile file,
            int minValuesCount,
            int maxValuesCount,
            int bufferSize
        );
    }
}
