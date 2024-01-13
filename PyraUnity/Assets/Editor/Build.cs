using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
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
        static bool buildAddressableContent()
        {
            AddressableAssetSettings
                .BuildPlayerContent(out AddressablesPlayerBuildResult result);
            bool success = string.IsNullOrEmpty(result.Error);

            if (!success)
            {
                Debug.LogError("Addressables build error encountered: " + result.Error);
            }
            
            var outputDirectory = Directory.GetCurrentDirectory() + "/bundle_tools_output";

            var args = String.Format("fix {0} {1}", outputDirectory, result.OutputPath);
            

            EditorUtility.RevealInFinder(outputDirectory);
            RunProcess("bundle_tools", false, args);

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
                        UnityEngine.Debug.Log(string.Format("{0} Output: {1}", DateTime.Now, errors));
                    }
                }
            }
        }

    }
    
}