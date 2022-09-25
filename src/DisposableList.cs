using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Describes a list of disposable values.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public class DisposableList<T> : List<T>, IDisposableCollection<T>
        where T : IDisposable
    {
        /// <summary>
        /// Creates a new empty instance.
        /// </summary>
        public DisposableList() { }

        /// <summary>
        /// Creates a new instance by copying the sequence items.
        /// </summary>
        /// <param name="collection">Sequence whose elements are copied into the new set.</param>
        public DisposableList(IEnumerable<T> collection)
            : base(collection) { }

        /// <summary>
        /// Creates a new empty instance.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public DisposableList(int capacity)
            : base(capacity) { }

        /// <summary>
        /// Disposes of all items.
        /// </summary>
        public void Dispose()
        {
            if (0 >= Count)
            {
                return;
            }

            Exception last = null;
            var list = this.ToList();
            Clear();
            foreach (T item in list)
            {
                try
                {
                    item?.Dispose();
                }
                catch (Exception ex)
                {
                    last = last ?? ex;
                }
            }
            if (last != null)
                throw last;
        }

        /// <summary>
        /// Removes items from the active instance.
        /// </summary>
        /// <param name="items">Items to remove.</param>
        /// <returns>The sequence of effectively removed items.</returns>
        public IEnumerable<T> RemoveRange(IEnumerable<T> items)
        {
            Throwable.ThrowIfArgumentNull(items, nameof(items));
            foreach (T item in items)
            {
                if (Remove(item))
                    yield return item;
            }
        }
    }
}
