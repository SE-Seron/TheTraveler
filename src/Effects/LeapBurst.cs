using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using Smoke;
using JetBrains.Annotations;
using Thetraveler;
using static Thetraveler.GhostPlayerImports;

public class LeapBurst
{
    public void Burst(Player self)
        {
        //发送给其他客户端弹射特效
        if (Plugin.enableGhostPlayer && !IsNetworkPlayer(self))
        {
            TrySendImportantValue(new TravelerData() { id = GetPlayerNetID(self) }, false);
        }

        //获取玩家所在房间 位置 速度
        var room = self.room;
        var pos = self.mainBodyChunk.pos;
        Vector2 vel = self.mainBodyChunk.vel;

        //生成烟雾
        FireSmoke smoke = new FireSmoke(room);
        room.AddObject(smoke);

        //循环发射青色烟雾25次
        for (int i = 0; i < 25; i++)
        {
            smoke.EmitSmoke(pos, vel * UnityEngine.Random.value * 0.5f, new Color(0f, 1f, 1f), 20);
        }

        //冲击波和光效
        room.AddObject(new ShockWave(pos, 130f + UnityEngine.Random.value * 50f, 0.045f, 10, false));
        room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(0.2f, 1f, 1f)));

        //随机选择音效播放
        System.Random random = new System.Random();
        double r = random.NextDouble();
        if (r < 0.1)
        {
            room.PlaySound(SoundID.Cyan_Lizard_Powerful_Jump, pos, 0.8f + UnityEngine.Random.value * 0.8f, 0.5f + UnityEngine.Random.value * 1.0f);
        }
        else if (r < 0.7)
        {
            room.PlaySound(SoundID.Cyan_Lizard_Medium_Jump, pos, 0.7f + UnityEngine.Random.value * 0.5f, 0.5f + UnityEngine.Random.value * 1.5f);
        }
        else
        {
            room.PlaySound(SoundID.Cyan_Lizard_Small_Jump, pos, 0.9f + UnityEngine.Random.value * 0.5f, 0.5f + UnityEngine.Random.value * 1.5f);
        }
    }
}

