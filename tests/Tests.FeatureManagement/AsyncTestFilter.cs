// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.FeatureManagement;
using System;
using System.Threading.Tasks;

namespace Tests.FeatureManagement
{
    class AsyncTestFilter : IAsyncFeatureFilter
    {
        public Func<FeatureFilterEvaluationContext, Task<bool>> Callback { get; set; }

        public Task<bool> Evaluate(FeatureFilterEvaluationContext context)
        {
            return Callback?.Invoke(context) ?? Task.FromResult(false);
        }
    }
}
