﻿// Copyright 2022 lllggghhhaaa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
// limitations under the License.

namespace Ceiralizer.Utils;

public static class EnumerableExtensionMethods
{
    public static ArraySegment<T> GetSegment<T>(this IEnumerable<T> arr, int offset, int? count = null)
    {
        IEnumerable<T> enumerable = arr as T[] ?? arr.ToArray();
        
        if (count == null) count = enumerable.Count() - offset;
        return new ArraySegment<T>(enumerable.ToArray(), offset, count.Value);
    }
}