using System;
using System.Runtime.InteropServices;

namespace Coffer.Services.Helpers
{
    public class OSHelper
    {
        [DllImport("libc", EntryPoint = "stat")] // Using stat() in libc (Linux C library)
        private static extern int stat(string path, out StatBuffer statBuffer); // An internal function for filling drive data in (by the linux kernel) and returning 0 if successful and -1 if faliure.

        [StructLayout(LayoutKind.Sequential)] // Lays the structs fields in the same order they're declared in memory, to prevent reordering and different padding.
        private struct StatBuffer // Creating a structure for data about a drive (for detecting what drive is it)
        {
            public long st_dev; // Device ID
            public long st_ino; // Inode number (unique ID of the file on the drive)
            public long st_nlink; // number of hard links to the file
            public uint st_mode; // file type and permissions
            public uint st_uid; // User ID of the owner
            public uint st_gid; // Group ID of the owner
            public uint __pad0; // Padding, keeps memory alignment correct
            public long st_rdev; // Device ID for if it's a special file
            public long st_size; // file size in bytes
            public long st_blksize; // preferred block size for filesystem I/O
            public long st_blocks; // number of 512B block allocated
            public long st_atime; // Last access time
            public long st_atime_nsec;
            public long st_mtime; // Last modification time
            public long st_mtime_nsec;
            public long st_ctime; // Last status change time
            public long st_ctime_nsec;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public long[] __unused; // reserved space
        }

        public static long GetDeviceID(string path)
        {
            if (stat(path, out StatBuffer buf) != 0) // If stat returns a number that is not the number for success (0) it will return an error.
            {
                throw new IOException($"Could not stat path: {path}");
            }
            return buf.st_dev; // Returns device ID.
        }
    }
}
