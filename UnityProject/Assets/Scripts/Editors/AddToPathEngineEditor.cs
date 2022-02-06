using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AddToPathEngine))]
[CanEditMultipleObjects]

public class AddDispenserToHandlerEditor : Editor
{
    SerializedProperty hasSpecular;
    SerializedProperty specular;
    SerializedProperty alpha;

    SerializedProperty hasAlbedo;
    SerializedProperty albedo;

    SerializedProperty hasEmission;
    SerializedProperty emissionColor;
    SerializedProperty emissionStrength;

    void OnEnable()
    {
        hasSpecular = serializedObject.FindProperty("hasSpecular");
        specular = serializedObject.FindProperty("specular");
        alpha = serializedObject.FindProperty("alpha");

        hasAlbedo = serializedObject.FindProperty("hasAlbedo");
        albedo = serializedObject.FindProperty("albedo");

        hasEmission = serializedObject.FindProperty("hasEmission");
        emissionColor = serializedObject.FindProperty("emissionColor");
        emissionStrength = serializedObject.FindProperty("emissionStrength");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(hasSpecular);

        if (hasSpecular.boolValue)
        {
            EditorGUILayout.PropertyField(specular);
            EditorGUILayout.PropertyField(alpha);
        }

        EditorGUILayout.PropertyField(hasAlbedo);

        if (hasAlbedo.boolValue)
            EditorGUILayout.PropertyField(albedo);

        EditorGUILayout.PropertyField(hasEmission);

        if (hasEmission.boolValue)
        {
            EditorGUILayout.PropertyField(emissionColor);
            EditorGUILayout.PropertyField(emissionStrength);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
