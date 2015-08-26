using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static PSShellProvider.ShellNativeFunctions;

namespace PSShellProvider
{
    public class ShellFolder : ShellItem
    {
        private static Lazy<ShellFolder> _desktopFolder = new Lazy<ShellFolder>(() =>
        {
            return new ShellFolder(null, SHGetDesktopFolder(), new ShellItemKnownInfo());
        });

        internal IShellFolder NativeFolder { get; private set; }

        internal ShellFolder(IShellItem shellItem, IShellFolder shellFolder, ShellItemKnownInfo knownInfo)
            : base(GetShellItemFromFolder(shellItem, shellFolder), knownInfo)
        {
            if (shellFolder == null)
            {
                shellFolder = (IShellFolder)shellItem.BindToHandler(null, ShellConsts.BHID_SFObject, typeof(IShellFolder).GUID);
            }
            NativeFolder = shellFolder;
        }

        private static IShellItem GetShellItemFromFolder(IShellItem shellItem, IShellFolder shellFolder)
        {
            if (shellItem == null && shellFolder == null)
                throw new ArgumentNullException("shellItem, shellFolder", "shellItem and shellFolder cannot both be null");
            if (shellItem != null)
                return shellItem;
            IdList pidl = SHGetIDListFromObject(shellFolder);
            return SHCreateItemFromIDList<IShellItem>(pidl);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (NativeFolder != null)
                {
                    Marshal.ReleaseComObject(NativeFolder);
                    NativeFolder = null;
                }
            }
            base.Dispose(disposing);
        }

        public IEnumerable<ShellItem> GetChildItems(int bulkRequestCount = 10)
        {
            IEnumIDList enumIdList;
            int hr = NativeFolder.EnumObjects(IntPtr.Zero, SHCONTF.Folder | SHCONTF.NonFolders | SHCONTF.IncludeHidden | SHCONTF.IncludeSuperHidden,
                out enumIdList);
            if (hr == ShellConsts.S_FALSE)
                yield break;
            else if (hr != ShellConsts.S_OK)
                Marshal.ThrowExceptionForHR(hr);
            try
            {
                IntPtr[] idListOut = new IntPtr[bulkRequestCount];
                uint fetched;
                do
                {
                    hr = enumIdList.Next((uint)bulkRequestCount, idListOut, out fetched);
                    if (hr != ShellConsts.S_OK && hr != ShellConsts.S_FALSE)
                        Marshal.ThrowExceptionForHR(hr);
                    IdList[] list = new IdList[(int)fetched];
                    for (int i = 0; i < fetched; i++)
                    {
                        list[i] = new IdList(idListOut[i]);
                        Marshal.FreeCoTaskMem(idListOut[i]);
                    }
                    foreach (var idList in list)
                    {
                        IdList absId = new IdList(Pidl.Parts.Concat(idList.Parts));
                        yield return ShellItem.GetShellItem(absId, new ShellItemKnownInfo()
                        {
                            ParentParsePath = ParsePath
                        });
                    }
                } while (hr == ShellConsts.S_OK);
            }
            finally
            {
                if (enumIdList != null)
                    Marshal.ReleaseComObject(enumIdList);
            }
        }

        public static ShellFolder DesktopFolder => _desktopFolder.Value;
    }
}