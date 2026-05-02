using System.IO;
using UnityEditor;
using UnityEngine;

namespace CMS.Bootstrap
{
    // Standalone asmdef so this still compiles when CMS.asmdef fails due to the missing dep.
    [InitializeOnLoad]
    internal static class CMSDependencyBootstrap
    {
        const string DepId = "com.mackysoft.serializereference-extensions";
        const string DepVersion = "1.6.1";
        const string RegistryName = "OpenUPM";
        const string RegistryUrl = "https://package.openupm.com";
        const string RegistryScope = "com.mackysoft";

        const string SkipPrefKey = "CMS.DependencyBootstrap.Skip";
        const string SessionKey = "CMS.DependencyBootstrap.Checked";
        const string MenuPath = "CMS/Check Dependencies";

        static CMSDependencyBootstrap()
        {
            EditorApplication.delayCall += AutoCheck;
        }

        static void AutoCheck()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);
            if (EditorPrefs.GetBool(SkipPrefKey, false)) return;
            Run(force: false);
        }

        [MenuItem(MenuPath)]
        static void MenuCheck()
        {
            EditorPrefs.SetBool(SkipPrefKey, false);
            Run(force: true);
        }

        static void Run(bool force)
        {
            string manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");
            if (!File.Exists(manifestPath))
            {
                if (force) EditorUtility.DisplayDialog("CMS", "Could not find Packages/manifest.json.", "OK");
                return;
            }

            string manifest = File.ReadAllText(manifestPath);
            bool hasRegistry = manifest.Contains("\"" + RegistryUrl + "\"") && manifest.Contains("\"" + RegistryScope + "\"");
            bool hasDep = manifest.Contains("\"" + DepId + "\"");

            if (hasRegistry && hasDep)
            {
                if (force) EditorUtility.DisplayDialog("CMS", "Dependencies already installed.", "OK");
                return;
            }

            int choice = EditorUtility.DisplayDialogComplex(
                "CMS — Add OpenUPM Dependency?",
                "CMS depends on '" + DepId + "' from the OpenUPM registry.\n\n" +
                "Add the OpenUPM scoped registry and dependency to this project's Packages/manifest.json?",
                "Yes, add it",
                "Not now",
                "Don't ask again"
            );

            if (choice == 0)
            {
                if (TryPatch(manifest, hasRegistry, hasDep, out string patched))
                {
                    File.WriteAllText(manifestPath, patched);
                    AssetDatabase.Refresh();
                    Debug.Log("[CMS] Added OpenUPM scoped registry and " + DepId + " to Packages/manifest.json.");
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "CMS",
                        "Could not safely patch Packages/manifest.json. Add the OpenUPM scoped registry and dependency manually — see the CMS README.",
                        "OK"
                    );
                }
            }
            else if (choice == 2)
            {
                EditorPrefs.SetBool(SkipPrefKey, true);
                Debug.Log("[CMS] Dependency check disabled. Re-enable via menu '" + MenuPath + "'.");
            }
        }

        static bool TryPatch(string manifest, bool hasRegistry, bool hasDep, out string result)
        {
            result = manifest;
            if (!hasRegistry && !TryAddScopedRegistry(result, out result)) return false;
            if (!hasDep && !TryAddDependency(result, out result)) return false;
            return true;
        }

        static bool TryAddScopedRegistry(string manifest, out string result)
        {
            result = manifest;
            string entry =
                "{\n" +
                "      \"name\": \"" + RegistryName + "\",\n" +
                "      \"url\": \"" + RegistryUrl + "\",\n" +
                "      \"scopes\": [ \"" + RegistryScope + "\" ]\n" +
                "    }";

            int srIdx = manifest.IndexOf("\"scopedRegistries\"");
            if (srIdx < 0)
            {
                int braceIdx = manifest.IndexOf('{');
                if (braceIdx < 0) return false;
                result = manifest.Insert(braceIdx + 1,
                    "\n  \"scopedRegistries\": [\n    " + entry + "\n  ],");
                return true;
            }

            int openBracket = manifest.IndexOf('[', srIdx);
            if (openBracket < 0) return false;
            int closeBracket = manifest.IndexOf(']', openBracket);
            if (closeBracket < 0) return false;

            bool empty = manifest.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim().Length == 0;
            string insertion = empty
                ? "\n    " + entry + "\n  "
                : "\n    " + entry + ",";
            result = manifest.Insert(openBracket + 1, insertion);
            return true;
        }

        static bool TryAddDependency(string manifest, out string result)
        {
            result = manifest;
            int depIdx = manifest.IndexOf("\"dependencies\"");
            if (depIdx < 0) return false;
            int openBrace = manifest.IndexOf('{', depIdx);
            if (openBrace < 0) return false;
            int closeBrace = manifest.IndexOf('}', openBrace);
            if (closeBrace < 0) return false;

            string entry = "\"" + DepId + "\": \"" + DepVersion + "\"";
            bool empty = manifest.Substring(openBrace + 1, closeBrace - openBrace - 1).Trim().Length == 0;
            string insertion = empty
                ? "\n    " + entry + "\n  "
                : "\n    " + entry + ",";
            result = manifest.Insert(openBrace + 1, insertion);
            return true;
        }
    }
}
