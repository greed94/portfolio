using System;
using System.Collections.Generic;
using UnityEngine;

public enum SexType { W, M }
public enum AvatarType { Player, NPC, Meti }

[Serializable]
public class MetiTable
{
    public int layer;
    public Vector3 playerPos;
    public Vector3 playerRot;
    public SexType sex;
    public AvatarType avatar;
    public string npcNo;
    public List<string> dressList = new List<string>();
}
