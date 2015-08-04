using ReServer.Core.Responses;

namespace ReServer.Conversion
{
    public interface IConverter
    {
        RSResponse Convert(RSResponse original);
    }
}
