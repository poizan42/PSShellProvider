using System;
using System.Runtime.InteropServices;
using static PSShellProvider.ShellNativeFunctions;

namespace PSShellProvider
{
    public class ShellItem : IDisposable
    {
        internal class ShellItemKnownInfo
        {
            public SFGAO LoadedAttributes { get; set; }
            public SFGAO Attributes { get; set; }
            public string ParentParsePath { get; set; }
            public ShellFolder ParentFolder { get; set; }
        }

        private IShellItem nativeItem;
        private Lazy<IdList> _pidl;
        private Lazy<string> _parseName, _parsePath, _desktopAbsoluteParsePath, _fullParsePath, _displayName;
        private Lazy<ShellFolder> _parentFolder;
        private string _parentParsePath;
        private SFGAO _loadedAttrs;
        private SFGAO _attrs;

        private void Init(ShellItemKnownInfo knownInfo)
        {
            _loadedAttrs = knownInfo.LoadedAttributes;
            _attrs = knownInfo.Attributes;
            _parentParsePath = knownInfo.ParentParsePath;

            _pidl = new Lazy<IdList>(() => SHGetIDListFromObject(nativeItem));
            _parentFolder = new Lazy<ShellFolder>(() =>
            {
                var nativeParent = nativeItem.GetParent();
                if (nativeParent == null)
                    return null;
                return new ShellFolder(nativeParent, null, new ShellItemKnownInfo());
            });
            _desktopAbsoluteParsePath = new Lazy<string>(() => nativeItem.GetDisplayName(SIGDN.DesktopAbsoluteParsing));
            _parseName = new Lazy<string>(() => nativeItem.GetDisplayName(SIGDN.ParentRelativeParsing));
            _parsePath = new Lazy<string>(() =>
            {
                if (Pidl.Parts.Count == 0) // The actual desktop
                    return "";
                try
                {
                    if (_parentParsePath != null)
                    {
                        string normParentPath = _parentParsePath.Trim('\\');
                        if (normParentPath != "")
                            normParentPath += "\\";
                        return normParentPath + ParseName;
                    }
                    else
                        return DesktopAbsoluteParsePath;
                }
                catch (ArgumentException)
                {
                    IdList childPidl = new IdList(Pidl.Parts[Pidl.Parts.Count - 1]);
                    return GetParentFolder().NativeFolder.GetDisplayNameOf(childPidl, SHGDNF.ForParsing);
                }
            });
            _fullParsePath = new Lazy<string>(() =>
            {
                if (GetParentFolder() == null)
                    return "";
                string parentParsePath = GetParentFolder().FullParsePath.TrimEnd('\\');
                if (parentParsePath != "")
                    parentParsePath += "\\";
                return parentParsePath + nativeItem.GetDisplayName(SIGDN.ParentRelativeParsing);
            });
            /*_displayName = new Lazy<string>(() => GetParentFolder().NativeFolder.GetDisplayNameOf(
                new IdList(Pidl.Parts[Pidl.Parts.Count - 1]),
                SHGDNF.InFolder | SHGDNF.Normal));*/
            _displayName = new Lazy<string>(() => nativeItem.GetDisplayName(SIGDN.NormalDisplay));
        }

        public IdList Pidl { get { return _pidl.Value; } }
        /// <summary>
        /// The display name for parsing for this entry.
        /// </summary>
        public string ParseName { get { return _parseName.Value; } }
        /// <summary>
        /// This is *a* parse path, it may not be "canonical" in one sense or another
        /// </summary>
        public string ParsePath { get { return _parsePath.Value; } }
        /// <summary>
        /// This is specifically the desktop absolute parse path
        /// </summary>
        public string DesktopAbsoluteParsePath { get { return _desktopAbsoluteParsePath.Value; } }
        /// <summary>
        /// This is the parse path we get by explicitly querying every folder up to the root and concatenating their paths.
        /// </summary>
        public string FullParsePath { get { return _fullParsePath.Value; } }
        public string DisplayName { get { return _displayName.Value; } }

        public ShellFolder GetParentFolder()
        {
            return _parentFolder.Value;
        }

        public SFGAO GetAttributes(SFGAO attributes)
        {
            _attrs = LoadMissingAttributes(nativeItem, _loadedAttrs, _attrs, attributes);
            _loadedAttrs = _loadedAttrs | attributes;
            return _attrs;
        }

        internal ShellItem(IShellItem nativeItem, ShellItemKnownInfo knownInfo)
        {
            this.nativeItem = nativeItem;
            Init(knownInfo);
        }
        
        internal ShellItem(IdList pidl, ShellItemKnownInfo knownInfo)
        {
            nativeItem = SHCreateItemFromIDList<IShellItem>(pidl);
            Init(knownInfo);
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

        public static ShellItem GetShellItem(IdList pidl)
        {
            if (pidl.Parts.Count == 0)
                return ShellFolder.DesktopFolder;
            return GetShellItem(pidl, new ShellItemKnownInfo());
        }
        internal static ShellItem GetShellItem(IdList pidl, ShellItemKnownInfo knownInfo)
        {
            IShellItem nativeItem = SHCreateItemFromIDList<IShellItem>(pidl);
            knownInfo.Attributes = LoadMissingAttributes(nativeItem, knownInfo.LoadedAttributes, knownInfo.Attributes, SFGAO.Browsable | SFGAO.Folder);
            knownInfo.LoadedAttributes |= SFGAO.Browsable | SFGAO.Folder;
            if ((knownInfo.Attributes & (SFGAO.Browsable | SFGAO.Folder)) != SFGAO.None)
                return new ShellFolder(nativeItem, null, knownInfo);
            else
                return new ShellItem(nativeItem, knownInfo);
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
