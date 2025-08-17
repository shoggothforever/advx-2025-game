using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
using System;
using System.Reflection;
using System.Linq.Expressions;

public class LevelFactory
{
    static Type simulte_ =typeof(Simulation);
    // 手动字典：键 = 场景名，值 = 具体类
    private static readonly Dictionary<string, System.Type> map = new()
    {
        { "default", typeof(DefaultLevel) },
        { "playground", typeof(PlayGroundLevel) },
        { "Tutorial1", typeof(Tutorial1) }
        // 后续关卡继续加
    };
    public static ILevelLogic Create(string levelName)
    {
        if (!map.TryGetValue(levelName, out var type))
            return (ILevelLogic)Activator.CreateInstance(map["default"]);
        Debug.LogFormat("find levelLogic of {0}", levelName);
        MethodInfo registe = simulte_.GetMethod("SetLevelLogic")
                               .MakeGenericMethod(type);
        var ll = ((ILevelLogic)Activator.CreateInstance(type));
        object[] parameters = new object[] { ll };
        registe.Invoke(null, parameters);
        MethodInfo acquire = simulte_.GetMethod("GetLevelLogic")
                       .MakeGenericMethod(type);
        return (ILevelLogic)acquire.Invoke(null, null);
    }
}

public interface ILevelLogic
{
    public void DoBeforeLevel();
    public void DoWholeLevel();
    public void DoInPreReverse();
    public void DoInReverse();
    public void DoInPreForward();
    public void DoInForward();
    public void DoAfterLevel();
}
public abstract class EmptyLevel : ILevelLogic
{
    public virtual void DoBeforeLevel()
    {
        return;
    }
    public virtual void DoInForward()
    {
        return;
    }

    public virtual void DoInPreForward()
    {
        return;
    }

    public virtual void DoInPreReverse()
    {
        return;
    }

    public virtual void DoInReverse()
    {
        return;
    }

    public virtual void DoWholeLevel()
    {
        return;
    }
    public virtual void DoAfterLevel()
    {
        return;
    }
}
public class DefaultLevel : EmptyLevel, ILevelLogic
{
}
public class PlayGroundLevel : EmptyLevel,ILevelLogic
{
}
public class Tutorial1 : EmptyLevel, ILevelLogic
{
    public override void DoBeforeLevel()
    {
        GameManager.Instance.SetCapability(Capability.PlaceForwardZone);
        GameManager.Instance.pastPlayer.GetComponent<BlockPlacer>().enabled = true;
    }
    public override void DoWholeLevel()
    {
        
    }
}