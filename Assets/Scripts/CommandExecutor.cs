using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class CommandExecutor : MonoBehaviour
{
    /// <summary>
    /// Voert een command uit in de vorm van "ObjectName MethodName [param1] [param2] ..."
    /// </summary>
    public void ExecuteCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            Debug.LogWarning("[CommandExecutor] Leeg commando ontvangen.");
            return;
        }

        // Split command op spaties
        string[] parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            Debug.LogError("[CommandExecutor] Ongeldig commando formaat. Gebruik: ObjectName MethodName [params]");
            return;
        }

        string objectName = parts[0];
        string methodName = parts[1];
        string[] parameters = parts.Skip(2).ToArray();

        Debug.Log($"[CommandExecutor] Uitvoeren: {objectName}.{methodName}({string.Join(", ", parameters)})");

        // Zoek het MonoBehaviour object in de scène
        MonoBehaviour targetObject = FindObjectByName(objectName);

        if (targetObject == null)
        {
            Debug.LogError($"[CommandExecutor] Object '{objectName}' niet gevonden in de scène.");
            return;
        }

        // Zoek de methode via reflectie
        Type objectType = targetObject.GetType();
        MethodInfo method = FindMethod(objectType, methodName, parameters.Length);

        if (method == null)
        {
            Debug.LogError($"[CommandExecutor] Methode '{methodName}' niet gevonden op '{objectName}' met {parameters.Length} parameter(s).");
            return;
        }

        // Converteer parameters naar de juiste types
        ParameterInfo[] methodParams = method.GetParameters();
        object[] convertedParams = new object[methodParams.Length];

        try
        {
            for (int i = 0; i < methodParams.Length; i++)
            {
                convertedParams[i] = ConvertParameter(parameters[i], methodParams[i].ParameterType);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CommandExecutor] Fout bij converteren van parameters: {ex.Message}");
            return;
        }

        // Roep de methode aan
        try
        {
            method.Invoke(targetObject, convertedParams);
            Debug.Log($"[CommandExecutor] Commando succesvol uitgevoerd: {objectName}.{methodName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CommandExecutor] Fout bij uitvoeren van methode: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    /// <summary>
    /// Zoekt een MonoBehaviour in de scène op basis van class naam
    /// </summary>
    private MonoBehaviour FindObjectByName(string objectName)
    {
        // Zoek alle MonoBehaviours in de scène
        MonoBehaviour[] allObjects = FindObjectsOfType<MonoBehaviour>();

        foreach (MonoBehaviour obj in allObjects)
        {
            // Vergelijk class naam (case insensitive)
            if (obj.GetType().Name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            {
                return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// Zoekt een methode met de juiste naam en aantal parameters
    /// </summary>
    private MethodInfo FindMethod(Type type, string methodName, int paramCount)
    {
        // Zoek alle publieke methods
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (MethodInfo method in methods)
        {
            if (method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase) &&
                method.GetParameters().Length == paramCount)
            {
                return method;
            }
        }

        return null;
    }

    /// <summary>
    /// Converteert een string parameter naar het gewenste type
    /// </summary>
    private object ConvertParameter(string value, Type targetType)
    {
        try
        {
            // Bool conversie
            if (targetType == typeof(bool))
            {
                if (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1")
                    return true;
                if (value.Equals("false", StringComparison.OrdinalIgnoreCase) || value == "0")
                    return false;
                throw new FormatException($"Kan '{value}' niet converteren naar bool");
            }

            // Int conversie
            if (targetType == typeof(int))
            {
                return int.Parse(value);
            }

            // Float conversie
            if (targetType == typeof(float))
            {
                return float.Parse(value);
            }

            // Double conversie
            if (targetType == typeof(double))
            {
                return double.Parse(value);
            }

            // String conversie
            if (targetType == typeof(string))
            {
                return value;
            }

            // Fallback: gebruik Convert.ChangeType
            return Convert.ChangeType(value, targetType);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Kan '{value}' niet converteren naar type {targetType.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Test methode om de executor te testen vanuit de Unity Inspector
    /// </summary>
    [ContextMenu("Test Command: Cheats NoClip")]
    public void TestCheatNoClip()
    {
        ExecuteCommand("Cheats NoClip");
    }

    [ContextMenu("Test Command: Player Heal 50")]
    public void TestPlayerHeal()
    {
        ExecuteCommand("Player Heal 50");
    }
}
