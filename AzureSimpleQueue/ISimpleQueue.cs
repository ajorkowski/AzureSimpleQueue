﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AzureSimpleQueue
{
    public interface ISimpleQueue<T>
    {
        void Queue(Expression<Action<T>> serviceAction);
    }
}
