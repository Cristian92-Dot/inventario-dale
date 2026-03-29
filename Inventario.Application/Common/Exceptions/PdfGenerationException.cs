namespace Inventario.Application.Common.Exceptions;

public sealed class PdfGenerationException : AppException
{
    public PdfGenerationException(string message)
        : base(message)
    {
    }
}
