using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStar.DependencyInjection
{
    public interface IDependent<T>
    {
        T Dependency { get; set; }
    }
}
