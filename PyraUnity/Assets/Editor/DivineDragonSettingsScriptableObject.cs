using UnityEditor;
using UnityEngine;

namespace DivineDragon
{
    [CreateAssetMenu(fileName = "DivineDragonSettings", menuName = "Divine Dragon/Initialize Divine Dragon", order = 1)]
    public class DivineDragonSettingsScriptableObject : ScriptableObject
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