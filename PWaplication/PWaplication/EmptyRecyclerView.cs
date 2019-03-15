using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace PWaplication
{
    public class EmptyRecyclerView : RecyclerView
    {
        private EmptyView emptyView;
        private EmtyDataObserver observer;

        public EmptyRecyclerView(Context context) : base(context)
        {
            init();
        }

        public EmptyRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            init();
        }

        public EmptyRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            init();
        }

        protected EmptyRecyclerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            init();
        }

        public override void SetAdapter(Adapter adapter)
        {
            Adapter oldAdapter = GetAdapter();
            base.SetAdapter(adapter);
            oldAdapter?.UnregisterAdapterDataObserver(observer);
            adapter?.RegisterAdapterDataObserver(observer);
        }

        private void init()
        {
            observer = new EmtyDataObserver(this);
        }

        protected void initEmptyView()
        {
            if (emptyView != null)
            {
                var isEmpty = GetAdapter() == null || GetAdapter().ItemCount == 0;
                emptyView.Visibility = isEmpty ?
                    ViewStates.Visible : ViewStates.Gone;
                if (isEmpty)
                {
                    emptyView.SetTextProgress(EmptyView.StyleEmptyView.ONLY_TEXT,
                        Context.GetString(Resource.String.str_not_found));
                }
                Visibility = isEmpty ?
                    ViewStates.Gone : ViewStates.Visible;
            }
        }

        public void SetEmptView(EmptyView view)
        {
            emptyView = view;
        }

        public void ShowLoading(EmptyView.StyleEmptyView style, int res)
        {
            emptyView?.SetTextProgress(style,
                        Context.GetString(res));
            Visibility = ViewStates.Gone;
        }

        private class EmtyDataObserver : AdapterDataObserver
        {
            private EmptyRecyclerView recycler;

            public EmtyDataObserver(EmptyRecyclerView recyclerView)
            {
                recycler = recyclerView;
            }
            public override void OnChanged()
            {
                base.OnChanged();
                recycler.initEmptyView();
            }

            public override void OnItemRangeInserted(int positionStart, int itemCount)
            {
                base.OnItemRangeInserted(positionStart, itemCount);
                recycler.initEmptyView();
            }

            public override void OnItemRangeRemoved(int positionStart, int itemCount)
            {
                base.OnItemRangeRemoved(positionStart, itemCount);
                recycler.initEmptyView();
            }
        }
    }
}