using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Threading;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Presentation
{
    public class ObservableCollectionDispatched<T> : ICollection<T>, INotifyCollectionChanged
    {
        private readonly Semaphore _sync = new Semaphore(1, 1);
#pragma warning disable 414
        private bool _locked;
#pragma warning restore 414

        private readonly Collection<T> _collection = new Collection<T>();
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        internal class DisposableSync : IDisposable
        {
            private readonly ObservableCollectionDispatched<T> _parent;
            private readonly bool _lockAquired;

            internal DisposableSync(ObservableCollectionDispatched<T> parent)
            {
                _parent = parent;

                while (_parent._locked)
                {
                    Thread.Sleep(1);
                }
                _lockAquired = _parent._sync.WaitOne();
                if (_lockAquired)
                {
                    _parent._locked = true;
                }
            }

            public void Dispose()
            {
                if (!_lockAquired) return;

                _parent._sync.Release(1);
                _parent._locked = false;
            }
        }



        // constructors
        public ObservableCollectionDispatched()
        {
        }

        public ObservableCollectionDispatched(IEnumerable<T> items)
        {
            AddRange(items);
        }

        // properties
        public int Count => _collection.Count;
        public bool IsReadOnly => false;
        public object SyncRoot => _collection;
        public bool IsSynchronized => true;

        public T this[int index]
        {
            get => _collection[index];
            set => _collection[index] = value;
        }

        public IList<T> Items => _collection;

        //methods
        public void Add(T item)
        {
            using(new DisposableSync(this))
            {
                _collection.Add(item);
                OnCollectionCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
        }

        public void Clear()
        {
            ClearItems();
        }
        public void ClearItems()
        {
            using (new DisposableSync(this))
            {
                _collection.Clear();
                OnCollectionCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(Array array, int index)
        {
            _collection.CopyTo((T[])array, index);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _collection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            InsertItem(index, item);
        }
        public void InsertItem(int index, T item)
        {
            using (new DisposableSync(this))
            {
                _collection.Insert(index, item);
                OnCollectionCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }
        public void MoveItem(int oldIndex, int newIndex)
        {
            var item = _collection[oldIndex];
            _collection.RemoveAt(oldIndex);
            _collection.Insert(newIndex, item);
        }


        private void OnCollectionCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var eventHandler = CollectionChanged;
            if (eventHandler == null)
                return;

            var delegates = eventHandler.GetInvocationList();
            // Walk thru invocation list
            foreach (var del in delegates)
            {
                var handler = (NotifyCollectionChangedEventHandler)del;
                var dispatcherObject = handler.Target as DispatcherObject;

                try
                {
                    // If the subscriber is a DispatcherObject and different thread
                    if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
                    {
                        // Invoke handler in the target dispatcher's thread
                        //dispatcherObject.Dispatcher.BeginInvoke(handler, DispatcherPriority.DataBind, new object[] { this, e });
                        dispatcherObject.Dispatcher.Invoke(handler, this, e);
                    }
                    else // Execute handler as is
                    {
                        handler(this, e);
                    }
                }
                catch (Exception)
                {
                    //TODO: Why!
                    //Debug.WriteLine("ObservableCollectionDispatched" + ex.Message);
                }
            }
        }


        public bool Remove(T item)
        {
            var index = _collection.IndexOf(item);
            if (index == -1) return false;

            RemoveAt(index);
            return true;
        }


        public void RemoveAt(int index)
        {
            RemoveItem(index);
        }
        public void RemoveItem(int index)
        {
            using (new DisposableSync(this))
            {
                var item = _collection[index];
                OnCollectionCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<T> { item }, index));
                _collection.Remove(item);
            }
        }

        public void SetItem(int index, T item)
        {
            var oldItem = _collection[index];
            _collection[index] = item;
            OnCollectionCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }

        // additional methods
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _collection.Add(item);
            }
        }

    }

}
