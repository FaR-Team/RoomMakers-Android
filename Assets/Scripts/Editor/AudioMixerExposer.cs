using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

#if UNITY_EDITOR
[CustomEditor(typeof(AudioManager))]
public class AudioMixerExposer : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        AudioManager audioManager = (AudioManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audio Mixer Parameter Helper", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Expose Lowpass Parameter"))
        {
            SerializedProperty mixerProperty = serializedObject.FindProperty("audioMixer");
            AudioMixer mixer = mixerProperty.objectReferenceValue as AudioMixer;
            
            if (mixer != null)
            {
                SerializedObject mixerSerializedObject = new SerializedObject(mixer);
                SerializedProperty exposedParams = mixerSerializedObject.FindProperty("m_ExposedParameters");
                
                bool parameterExists = false;
                
                // Check if parameter already exists
                for (int i = 0; i < exposedParams.arraySize; i++)
                {
                    SerializedProperty param = exposedParams.GetArrayElementAtIndex(i);
                    if (param.FindPropertyRelative("name").stringValue == "Lowpass")
                    {
                        parameterExists = true;
                        break;
                    }
                }
                
                if (!parameterExists)
                {
                    int index = exposedParams.arraySize;
                    exposedParams.arraySize++;
                    SerializedProperty newParam = exposedParams.GetArrayElementAtIndex(index);
                    newParam.FindPropertyRelative("name").stringValue = "Lowpass";
                    newParam.FindPropertyRelative("type").intValue = 1; // Float type
                    
                    mixerSerializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(mixer);
                    AssetDatabase.SaveAssets();
                    
                    Debug.Log("Lowpass parameter exposed in AudioMixer");
                }
                else
                {
                    Debug.Log("Lowpass parameter already exposed in AudioMixer");
                }
            }
            else
            {
                Debug.LogError("No AudioMixer assigned to AudioManager");
            }
        }
    }
}
#endif