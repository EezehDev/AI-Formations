using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // Unit list
    public List<UnitBehavior> selectedUnits = new List<UnitBehavior>();
    public List<GroupLeader> selectedLeaders = new List<GroupLeader>();

    // Groups
    const int maxGroups = 4;
    public bool[] groups = new bool[maxGroups];
    public Material[] groupMaterials = new Material[maxGroups];


    // -------------------------
    // GETTERS & SETTERS
    // -------------------------

    public int GetMaxGroupCount()
    {
        return maxGroups;
    }
}
