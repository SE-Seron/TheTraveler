using BepInEx;
using IL.Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thetraveler;
using UnityEngine;

namespace Thetraveler
{
    //用于创建Remix菜单的类 菜单实例加载在Plugin.cs中
    public class OptionsMenu : OptionInterface
    {
        //字段-按键设置按钮
        public readonly Configurable<KeyCode> skillKeyCode;
        
        //按键按钮的默认状态
        public OptionsMenu(Plugin plugin) 
        {
            skillKeyCode = this.config.Bind<KeyCode>("TheTraveler_KeyCode_Keybind", KeyCode.LeftControl);       
        }

        //重写初始化内容，添加并且实现UI元素，添加翻译                                                                                                     
        public override void Initialize()
        {
            //翻译
            string title;
            string keybinderdescription;
            int title_x;
            int description_x;
            if ( RWCustom.Custom.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese) 
            {
                title = "自定义按键";
                title_x = 249;
                keybinderdescription = "时间膨胀&弹射";
                description_x = 250;
            }//如果游戏语言为中文
            else
            {
                title = "Customize Controls";
                title_x = 207;
                keybinderdescription = "Time Dilation & Leap Skill";
                description_x = 230;
            }

            //把标签页加入标签页列表 只有一页的话真的有用吗（？
            var SettingsTab = new OpTab(this, "Settings");
            this.Tabs = new[] { SettingsTab }; 

            //以下为SettingsTab标签页的内容
            OpContainer SettingsTabContainer = new OpContainer(new Vector2(0, 0));
            SettingsTab.AddItems(SettingsTabContainer);

            //生成包含多个UI元素的序列
            UIelement[] UIArrayElements = new UIelement[] 
            {
                new OpRect(new Vector2(10,470),new Vector2(580,120), 0.3f){doesBump = true, description = "OwO"},//一个装饰框
                new OpKeyBinder(skillKeyCode, new Vector2(240,510),new Vector2(120,30)),//一个按键设置按钮
                new OpLabel(title_x,550,title,true),//标题文字
                new OpLabel(description_x,480,keybinderdescription),//描述文字
            };

            SettingsTab.AddItems(UIArrayElements);//将列表载入标签页
        }
        
    }
}
