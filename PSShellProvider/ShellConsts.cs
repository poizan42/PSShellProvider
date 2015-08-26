using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    internal static class ShellConsts
    {
        public const int S_OK = 0;
        public const int S_FALSE = 1;

        public const int E_NOINTERFACE = unchecked((int)0x80004002);
        public const int MK_E_NOOBJECT = unchecked((int)0x800401E5);

        public static readonly Guid BHID_SFObject = new Guid("3981e224-f559-11d3-8e3a-00c04f6837d5");
    }

    public enum SIGDN : uint
    {
        NormalDisplay                = 0x00000000,
        ParentRelativeParsing        = 0x80018001,
        DesktopAbsoluteParsing       = 0x80028000,
        ParentRelativeEditing        = 0x80031001,
        DesktopAbsoluteEditing       = 0x8004c000,
        FileSysPath                  = 0x80058000,
        Url                          = 0x80068000,
        ParentRelativeForAddressBar  = 0x8007c001,
        ParentRelative               = 0x80080001,
        ParentRelativeForUi          = 0x80094001
    }

    [Flags]
    public enum SFGAO : uint
    {
        None = 0,
        CanCopy       = 0x1,                // Objects can be copied    (DROPEFFECT_COPY)
        CanMove       = 0x2,                // Objects can be moved     (DROPEFFECT_MOVE)
        CanLink       = 0x4,                // Objects can be linked    (DROPEFFECT_LINK)
        Storage       = 0x00000008,         // supports BindToObject(IID_IStorage)
        CanRename     = 0x00000010,         // Objects can be renamed
        CanDelete     = 0x00000020,         // Objects can be deleted
        HasPropSheet      = 0x00000040,         // Objects have property sheets
        DropTarget    = 0x00000100,         // Objects are drop target
        CapabilityMask    = 0x00000177,
        Encrypted     = 0x00002000,         // object is encrypted (use alt color)
        IsSlow        = 0x00004000,         // 'slow' object
        Ghosted       = 0x00008000,         // ghosted icon
        Link          = 0x00010000,         // Shortcut (link)
        Share         = 0x00020000,         // shared
        ReadOnly      = 0x00040000,         // read-only
        Hidden        = 0x00080000,         // hidden object
        DisplayAttrMask   = 0x000FC000,
        FileSysAncestor   = 0x10000000,         // may contain children with SFGAO_FILESYSTEM
        Folder        = 0x20000000,         // support BindToObject(IID_IShellFolder)
        FileSystem    = 0x40000000,         // is a win32 file system object (file/folder/root)
        HasSubfolder      = 0x80000000,         // may contain children with SFGAO_FOLDER
        ContentMask      = 0x80000000,
        Validate      = 0x01000000,         // invalidate cached information
        Removable     = 0x02000000,         // is this removeable media?
        Compressed    = 0x04000000,         // Object is compressed (use alt color)
        Browsable     = 0x08000000,         // supports IShellFolder, but only implements CreateViewObject() (non-folder view)
        NonEnumerated     = 0x00100000,         // is a non-enumerated object
        NewContent    = 0x00200000,         // should show bold in explorer tree
        CanMoniker    = 0x00400000,         // defunct
        HasStorage    = 0x00400000,         // defunct
        Stream        = 0x00400000,         // supports BindToObject(IID_IStream)
        StorageAncestor   = 0x00800000,         // may contain children with SFGAO_STORAGE or SFGAO_STREAM
        StorageCapMask    = 0x70C50008,         // for determining storage capabilities, ie for open/save semantics
    }

    [Flags]
    public enum SHCONTF { 
      CheckingForChildren    = 0x00010,
      Folder                 = 0x00020,
      NonFolders             = 0x00040,
      IncludeHidden          = 0x00080,
      InitOnFirstNext        = 0x00100,
      NetPrinterSrch         = 0x00200,
      Shareable              = 0x00400,
      Storage                = 0x00800,
      NavigationEnum         = 0x01000,
      FastItems              = 0x02000,
      FlatList               = 0x04000,
      EnableAsync            = 0x08000,
      IncludeSuperHidden     = 0x10000
    };
    public enum SHGDNF { 
      Normal         = 0,
      InFolder       = 0x1,
      ForEditing     = 0x1000,
      ForAddressBar  = 0x4000,
      ForParsing     = 0x8000
    };
}
