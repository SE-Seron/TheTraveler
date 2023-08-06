using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Thetraveler;

//用于完成玩家图像相关挂钩并构建相关的方法的类，方法的实际实现在PlayerGraphicsModule里面(也就是说这个部分的作用只是关联角色图像模块和Plugin类）
public class PlayerGraphic 
{
    private static ConditionalWeakTable<PlayerGraphics, PlayerGraphicsModule> modules;
    public PlayerGraphic() {}

    /*----------------------------------------------------------------------------------------------------------*/
    //这一条用于挂钩
    public void Hook()
    {
        modules = new ConditionalWeakTable<PlayerGraphics, PlayerGraphicsModule>();
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.PlayerGraphics.ctor += PlayerGraphics_ctor;
    }

    /*----------------------------------------------------------------------------------------------------------*/
    private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (!modules.TryGetValue(self, out _) && self.player.slugcatStats.name == Plugin.YourSlugID)
        {
            modules.Add(self, new PlayerGraphicsModule(self));

        }
        
    }
    /*----------------------------------------------------------------------------------------------------------*/
    //玩家图像更新
    private void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig.Invoke(self);
        bool flag = (modules.TryGetValue(self, out var module));
        if (flag)
        {
            module.RollUpdate();
        }
    }

    /*----------------------------------------------------------------------------------------------------------*/
    //玩家图像初始化
    private void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig.Invoke(self, sLeaser, rCam);
        bool flag = (modules.TryGetValue(self, out var module));
        if (flag)
        {
            module.InitiatSprites(sLeaser, rCam);
        }
    }

    /*----------------------------------------------------------------------------------------------------------*/
    //玩家图像绘制
    private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
        bool flag = (modules.TryGetValue(self, out var module));
        if (flag)
        {
            module.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }
}

