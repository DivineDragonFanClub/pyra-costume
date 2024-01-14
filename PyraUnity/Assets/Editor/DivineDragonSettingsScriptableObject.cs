using UnityEditor;
using UnityEngine;

namespace DivineDragon
{
    [FilePath("DivineDragon/StateFile.foo", FilePathAttribute.Location.PreferencesFolder)]
    public class DivineDragonSettingsScriptableObject : ScriptableSingleton<DivineDragonSettingsScriptableObject>
    {
        public string outputPath;
    }
    
    [CustomEditor(typeof(DivineDragonSettingsScriptableObject))]
    public class OBJEditor: UnityEditor.Editor 
    {
        DivineDragonSettingsScriptableObject obj = null;
        protected void OnEnable()
        {
            obj = (DivineDragonSettingsScriptableObject)target;
        }
        public override void OnInspectorGUI() 
        {
            serializedObject.Update();

            _ = DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();

            bool somethingChanged = EditorGUI.EndChangeCheck();
            if(somethingChanged)
            {
                EditorUtility.SetDirty(obj);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}