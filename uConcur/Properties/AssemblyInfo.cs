using System.Reflection;
using System.Runtime.InteropServices;
using uConcur.Properties;

[assembly: AssemblyTitle("uConcur")]
[assembly: AssemblyDescription("An overwrite blocking plugin for Umbraco.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Affinity ID")]
[assembly: AssemblyProduct("uConcur")]

[assembly: ComVisible(false)]
[assembly: Guid("836caa57-dbd3-4fef-b724-f5eafa979107")]

[assembly: AssemblyVersion(AssemblyInfo.VersionString)]
[assembly: AssemblyFileVersion(AssemblyInfo.VersionString)]
[assembly: AssemblyInformationalVersion(AssemblyInfo.InformationalVersionString)]

namespace uConcur.Properties {
    public static class AssemblyInfo {
        // please follow SemVer here:
        public const string VersionString = "0.1.0";
        public const string InformationalVersionString = VersionString + "-int-test-04";
    }
}