using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace MediaScoutGUI
{
	public class DispatchingCollection<TCollection, TItem> : ICollection<TItem>, IEnumerable<TItem>, IEnumerable, INotifyCollectionChanged where TCollection : ICollection<TItem>, INotifyCollectionChanged
	{
		private delegate void AddHandler(TItem item);

		private delegate void ClearHandler();

		private delegate bool RemoveHandler(TItem item);

		private delegate void OnCollectionChangedHandler(NotifyCollectionChangedEventArgs e);

		private readonly Dispatcher _dispatcher;

		private readonly TCollection _underlyingCollection;

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		protected TCollection UnderlyingCollection
		{
			get
			{
				return this._underlyingCollection;
			}
		}

		public Dispatcher Dispatcher
		{
			get
			{
				return this._dispatcher;
			}
		}

		public int Count
		{
			get
			{
				TCollection underlyingCollection = this._underlyingCollection;
				return underlyingCollection.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				TCollection underlyingCollection = this._underlyingCollection;
				return underlyingCollection.IsReadOnly;
			}
		}

		public DispatchingCollection(TCollection underlyingCollection, Dispatcher dispatcher)
		{
			if (underlyingCollection == null)
			{
				throw new ArgumentNullException("underlyingCollection");
			}
			if (dispatcher == null)
			{
				throw new ArgumentNullException("dispatcher");
			}
			this._underlyingCollection = underlyingCollection;
			this._dispatcher = dispatcher;
			this._underlyingCollection.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
			{
				this.OnCollectionChanged(e);
			};
		}

		public void Add(TItem item)
		{
			if (!this.Dispatcher.CheckAccess())
			{
				this.Dispatcher.Invoke(DispatcherPriority.Send, new DispatchingCollection<TCollection, TItem>.AddHandler(this.Add), item);
				return;
			}
			TCollection underlyingCollection = this._underlyingCollection;
			underlyingCollection.Add(item);
		}

		public void Clear()
		{
			if (!this.Dispatcher.CheckAccess())
			{
				this.Dispatcher.Invoke(DispatcherPriority.Send, new DispatchingCollection<TCollection, TItem>.ClearHandler(this.Clear));
				return;
			}
			TCollection underlyingCollection = this._underlyingCollection;
			underlyingCollection.Clear();
		}

		public bool Contains(TItem item)
		{
			TCollection underlyingCollection = this._underlyingCollection;
			return underlyingCollection.Contains(item);
		}

		public void CopyTo(TItem[] array, int arrayIndex)
		{
			TCollection underlyingCollection = this._underlyingCollection;
			underlyingCollection.CopyTo(array, arrayIndex);
		}

		public bool Remove(TItem item)
		{
			if (!this.Dispatcher.CheckAccess())
			{
				return (bool)this.Dispatcher.Invoke(DispatcherPriority.Send, new DispatchingCollection<TCollection, TItem>.RemoveHandler(this.Remove), item);
			}
			TCollection underlyingCollection = this._underlyingCollection;
			return underlyingCollection.Remove(item);
		}

		public IEnumerator<TItem> GetEnumerator()
		{
			TCollection underlyingCollection = this._underlyingCollection;
			return underlyingCollection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (this._underlyingCollection as ICollection).GetEnumerator();
		}

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (this._dispatcher.CheckAccess())
			{
				if (this.CollectionChanged != null)
				{
					this.CollectionChanged.DynamicInvoke(new object[]
					{
						this,
						e
					});
					return;
				}
			}
			else
			{
				this._dispatcher.Invoke(DispatcherPriority.Send, new DispatchingCollection<TCollection, TItem>.OnCollectionChangedHandler(this.OnCollectionChanged), e);
			}
		}
	}
}
