using System;
using System.Security.Permissions;
using UnityEngine;

/*
 * This file contains fixes to some common problems when modding Rain World.-这个文件为雨世界的mod制作中的一些常见问题提供修复
 * Unless you know what you're doing, you shouldn't modify anything here.-除非你知道你在做什么，否则不应该对这里做任何更改
 */

// Allows access to private members-允许对私有成员的访问
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618


internal static class Extras
{
    private static bool _initialized;

    // Ensure resources are only loaded once and that failing to load them will not break other mods-确保资源仅加载一次 并且加载失败的资源不影响其他模组
    public static On.RainWorld.hook_OnModsInit WrapInit(Action<RainWorld> loadResources)
    {
        return (orig, self) =>
        {
            orig(self);

            try
            {
                if (!_initialized)
                {
                    _initialized = true;
                    loadResources(self);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        };
    }
}