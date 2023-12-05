using System;
using System.Collections.Generic;

namespace LokalizacjaWSklepie.Models;

public partial class ProductContainer
{
    public int ContainerId { get; set; }

    public int ProductId { get; set; }

    public virtual Container Container { get; set; }

    public virtual Product Product { get; set; }
}
