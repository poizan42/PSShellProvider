using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    public class ShellItem : IDisposable
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHCreateItemFromIDList(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            IdList pidl,
            [MarshalAs(UnmanagedType.LPStruct)]
            Guid riid,
            [MarshalAs(UnmanagedType.Interface)]
            out IShellItem v);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetIDListFromObject(
            [MarshalAs(UnmanagedType.IUnknown)]
            object obj,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IdListMarshaler))]
            out IdList pidl);

        private IShellItem nativeItem;
        private Lazy<IdList> _pidl;
        private SFGAO _loadedAttrs;
        private SFGAO _attrs;

        private void Init()
        {
            _pidl = new Lazy<IdList>(() =>
            {
                IdList pidl;
                int hr = SHGetIDListFromObject(nativeItem, out pidl);
                if (hr == ShellConsts.E_NOINTERFACE)
                    return null;
                if (hr != 0)
                    Marshal.ThrowExceptionForHR(hr);
                return pidl;
            });
        }

        public IdList Pidl { get { return _pidl.Value; } }

        public SFGAO GetAttributes(SFGAO attributes)
        {
            _attrs = LoadMissingAttributes(nativeItem, _loadedAttrs, _attrs, attributes);
            _loadedAttrs = _loadedAttrs | attributes;
            return _attrs;
        }

        internal ShellItem(IShellItem nativeItem, SFGAO loadedAttrs, SFGAO attrs)
        {
            this.nativeItem = nativeItem;
            _loadedAttrs = loadedAttrs;
            _attrs = attrs;
            Init();
        }
        
        internal ShellItem(IdList pidl, SFGAO loadedAttrs, SFGAO attrs)
        {
            int hr = SHCreateItemFromIDList(pidl, typeof(IShellItem).GUID, out nativeItem);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
            Init();
        }

        private static SFGAO LoadMissingAttributes(IShellItem nativeItem, SFGAO loadedAttrs, SFGAO curAttrs, SFGAO requestedAttrs)
        {
            SFGAO unloadedAttributes = requestedAttrs & (~loadedAttrs);
            if (unloadedAttributes != SFGAO.None)
            {
                SFGAO newAttributes;
                nativeItem.GetAttributes(unloadedAttributes, out newAttributes);
                curAttrs |= newAttributes;
            }
            return curAttrs;
        }

        internal static ShellItem GetShellItem(IdList pidl, SFGAO loadedAttrs, SFGAO attrs)
        {
            IShellItem nativeItem;
            int hr = SHCreateItemFromIDList(pidl, typeof(IShellItem).GUID, out nativeItem);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
            attrs = LoadMissingAttributes(nativeItem, loadedAttrs, attrs, SFGAO.Browsable | SFGAO.Folder);
            loadedAttrs |= SFGAO.Browsable | SFGAO.Folder;
            if ((attrs & (SFGAO.Browsable | SFGAO.Folder)) != SFGAO.None)
                return new ShellFolder(nativeItem, null, loadedAttrs, attrs);
            else
                return new ShellItem(nativeItem, loadedAttrs, attrs);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (nativeItem != null)
                {
                    Marshal.ReleaseComObject(nativeItem);
                    nativeItem = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ShellItem()
        {
            Dispose(false);
        }
    }
}
