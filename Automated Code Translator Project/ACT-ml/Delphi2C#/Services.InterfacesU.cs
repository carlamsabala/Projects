using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Entities;

namespace Services.InterfacesU
{
    [Guid("347532A0-1B28-40C3-A2E9-51DF62365FE7")]
    public interface IPeopleService
    {
        IList<TPerson> GetAll();
    }

    [Guid("146C21A5-07E8-456D-8E6D-A72820BD17AA")]
    public interface IConnectionService
    {
        string GetConnectionName();
    }
}
