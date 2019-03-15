using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace PWaplication
{
    public class EmptyView : RelativeLayout
    {
        public enum StyleEmptyView {
            ONLY_TEXT,
            TEXT_AND_PROGRESS,
            ONLY_PROGRESS
        }

        private ContentLoadingProgressBar progressBar;
        private TextView text;

        public EmptyView(Context context) : base(context)
        {
            init();
        }

        public EmptyView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            init();
        }

        public EmptyView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            init();
        }

        public EmptyView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            init();
        }

        protected EmptyView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            init();
        }

        private void init()
        {
            Inflate(Context, Resource.Layout.empty_view, this);
            progressBar = FindViewById<ContentLoadingProgressBar>(Resource.Id.progress_bar);
            text = FindViewById<TextView>(Resource.Id.progress_text);
        }

        public void SetTextProgress(StyleEmptyView style, string str)
        {
            switch (style) {
                case StyleEmptyView.ONLY_PROGRESS:
                    progressBar.Visibility = ViewStates.Visible;
                    text.Visibility = ViewStates.Gone;
                    break;
                case StyleEmptyView.ONLY_TEXT:
                    progressBar.Visibility = ViewStates.Invisible;
                    text.Visibility = ViewStates.Visible;
                    break;
                case StyleEmptyView.TEXT_AND_PROGRESS:
                    progressBar.Visibility = ViewStates.Visible;
                    text.Visibility = ViewStates.Visible;
                    break;
                default:
                    break;
            }
            Visibility = ViewStates.Visible;
            text.Text = str;
        }
    }
}