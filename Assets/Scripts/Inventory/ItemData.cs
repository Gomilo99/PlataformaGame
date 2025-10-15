using UnityEngine;

/// <summary>
/// Datos de ítem simples. Puedes convertirlo a ScriptableObject si necesitas muchas definiciones.
/// </summary>
[System.Serializable]
public class ItemData
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int maxStack = 99;
}
