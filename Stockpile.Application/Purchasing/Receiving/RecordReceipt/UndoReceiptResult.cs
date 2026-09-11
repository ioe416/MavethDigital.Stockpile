namespace Stockpile.Application.Purchasing.Receiving.RecordReceipt
{
    public sealed record UndoReceiptResult(
        bool IsSuccessful,
        string Message = ""
    );
}
