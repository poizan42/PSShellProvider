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

            return new ShellPSDriveInfo((ShellFolder)ShellItem.GetShellItem(pidl, isContainerQuery, attributes), drive);
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

        protected override void GetItem(string path)
        {
            IdList pidl = GetPidlFromPath(path);
            ShellItem obj = ShellItem.GetShellItem(pidl, SFGAO.None, SFGAO.None);
            WriteItemObject(obj, path == "" ? "\\" : path, false);
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

        private IdList GetPidlFromPath(string path)
        {
            SFGAO dummy;
            return GetPidlFromPath(path, SFGAO.None, out dummy);
        }

        private IdList GetPidlFromPath(string path, SFGAO queryForAttributes, out SFGAO attributes)
        {
            return IdList.Parse(path.TrimEnd('\\'), queryForAttributes, out attributes);
        }

    }
}
