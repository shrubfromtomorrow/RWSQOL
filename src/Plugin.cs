using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
using System;
using UnityEngine;
using RWSQOL.Modules;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace RWSQOL;

[BepInPlugin("shrub.rwsqol", "Speedrunning QOL", "1.0.1")]
sealed class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    public static Plugin Instance;
    public Config options;
    bool IsInit;

    public void OnEnable()
    {
        Instance = this;
        Logger = base.Logger;
        options = new Config();
        On.RainWorld.OnModsInit += OnModsInit;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (IsInit) return;
        IsInit = true;

        try
        {
            Modules.Main.Apply();
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
        MachineConnector.SetRegisteredOI("shrub.rwsqol", options);
    }

    public void Update()
    {
        if (Input.anyKeyDown && Input.GetKeyDown(FastResetHandler.FastResetKey))
        {
            FastResetHandler.TriggerReset();
            Plugin.Logger.LogInfo("Reset button pressed");
        }
    }
}
