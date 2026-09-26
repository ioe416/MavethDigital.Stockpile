namespace Stockpile.Application.Purchasing.Receiving.UndoReceipt
{
    public sealed record UndoReceiptResult(
        bool IsSuccessful,
        string Message = ""
    );
}
