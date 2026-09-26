using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpile.Application.Purchasing.RTV;

public sealed record RecordRtvResult(
    bool IsSuccessful,
    string Message = "",
    Guid? RtvId = null
);
