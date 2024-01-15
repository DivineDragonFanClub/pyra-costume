using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace DivineDragon
{
    [FilePath("DivineDragon/StateFile.foo", FilePathAttribute.Location.PreferencesFolder)]
    public class DivineDragonSettingsScriptableObject : ScriptableSingleton<DivineDragonSettingsScriptableObject>
    {
        public string bundleOutputPath;
    }

    /// <summary>
    /// Copied from https://www.kodeco.com/6452218-uielements-tutorial-for-unity-getting-started?page=2
    /// </summary>
    public class SettingsWindow: EditorWindow
    {
        [MenuItem("Divine Dragon/Divine Dragon Window")]
        public static void ShowSettings()
        {
            SettingsWindow wnd = GetWindow<SettingsWindow>();
            wnd.titleContent = new GUIContent("Divine Dragon Window");
        }
        
        public void OnEnable()
        {
            // 3
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // 5
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>
                ("Assets/Editor/DivineWindow.uxml");
            VisualElement divineWindow = visualTree.CloneTree();
            root.Add(divineWindow);
            
            InitializeBundleOutputField(divineWindow);
            InitializeBrowseButton(divineWindow);
            InitializeOpenOutputButton(divineWindow);
            InitializeBuildButton(divineWindow);
            
            
        }
        
        private void InitializeBundleOutputField(VisualElement divineWindow)
        {
            TextField bundleOutputField = divineWindow.Q<TextField>("BundleOutputField");
            
            bundleOutputField.value = DivineDragonSettingsScriptableObject.instance.bundleOutputPath;
            
            // reflect edits of the field back to the scriptable object
            bundleOutputField.RegisterValueChangedCallback(evt =>
            {
                DivineDragonSettingsScriptableObject.instance.bundleOutputPath = evt.newValue;
            });
        }
        
        private void InitializeBrowseButton(VisualElement divineWindow)
        {
            Button browseButton = divineWindow.Q<Button>("BrowseOutputPath");
            
            browseButton.clickable.clicked += () =>
            {
                var outputPath = EditorUtility.OpenFolderPanel("Choose a folder to save the output to", "", "");
                if (string.IsNullOrEmpty(outputPath))
                {
                    Debug.Log("no path to output?");
                    return;
                }
                DivineDragonSettingsScriptableObject.instance.bundleOutputPath = outputPath;
                Debug.Log("Set the output path to " + DivineDragonSettingsScriptableObject.instance.bundleOutputPath);
            };
        }
        
        private void InitializeOpenOutputButton(VisualElement divineWindow)
        {
            Button openOutputButton = divineWindow.Q<Button>("OpenOutputPath");
            
            openOutputButton.clickable.clicked += () =>
            {
                EditorUtility.OpenWithDefaultApp(DivineDragonSettingsScriptableObject.instance.bundleOutputPath);
            };
        }
        
        private void InitializeBuildButton(VisualElement divineWindow)
        {
            Button buildButton = divineWindow.Q<Button>("BuildAddressablesDivine");
            
            buildButton.clickable.clicked += () =>
            {
                Build.BuildAddressableContent();
            };
        }

    }
}