using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Beach/ObjectData")]
public class BeachObjectData : ScriptableObject
{
    public string objectName;
    public bool isValuable;
    public MemoryFlag memory = MemoryFlag.NONE;
    public GameObject objectPrefab;
}

/*

ITEM IDEAS

Valuable:
Generic Console
Nokia flip phone
A beer bottle
A Melodica
Childhood toy

Not Valueable:
Somebody's Microwave
Rock
Sea Shell
Kelp/seaweed
Needle
An old coin
Plastic bucket with a hole in it
A plate of meatballs
Garbage (cans, banana peel, broken glass)

 */