// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.IO.IsolatedStorage
{
    public class RemoveTests : IsoStorageTest
    {
        [Fact]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/114403", typeof(PlatformDetection), nameof(PlatformDetection.IsMacCatalyst))]
        public void RemoveUserStoreForApplication()
        {
            TestHelper.WipeStores();

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string root = isf.GetUserRootDirectory();
                isf.Remove();
                Assert.False(Directory.Exists(root), "store root folder should not exist");
                string parent = Path.GetDirectoryName(root.TrimEnd(Path.DirectorySeparatorChar));
                Assert.False(Directory.Exists(parent), $"identity folder {parent} should not exist");
            }
        }

        [Fact]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/114403", typeof(PlatformDetection), nameof(PlatformDetection.IsMacCatalyst))]
        public void RemoveUserStoreForAssembly()
        {
            TestHelper.WipeStores();

            using (var isf = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                string root = isf.GetUserRootDirectory();
                isf.Remove();
                Assert.False(Directory.Exists(root), "store root folder should not exist");
                string parent = Path.GetDirectoryName(root.TrimEnd(Path.DirectorySeparatorChar));
                Assert.False(Directory.Exists(parent), "identity folder should not exist");
            }
        }

        [Fact]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/114403", typeof(PlatformDetection), nameof(PlatformDetection.IsMacCatalyst))]
        public void RemoveUserStoreForDomain()
        {
            TestHelper.WipeStores();

            using (var isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                string root = isf.GetUserRootDirectory();
                isf.Remove();
                Assert.False(Directory.Exists(root), "store root folder should not exist");
                string parent = Path.GetDirectoryName(root.TrimEnd(Path.DirectorySeparatorChar));
                Assert.False(Directory.Exists(parent), "domain identity folder should not exist");
                parent = Path.GetDirectoryName(root.TrimEnd(Path.DirectorySeparatorChar));
                Assert.False(Directory.Exists(parent), "assembly identity folder should not exist");
            }
        }

        [Theory]
        [MemberData(nameof(ValidStores))]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/114403", typeof(PlatformDetection), nameof(PlatformDetection.IsMacCatalyst))]
        public void RemoveStoreWithContent(PresetScopes scope)
        {
            TestHelper.WipeStores();

            using (var isf = GetPresetScope(scope))
            {
                isf.CreateDirectory("RemoveStoreWithContent_Dir");
                using (isf.CreateFile("RemoveStoreWithContent_File")) { };
                string root = isf.GetUserRootDirectory();
                isf.Remove();
                Assert.False(Directory.Exists(root));
            }
        }
    }
}
