/*
        _            _            _           _            _          
       / /\         /\ \         /\ \        /\ \         /\ \     _  
      / /  \       /  \ \       /  \ \      /  \ \       /  \ \   /\_\
     / / /\ \__   / /\ \ \     / /\ \ \    / /\ \ \     / /\ \ \_/ / /
    / / /\ \___\ / / /\ \_\   / / /\ \_\  / / /\ \ \   / / /\ \___/ / 
    \ \ \ \/___// /_/_ \/_/  / / /_/ / / / / /  \ \_\ / / /  \/____/  
     \ \ \     / /____/\    / / /__\/ / / / /   / / // / /    / / /   
 _    \ \ \   / /\____\/   / / /_____/ / / /   / / // / /    / / /    
/_/\__/ / /  / / /______  / / /\ \ \  / / /___/ / // / /    / / /     
\ \/___/ /  / / /_______\/ / /  \ \ \/ / /____\/ // / /    / / /      
 \_____\/   \/__________/\/_/    \_\/\/_________/ \/_/     \/_/       

*/
using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using RWCustom;
using Smoke;
using IL;
using MonoMod.ModInterop;
using static Thetraveler.GhostPlayerImports;

namespace Thetraveler 
{
    [BepInPlugin(MOD_ID, "The Traveler", "0.2.4")]
    public class Plugin : BaseUnityPlugin 
    {
        /*-----------------------------------------------------字段-----------------------------------------------------*/
        //设置ModID
        private const string MOD_ID = "seron.thetraveler";
        //添加了一个自定义属性IsTravelerCat，可以通过读这个属性判断是否是旅者角色
        public static readonly PlayerFeature<bool> IsTravelerCat = PlayerBool("thetraveler/is_traveler_cat");
        //用于检查角色id，上面那条不可用时的替代
        public static readonly SlugcatStats.Name YourSlugID = new SlugcatStats.Name("TheTraveler", false);
        //PlayerGraphic模块
        public PlayerGraphic playerGraphic = new PlayerGraphic();
        //LeapBurst效果
        public LeapBurst burst = new LeapBurst();
        //角色能力的状态和冷却时间
        bool player_skill = false;
        float player_skill_cd = 0f;
        //Remix菜单实例
        private OptionsMenu optionsMenuInstance;
        //检查GhostPlayer是否启用
        public static bool enableGhostPlayer = false;
        /*-----------------------------------------------------挂钩-----------------------------------------------------*/
        public void OnEnable() 
        {
            //模组初始化-加载自定义贴图
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            //角色更新-弹射能力和其他技能
            On.Player.Update += Player_Skills;
            //启用PlayerGraphic模块的挂钩和方法
            playerGraphic.Hook();
            //加载GhostPlayer扩展
            typeof(GhostPlayerImports).ModInterop();
            enableGhostPlayer = GhostPlayerImports.IsNetworkPlayer != null;
            if (enableGhostPlayer)
            {
                GhostPlayerImports.Register(typeof(TravelerData));
            }

        }
        /*-----------------------------------------------------方法-----------------------------------------------------*/
        //在模组初始化时加载尾巴以外的自定义贴图和Remix菜单
        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            //加载自定义角色贴图
            Futile.atlasManager.LoadAtlas("atlases/travelerarm");
            Futile.atlasManager.LoadAtlas("atlases/travelerbody");
            Futile.atlasManager.LoadAtlas("atlases/travelerface");
            Futile.atlasManager.LoadAtlas("atlases/travelerhead");
            Futile.atlasManager.LoadAtlas("atlases/travelerhips");
            Futile.atlasManager.LoadAtlas("atlases/travelerlegs");
            Futile.atlasManager.LoadAtlas("atlases/travelerheadC"); 
            

            //加载Remix菜单
            optionsMenuInstance = new OptionsMenu(this);
            try
            {
                MachineConnector.SetRegisteredOI("seron.thetraveler", optionsMenuInstance);
            }
            catch (Exception ex)
            {
                Debug.Log($"Remix Menu Template examples: Hook_OnModsInit options failed init error {optionsMenuInstance}{ex}");
                Logger.LogError(ex);
                Logger.LogMessage("哼，哼，啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊");
            } 
            
        }
        //角色能力的实现，包括子弹时间和弹射
        private void Player_Skills(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            // 冷却时间每次更新时衰减
            if (player_skill_cd > 0f) 
            {
                player_skill_cd -= 1f; 
            }

            //判断是否为TheTraveler角色 并判断是否可以触发技能
            if (IsTravelerCat.TryGet(self, out bool is_traveler_cat) && is_traveler_cat)
            {
                //弹射条件判断:没有死亡或昏迷 非涉水/捷径/管道/匍匐/爬墙状态 且满足站立/杆上/鹿角/藤蔓中任一
                bool player_can_launch =
                       self.Consious
                    && self.bodyMode != Player.BodyModeIndex.Stunned
                    && self.bodyMode != Player.BodyModeIndex.Swimming
                    && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut
                    && self.bodyMode != Player.BodyModeIndex.CorridorClimb
                    && self.bodyMode != Player.BodyModeIndex.Crawl
                    && self.bodyMode != Player.BodyModeIndex.WallClimb
                    &&(self.bodyMode == Player.BodyModeIndex.Stand
                    || self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam
                    || self.animation == Player.AnimationIndex.VineGrab
                    || self.animation == Player.AnimationIndex.AntlerClimb
                    || self.bodyMode == Player.BodyModeIndex.ZeroG);
                //此处用于其他客户端上的角色播放弹射效果
                if (enableGhostPlayer && IsNetworkPlayer(self))
                {
                    if (TryGetImportantValue(typeof(TravelerData), out var obj) &&
                        ((TravelerData)obj).id == GetPlayerNetID(self))
                    {
                        burst.Burst(self);
                    }
                    return;
                }

                //按键且冷却完成并且自己为当前客户端的角色则触发能力
                if (Input.GetKey(optionsMenuInstance.skillKeyCode.Value) && player_skill_cd == 0f && (!enableGhostPlayer || !GhostPlayerImports.IsNetworkPlayer(self)))
                {
                    player_skill = true;
                }

                if (player_skill == true) 
                {
                    //蘑菇效果
                    self.mushroomEffect = 1.0f;

                    //如果在技能中松开按键，结束子弹时间，设置冷却，判断是否弹射
                    if (!Input.GetKey(optionsMenuInstance.skillKeyCode.Value)) 
                    { 
                        self.mushroomEffect = 0f;
                        player_skill = false;
                        player_skill_cd = 10f;

                        //有方向键输入且满足条件时开始弹射
                        if ((self.input[0].x != 0 || self.input[0].y != 0) && player_can_launch) 
                        {
                            burst.Burst(self);

                            /*---------------------------------------------------------------------------------------------------------------------*/
                            //以上为视觉效果实现-以下为弹射物理效果实现
                            /*---------------------------------------------------------------------------------------------------------------------*/
                            if (self.bodyMode == Player.BodyModeIndex.ZeroG)
                            {
                                self.bodyChunks[0].vel.x = 9f * (float)self.input[0].x;
                                self.bodyChunks[1].vel.x = 9f * (float)self.input[0].x;
                                self.bodyChunks[0].vel.y = 9f * (float)self.input[0].y;
                                self.bodyChunks[1].vel.y = 9f * (float)self.input[0].y;
                            }
                            else
                            {
                                if (self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam || self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.VineGrab || self.animation == Player.AnimationIndex.AntlerClimb)
                                {
                                    self.bodyMode = Player.BodyModeIndex.Default;

                                    self.bodyChunks[0].vel.x = 13f * (float)self.input[0].x;
                                    self.bodyChunks[1].vel.x = 11f * (float)self.input[0].x;
                                    self.bodyChunks[0].vel.y = 3f + 17f * (float)self.input[0].y;
                                    self.bodyChunks[1].vel.y = 3f + 13f * (float)self.input[0].y;
                                }
                                else if (self.input[0].x == 0)
                                {
                                    self.bodyChunks[0].vel.y = 21f * (float)self.input[0].y;
                                    self.bodyChunks[1].vel.y = 15f * (float)self.input[0].y;
                                    self.jumpBoost = 2f;
                                }
                                else if (self.input[0].y == 0)
                                {
                                    self.bodyChunks[0].vel.x = 15f * (float)self.input[0].x;
                                    self.bodyChunks[1].vel.x = 11f * (float)self.input[0].x;
                                    self.bodyChunks[0].vel.y = 5f;
                                    self.bodyChunks[1].vel.y = 5f;
                                    self.jumpBoost = 2f;
                                }
                                else
                                {
                                    self.bodyChunks[0].vel.x = 13f * (float)self.input[0].x;
                                    self.bodyChunks[1].vel.x = 11f * (float)self.input[0].x;
                                    self.bodyChunks[0].vel.y = 3f + 17f * (float)self.input[0].y;
                                    self.bodyChunks[1].vel.y = 3f + 13f * (float)self.input[0].y;
                                    self.jumpBoost = 2f;
                                }
                            }

                            self.animation = Player.AnimationIndex.Flip;
                            self.noGrabCounter = 5; 
                        }
                    }  
                }
            }
        }
    }
    //GhostPlayer联机API
    [ModImportName("GhostPlayerExtension")]
    public static class GhostPlayerImports
    {
        public delegate bool TryGetImportantValueDel(Type type, out object obj);
        public delegate bool TryGetValueForPlayerDel(Player player, Type type, out object obj);

        public static Func<Type, bool> Register;

        public static TryGetValueForPlayerDel TryGetValueForPlayer;
        public static Func<Player, object, bool> TrySetValueForPlayer;

        public static TryGetImportantValueDel TryGetImportantValue;
        public static Func<object, bool, bool> TrySendImportantValue;

        public static Func<Player, string, bool> SendMessage;
        //public static Func<Player, string, bool> SendConsoleMessage;

        public static Action<Action<string[]>> RegisterCommandEvent;

        public static Func<Player, int> GetPlayerNetID;
        public static Func<Player, string> GetPlayerNetName;
        public static Func<Player, bool> IsNetworkPlayer;
        public static Func<bool> IsConnected;

        //public static Func<string, string> GetPlayerRoom;
        //public static Func<string, string> GetPlayerRegion;

    }
    public class TravelerData
    {
        public int id;
    }
}