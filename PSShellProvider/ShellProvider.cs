using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    [CmdletProvider("Shell", ProviderCapabilities.None)]
    public class ShellProvider : NavigationCmdletProvider
    {
        protected override bool IsValidPath(string path)
        {
            return true;
        }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            if (drive == null)
            {
                WriteError(new ErrorRecord(
                           new ArgumentNullException("drive"),
                           "NullDrive",
                           ErrorCategory.InvalidArgument,
                           null));

                return null;
            }
            SFGAO attributes;
            SFGAO isContainerQuery = SFGAO.Browsable | SFGAO.Folder;
            IdList pidl = GetPidlFromPath(drive.Root, isContainerQuery, out attributes);
            if ((attributes & isContainerQuery) == SFGAO.None)
            {
                WriteError(new ErrorRecord(
                           new ArgumentException("drive.Root"),
                           "NotAContainer",
                           ErrorCategory.InvalidArgument,
                           null));
                return null;
            }

            return new ShellPSDriveInfo((ShellFolder)ShellItem.GetShellItem(pidl, new ShellItem.ShellItemKnownInfo()
            {
                LoadedAttributes = isContainerQuery,
                Attributes = attributes
            }), drive);
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            if (drive == null)
            {
                WriteError(new ErrorRecord(
                           new ArgumentNullException("drive"),
                           "NullDrive",
                           ErrorCategory.InvalidArgument,
                           drive));

                return null;
            }

            if (!(drive is ShellPSDriveInfo))
            {
                return null;
            }

            return drive;
        }

       private ShellItem GetItemCore(string path)
        {
            IdList pidl = GetPidlFromPath(path);
            return ShellItem.GetShellItem(pidl, new ShellItem.ShellItemKnownInfo());
        }

        protected override void GetItem(string path)
        {
            WriteItemObject(GetItemCore(path), path == "" ? "\\" : path, false);
        }

        protected override bool ItemExists(string path)
        {
            try
            {
                GetPidlFromPath(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        protected override bool IsItemContainer(string path)
        {
            SFGAO attributes;
            SFGAO isContainerQuery = SFGAO.Browsable | SFGAO.Folder;
            IdList pidl = GetPidlFromPath(path, isContainerQuery, out attributes);
            return (attributes & isContainerQuery) != SFGAO.None;
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            ShellItem item = GetItemCore(path);
            ShellFolder folder = item as ShellFolder;
            if (folder == null)
            {
                WriteItemObject(item, path, false);
                return;
            }
            foreach (var child in folder.GetChildItems())
            {
                WriteItemObject(child, child.ParsePath, child is ShellFolder);
            }
        }

        protected override string GetParentPath(string path, string root)
        {
            return base.GetParentPath(path, root);
        }

        private IdList GetPidlFromPath(string path)
        {
            SFGAO dummy;
            return GetPidlFromPath(path, SFGAO.None, out dummy);
        }

        private IdList GetPidlFromPath(string path, SFGAO queryForAttributes, out SFGAO attributes)
        {
            path = path.TrimEnd('\\');
            if (path == "")
            {
                ShellFolder desktopFolder = ShellFolder.DesktopFolder;
                attributes = desktopFolder.GetAttributes(queryForAttributes);
                return desktopFolder.Pidl;
            }
            return IdList.Parse(path, queryForAttributes, out attributes);
        }

    }
}
