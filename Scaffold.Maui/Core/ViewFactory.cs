﻿using Microsoft.Maui.Controls;
using ScaffoldLib.Maui.Containers;
using ScaffoldLib.Maui.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaffoldLib.Maui.Core
{
    public class ViewFactory
    {
        public virtual IFrame CreateFrame(View view)
        {
            return new Internal.Frame(view, this);
        }

        public virtual INavigationBar? CreateNavigationBar(View view)
        {
#if ANDROID
            return new Platforms.Android.NavigationBar(view);
#else
            return null;
#endif
        }

        public virtual IViewWrapper CreateViewWrapper(View view)
        {
            return new ViewWrapper(view);
        }

        public virtual IDisplayAlert CreateDisplayAlert(string title, string message, string ok)
        {
#if ANDROID
            return new global::ScaffoldLib.Maui.Platforms.Android.DisplayAlertLayer(title, message, ok);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual IDisplayAlert CreateDisplayAlert(string title, string message, string ok, string cancel)
        {
#if ANDROID
            return new global::ScaffoldLib.Maui.Platforms.Android.DisplayAlertLayer(title, message, ok, cancel);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual IZBufferLayout CreateDisplayMenuItemslayer(View view)
        {
#if ANDROID
            return new global::ScaffoldLib.Maui.Platforms.Android.DisplayMenuItemslayer(view);
#else
            throw new NotImplementedException();
#endif
        }
    }
}
