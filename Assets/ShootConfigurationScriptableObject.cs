using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SHoot Config", menuName = "Guns/SHoot COnfiguration", order = 2)]

public class ShootConfigurationScriptableObject : ScriptableObject
{
    public LayerMask Hitmask;
    public Vector3 Spread = new Vector3(0.1f, 0.1f, 0.1f);
    public float FireRate = 0.25f;
}
