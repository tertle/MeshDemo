// <copyright file="Extensions.cs" company="Timothy Raines">
//     Copyright (c) Timothy Raines. All rights reserved.
// </copyright>

namespace BovineLabs.Common.Native
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Common.Utility;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;

    /// <summary>
    /// Extensions for Native Containers.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds a native version of <see cref="List{T}.AddRange(IEnumerable{T})"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="list">The <see cref="List{T}"/> to add to.</param>
        /// <param name="array">The native array to add to the list.</param>
        public static void AddRange<T>(this List<T> list, NativeArray<T> array)
            where T : struct
        {
            AddRange(list, array, array.Length);
        }

        /// <summary>
        /// Adds a native version of <see cref="List{T}.AddRange(IEnumerable{T})"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="list">The <see cref="List{T}"/> to add to.</param>
        /// <param name="array">The array to add to the list.</param>
        /// <param name="length">The length of the array to add to the list.</param>
        public static unsafe void AddRange<T>(this List<T> list, NativeArray<T> array, int length)
            where T : struct
        {
            list.AddRange(array.GetUnsafeReadOnlyPtr(), length);
        }

        /// <summary>
        /// Adds a native version of <see cref="List{T}.AddRange(IEnumerable{T})"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="list">The <see cref="List{T}"/> to add to.</param>
        /// <param name="nativeList">The native list to add to the list.</param>
        public static unsafe void AddRange<T>(this List<T> list, NativeList<T> nativeList)
            where T : struct
        {
            list.AddRange(nativeList.GetUnsafePtr(), nativeList.Length);
        }

        /// <summary>
        /// Adds a native version of <see cref="List{T}.AddRange(IEnumerable{T})"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="list">The <see cref="List{T}"/> to add to.</param>
        /// <param name="nativeSlice">The array to add to the list.</param>
        public static unsafe void AddRange<T>(this List<T> list, NativeSlice<T> nativeSlice)
            where T : struct
        {
            list.AddRange(nativeSlice.GetUnsafeReadOnlyPtr(), nativeSlice.Length);
        }

        /// <summary>
        /// Adds a native version of <see cref="List{T}.AddRange(IEnumerable{T})"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="list">The <see cref="List{T}"/> to add to.</param>
        /// <param name="dynamicBuffer">The dynamic buffer to add to the list.</param>
        public static unsafe void AddRange<T>(this List<T> list, DynamicBuffer<T> dynamicBuffer)
            where T : struct
        {
            list.AddRange(dynamicBuffer.GetUnsafePtr(), dynamicBuffer.Length);
        }

        /// <summary>
        /// Adds a range of values to a list using a buffer;
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="list">The list to add the values to.</param>
        /// <param name="arrayBuffer">The buffer to add from.</param>
        /// <param name="length">The length of the buffer.</param>
        public static unsafe void AddRange<T>(this List<T> list, void* arrayBuffer, int length)
            where T : struct
        {
            var index = list.Count;
            var newLength = index + length;

            // Resize our list if we require
            if (list.Capacity < newLength)
            {
                list.Capacity = newLength;
            }

            var items = NoAllocHelpers.ExtractArrayFromListT(list);
            var size = UnsafeUtility.SizeOf<T>();

            // Get the pointer to the end of the list
            var bufferStart = (IntPtr)UnsafeUtility.AddressOf(ref items[0]);
            var buffer = (byte*)(bufferStart + (size * index));

            UnsafeUtility.MemCpy(buffer, arrayBuffer, length * (long)size);

            NoAllocHelpers.ResizeList(list, newLength);
        }


        public static void Resize<T>(this List<T> list, int sz, T c = default)
        {
            int cur = list.Count;

            if (sz < cur)
            {
                list.RemoveRange(sz, cur - sz);
            }
            else if (sz > cur)
            {
                list.AddRange(Enumerable.Repeat(c, sz - cur));
            }
        }

        public static void EnsureLength<T>(this List<T> list, int sz, T c = default)
        {
            int cur = list.Count;

            if (sz > cur)
            {
                list.AddRange(Enumerable.Repeat(c, sz - cur));
            }
        }
    }
}