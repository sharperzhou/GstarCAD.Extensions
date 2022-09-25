﻿using System;
using System.Collections.Generic;
using System.Linq;
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;
using Exception = GrxCAD.Runtime.Exception;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Defines methods to add or removes items from a sequence of disposable objects.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public interface IDisposableCollection<T> : ICollection<T>, IDisposable
        where T : IDisposable
    {
        /// <summary>
        /// Adds items to the sequence.
        /// </summary>
        /// <param name="items">Items to add.</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Removes items from the sequence.
        /// </summary>
        /// <param name="items">Items to remove.</param>
        /// <returns>The sequence of removed items.</returns>
        IEnumerable<T> RemoveRange(IEnumerable<T> items);
    }

    /// <summary>
    /// Provides extension methods for the IEnumerable(T) type.
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Opens the objects which type matches to the given one, and return them.
        /// </summary>
        /// <typeparam name="T">Type of object to return.</typeparam>
        /// <param name="source">Sequence of ObjectIds.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayers">Value indicating if locked layers should be opened.</param>
        /// <returns>The sequence of opened objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="GrxCAD.Runtime.Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(
            this IEnumerable<ObjectId> source,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayers = false) where T : DBObject
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));

            if (!source.Any())
            {
                yield break;
            }

            var tr = source.First().Database.GetTopTransaction();
            var rxClass = RXObject.GetClass(typeof(T));
            foreach (ObjectId id in source)
            {
                if (!id.ObjectClass.IsDerivedFrom(rxClass))
                {
                    continue;
                }

                if (!id.IsErased || openErased)
                    yield return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
            }
        }

        /// <summary>
        /// Upgrades the open mode of all objects in the sequence.
        /// </summary>
        /// <typeparam name="T">Type of objects.</typeparam>
        /// <param name="source">Sequence of DBObjects to upgrade.</param>
        /// <returns>The sequence of opened for write objects (objets on locked layers are discard).</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="GrxCAD.Runtime.Exception">eNoActiveTransactions is thrown if there's no active transaction.</exception>
        public static IEnumerable<T> UpgradeOpen<T>(this IEnumerable<T> source) where T : DBObject
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            foreach (T item in source)
            {
                try
                {
                    item.OpenForWrite();
                }
                catch (Exception ex)
                {
                    if (ex.ErrorStatus != ErrorStatus.OnLockedLayer)
                        throw;
                    continue;
                }

                yield return item;
            }
        }

        /// <summary>
        /// Disposes of all items of the sequence.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="source">Sequence of disposable objects.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static void DisposeAll<T>(this IEnumerable<T> source) where T : IDisposable
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            if (!source.Any())
            {
                return;
            }

            System.Exception last = null;
            foreach (T item in source)
            {
                try
                {
                    item?.Dispose();
                }
                catch (System.Exception ex)
                {
                    last = last ?? ex;
                }
            }

            if (last != null)
                throw last;
        }

        /// <summary>
        /// Runs the action for each item of the collection.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="source">Sequence to process.</param>
        /// <param name="action">Action to run.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="action"/> is null.</exception>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfArgumentNull(action, nameof(action));
            foreach (T item in source) action(item);
        }

        /// <summary>
        /// Runs the indexed action for each item of the collection.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="source">Sequence to process.</param>
        /// <param name="action">Indexed action to run.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="action"/> is null.</exception>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfArgumentNull(action, nameof(action));
            int i = 0;
            foreach (T item in source) action(item, i++);
        }

        /// <summary>
        /// Gets the greatest item of the sequence using the default comparer with the <paramref name ="selector"/> function returned values.
        /// </summary>
        /// <typeparam name="TSource">Type the items.</typeparam>
        /// <typeparam name="TKey">Type of the returned value of <paramref name ="selector"/> function.</typeparam>
        /// <param name="source">Sequence to which the method applies.</param>
        /// <param name="selector">Mapping function from <c>TSource</c> to <c>TKey</c>.</param>
        /// <returns>The greatest item in the sequence.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="selector"/> is null.</exception>
        public static TSource MaxBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Gets the greatest item of the sequence using <paramref name ="comparer"/> with the <paramref name ="selector"/> function returned values.
        /// </summary>
        /// <typeparam name="TSource">Type the items.</typeparam>
        /// <typeparam name="TKey">Type of the returned value of <paramref name ="selector"/> function.</typeparam>
        /// <param name="source">Sequence to which the method applies.</param>
        /// <param name="selector">Mapping function from <c>TSource</c> to <c>TKey</c>.</param>
        /// <param name="comparer">Comparer used fot the <c>TKey</c> type.</param>
        /// <returns>The greatest item in the sequence.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="selector"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="comparer"/> is null.</exception>
        public static TSource MaxBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selector,
            IComparer<TKey> comparer)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfArgumentNull(selector, nameof(selector));
            Throwable.ThrowIfArgumentNull(comparer, nameof(comparer));
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    throw new InvalidOperationException("Empty sequence");

                var max = iterator.Current;
                var maxKey = selector(max);
                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    var currentKey = selector(current);
                    if (comparer.Compare(currentKey, maxKey) > 0)
                    {
                        max = current;
                        maxKey = currentKey;
                    }
                }

                return max;
            }
        }

        /// <summary>
        /// Gets the smallest item of the sequence using the default comparer with the <paramref name ="selector"/> function returned values.
        /// </summary>
        /// <typeparam name="TSource">Type the items.</typeparam>
        /// <typeparam name="TKey">Type of the returned value of <paramref name ="selector"/> function.</typeparam>
        /// <param name="source">Sequence to which the method applies.</param>
        /// <param name="selector">Mapping function from <c>TSource</c> to <c>TKey</c>.</param>
        /// <returns>The smallest item in the sequence.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="selector"/> is null.</exception>
        public static TSource MinBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Gets the smallest item of the sequence using the <paramref name ="comparer"/> with the <paramref name ="selector"/> function returned values.
        /// </summary>
        /// <typeparam name="TSource">Type the items.</typeparam>
        /// <typeparam name="TKey">Type of the returned value of <paramref name ="selector"/> function.</typeparam>
        /// <param name="source">Sequence to which the method applies.</param>
        /// <param name="selector">Mapping function from <c>TSource</c> to <c>TKey</c>.</param>
        /// <param name="comparer">A comparer for <c>TKey</c>.</param>
        /// <returns>The smallest item in the sequence.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="selector"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="selector"/> is null.</exception>
        public static TSource MinBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selector,
            IComparer<TKey> comparer)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfArgumentNull(selector, nameof(selector));
            Throwable.ThrowIfArgumentNull(comparer, nameof(comparer));
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    throw new InvalidOperationException("Empty sequence");

                var min = iterator.Current;
                var minKey = selector(min);
                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    var currentKey = selector(current);
                    if (comparer.Compare(currentKey, minKey) < 0)
                    {
                        min = current;
                        minKey = currentKey;
                    }
                }

                return min;
            }
        }
    }
}
