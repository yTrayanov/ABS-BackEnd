using System;
using System.Collections.Generic;
using System.Text;

namespace ABS.Data.DynamoDBHelpers
{
    public enum UpdateReturnValues
    {
        NONE,
        ALL_OLD,
        UPDATED_OLD,
        ALL_NEW,
        UPDATED_NEW
    }
}
