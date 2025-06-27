using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ShaderGlobalVariablesUpdater : MonoBehaviour
{
    [System.Serializable]
    public class Variable
    {
        public string globalReference;
        public enum VariableType { Float, Texture, Color, Vector }
        public VariableType selectedType;
        public float floatValue;
        public Texture textureValue;
        public Color colorValue;
        public Vector4 vectorValue;
    }

    public List<Variable> variables = new List<Variable>(); // List of shader variables

#if UNITY_EDITOR
    [CustomEditor(typeof(ShaderGlobalVariablesUpdater))]
    public class ShaderGlobalVariablesUpdaterEditor : Editor
    {
        private ShaderGlobalVariablesUpdater shaderGlobalVariablesUpdater;

        private void OnEnable()
        {
            // Get the instance of ShaderGlobalVariablesUpdater
            shaderGlobalVariablesUpdater = (ShaderGlobalVariablesUpdater)target;
            // Register the UpdateVariables method to be called in the Editor update loop
            EditorApplication.update += UpdateVariables;
        }

        private void OnDisable()
        {
            // Unregister the UpdateVariables method from the Editor update loop
            EditorApplication.update -= UpdateVariables;
        }

        // Method to update the shader variable values
        private void UpdateVariables()
        {
            foreach (Variable variable in shaderGlobalVariablesUpdater.variables)
            {
                UpdateShaderGlobalVariable(variable);
            }
        }

        // Method to draw the custom inspector for ShaderGlobalVariablesUpdater
        public override void OnInspectorGUI()
        {
            // Get the instance of ShaderGlobalVariablesUpdater
            shaderGlobalVariablesUpdater = (ShaderGlobalVariablesUpdater)target;

            // Loop through each variable and draw its inspector
            for (int i = 0; i < shaderGlobalVariablesUpdater.variables.Count; i++)
            {
                DrawVariableInspector(shaderGlobalVariablesUpdater.variables[i], i);
            }

            // Button to add a new variable
            if (GUILayout.Button("Add Variable"))
            {
                shaderGlobalVariablesUpdater.variables.Add(new ShaderGlobalVariablesUpdater.Variable());
            }
        }

        // Method to draw the inspector for a single variable
        private void DrawVariableInspector(Variable variable, int index)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Variable", EditorStyles.boldLabel);
            variable.globalReference = EditorGUILayout.TextField("Global Variable Reference", variable.globalReference);
            variable.selectedType = (Variable.VariableType)EditorGUILayout.EnumPopup("Variable Type", variable.selectedType);

            switch (variable.selectedType)
            {
                case Variable.VariableType.Float:
                    variable.floatValue = EditorGUILayout.FloatField("Float Value", variable.floatValue);
                    break;
                case Variable.VariableType.Texture:
                    variable.textureValue = (Texture)EditorGUILayout.ObjectField("Texture Value", variable.textureValue, typeof(Texture), false);
                    break;
                case Variable.VariableType.Color:
                    variable.colorValue = EditorGUILayout.ColorField("Color Value", variable.colorValue);
                    break;
                case Variable.VariableType.Vector:
                    variable.vectorValue = EditorGUILayout.Vector4Field("Vector Value", variable.vectorValue);
                    break;
            }

            EditorGUILayout.BeginHorizontal();

            // Button to remove the variable
            if (GUILayout.Button("Remove Variable"))
            {
                shaderGlobalVariablesUpdater.variables.RemoveAt(index);
                return;
            }

            EditorGUILayout.EndHorizontal();

            // Update the shader global variable based on the variable's data
            UpdateShaderGlobalVariable(variable);

            GUILayout.EndVertical();
        }

        // Method to update the shader global variable based on the variable's data
        private void UpdateShaderGlobalVariable(Variable variable)
        {
            switch (variable.selectedType)
            {
                case Variable.VariableType.Float:
                    Shader.SetGlobalFloat(variable.globalReference, variable.floatValue);
                    break;
                case Variable.VariableType.Texture:
                    Shader.SetGlobalTexture(variable.globalReference, variable.textureValue);
                    break;
                case Variable.VariableType.Color:
                    Shader.SetGlobalColor(variable.globalReference, variable.colorValue);
                    break;
                case Variable.VariableType.Vector:
                    Shader.SetGlobalVector(variable.globalReference, variable.vectorValue);
                    break;
            }
        }
    }
#endif
}
