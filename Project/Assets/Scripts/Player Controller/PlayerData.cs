using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // Unit list
    public List<KeyValuePair<int, UnitBehavior>> selectedUnits = new List<KeyValuePair<int, UnitBehavior>>();
    public List<KeyValuePair<int, GroupLeader>> selectedLeaders = new List<KeyValuePair<int, GroupLeader>>();
}
