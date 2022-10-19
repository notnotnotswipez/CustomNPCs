using MelonLoader;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(CustomNPCs.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(CustomNPCs.BuildInfo.Company)]
[assembly: AssemblyProduct(CustomNPCs.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + CustomNPCs.BuildInfo.Author)]
[assembly: AssemblyTrademark(CustomNPCs.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(CustomNPCs.BuildInfo.Version)]
[assembly: AssemblyFileVersion(CustomNPCs.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(CustomNPCs.CustomNPCs), CustomNPCs.BuildInfo.Name, CustomNPCs.BuildInfo.Version, CustomNPCs.BuildInfo.Author, CustomNPCs.BuildInfo.DownloadLink)]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]