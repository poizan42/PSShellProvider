using System;
using System.Runtime.InteropServices;

namespace PSShellProvider
{
    public class ShellFolder : ShellItem
    {
        private IShellFolder shellFolder;

        internal ShellFolder(IShellItem shellItem, IShellFolder shellFolder, SFGAO loadedAttrs, SFGAO attrs)
            : base(shellItem, loadedAttrs, attrs)
        {
            if (shellFolder == null)
            {
                object objShellFolder;
                shellItem.BindToHandler(null, ShellConsts.BHID_SFObject, typeof(IShellFolder).GUID, out objShellFolder);
                shellFolder = (IShellFolder)objShellFolder;
            }
            this.shellFolder = shellFolder;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (shellFolder != null)
                {
                    Marshal.ReleaseComObject(shellFolder);
                    shellFolder = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}