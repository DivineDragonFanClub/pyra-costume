using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DivineDragon
{
    public class Build
    {
        [MenuItem("Divine Dragon/Build Addressables")]
        public static void BuildAddressables()
        {
            buildAddressableContent();
        }
        
        [MenuItem("Divine Dragon/Set Output Path")]
        public static void SetOutputPath()
        {
            var outputPath = EditorUtility.OpenFolderPanel("Choose a folder to save the output to", "", "");
            if (outputPath == null)
            {
                Debug.Log("no path to asset bundle?");
                return;
            }
            DivineDragonSettingsScriptableObject.instance.outputPath = outputPath;
            Debug.Log(DivineDragonSettingsScriptableObject.instance.outputPath);
        }
        static bool buildAddressableContent()
        {
            AddressableAssetSettings
                .BuildPlayerContent(out AddressablesPlayerBuildResult result);
            bool success = string.IsNullOrEmpty(result.Error);

            if (!success)
            {
                Debug.LogError("Addressables build error encountered: " + result.Error);
            }
            
            var outputDirectory = DivineDragonSettingsScriptableObject.instance.outputPath;

            var args = String.Format("fix {0} {1}", outputDirectory, result.OutputPath);

            var bundleTools = "bundle_tools";
            // get the platform that this editor is running on
            var platform = Application.platform;
            
            // if the platform is windows, append .exe to the bundle_tools name
            if (platform == RuntimePlatform.WindowsEditor)
            {
                bundleTools += ".exe";
            }
            
            RunProcess(bundleTools, false, args);
            EditorUtility.RevealInFinder(outputDirectory);

            return success;
        }
        
        static void RunProcess(string command, bool runShell, string args = null)
        {
            string projectCurrentDir = Directory.GetCurrentDirectory();
            command = projectCurrentDir + "/Assets/" + command;
 
            UnityEngine.Debug.Log(string.Format("{0} Run command: {1}", DateTime.Now, command));
 
            ProcessStartInfo ps = new ProcessStartInfo(command);
            using (Process p = new Process())
            {
                ps.UseShellExecute = runShell;
                if (!runShell)
                {
                    ps.RedirectStandardOutput = true;
                    ps.RedirectStandardError = true;
                    ps.StandardOutputEncoding = System.Text.ASCIIEncoding.ASCII;
                }
                if (args != null && args != "")
                {
                    ps.Arguments = args;
                }
                p.StartInfo = ps;
                p.Start();
                p.WaitForExit();
                if (!runShell)
                {
                    string output = p.StandardOutput.ReadToEnd().Trim();
                    if (!string.IsNullOrEmpty(output))
                    {
                        // Split output into lines and debug log each line
                        string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        foreach (string line in lines)
                        {
                            UnityEngine.Debug.Log(string.Format("{0} Output: {1}", DateTime.Now, line));
                        }
                    }
 
                    string errors = p.StandardError.ReadToEnd().Trim();
                    if (!string.IsNullOrEmpty(errors))
                    {
                        // Split output into lines and debug log each line
                        string[] lines = errors.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        foreach (string line in lines)
                        {
                            UnityEngine.Debug.Log(string.Format("{0} Output: {1}", DateTime.Now, line));
                        }
                    }
                }
            }
        }

    }
    
}