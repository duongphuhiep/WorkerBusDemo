using System;
using System.Collections.Generic;

namespace Core.Db;

public partial class CurrentProductList
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;
}
