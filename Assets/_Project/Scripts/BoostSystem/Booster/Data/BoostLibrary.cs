using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boost/Boost Library", fileName = "BoostLibrary")]
public class BoostLibrary : ScriptableObject
{
    public List<BoostZonePreset> Presets = new List<BoostZonePreset>();
}