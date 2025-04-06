using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StarVally
{
    [BepInPlugin("liuheniw.csti.plugin.StarVally", "星露谷核心机制修改模组", "0.0.1")]
    public class StarVallyApplication : BaseUnityPlugin
    {

        /**
         * 代码执行顺序：
         * 进入存档：
         *      --GraphicsManager.CreateStatusGraphics
         *      --GameManager.LoadCard
         *      --GraphicsManager.CreateStatusGraphics
         *      --GameManager.LoadCard
         *      --GameManager.Awake
         *      --GameManager.LateUpdate
         * 重置今日：
         *      --GraphicsManager.CreateStatusGraphics
         *      --GameManager.LoadCard
         *      --GraphicsManager.CreateStatusGraphics
         *      --GameManager.LoadCard
         *      --GameManager.Awake
         *      --GameManager.LateUpdate
         * 退出存档：
         *      --GameManager.quitGame
         *      --GameManager.LateUpdate
         */

        public static StarVallyTools starVallyTools = new StarVallyTools();
        public static SpecialActionSet OldTimeOptions;
        public static List<InGameStat> oldInGameStats = new List<InGameStat>();

        public static string myPerk = "初始星露谷";
        public static int oldGrandFatherCardInteractionsLength = 0;
        public static bool hasModifyCardsAndStatus = false;
        public static bool hasMyPerk = false;
        //public static bool wantSetDayTime = false;
        //存储自定义状态列表
        public static List<string> starVallyStats = new List<string>
        {
            "季节",
            "求婚",
            "生命",
            "能量",
            "雨水"
        };

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(StarVallyApplication));
            Debug.Log("游戏修改模组已加载");
        }

        void Start()
        {
            //启动方法，可以放热键配置
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameManager), "Awake")]
        public static void GameManagerAwakePostfix()
        {
            GameManager gm = StarVallyTools.gameManager;
            if (gm != null)
            {
                //有指定特质才对游戏进行修改，主要涉及游戏基础机制类型与原版物品
                if (hasMyPerk)
                {
                    if (!StarVallyTools.hasModified)
                    {
                        starVallyTools.LoadWeatherEffectFromAB("StarVally.springwindeffect", "SpringWindEffect", "星露谷_天气_春季_微风");
                        starVallyTools.LoadWeatherEffectFromAB("StarVally.fallwindeffect", "FallWindEffect", "星露谷_天气_秋季_微风");
                        StarVallyTools.hasModified = true;
                    }


                    ModifyGrandFather();

                }
                else
                {
                    if (oldGrandFatherCardInteractionsLength != 0)
                    {
                        RevertGrandFather();
                    }
                }
            }
            else
            {
                Debug.LogError($"gm为空");
            }
        }

        void Update()
        {
            //检测更新方法，可以监听热键按下并进行处理
            //if (Input.GetKeyDown(KeyCode.Alpha9))
            //{
            //    wantSetDayTime = !wantSetDayTime;
            //    Debug.LogError($"将是否修改时间设置为{wantSetDayTime}");
            //}
        }

        //游戏加载时修改卡牌数据
        [HarmonyPostfix, HarmonyPatch(typeof(GameManager), "LoadCard")]
        public static void ModifyCardsAndStatus()
        {
            if (hasMyPerk && !hasModifyCardsAndStatus)
            {
                GameManager gm = StarVallyTools.gameManager;

                //修改模组物品
                if (StarVallyTools.cardDataDict.TryGetValue("星露谷_物品_单人床", out var singleBed))
                {
                    Debug.Log($"取得星露谷_物品_单人床卡牌");
                    DismantleCardAction action1 = StarVallyTools.BuildAction("打盹", "星露谷_状态栏_能量", 5.625f, 5.625f, true, "星露谷_状态栏_生命", 2.083f, 2.083f, true, CardModifications.DurabilityChanges, 16, FadeToBlackTypes.Full, "打盹中...", true);
                    DismantleCardAction action2 = StarVallyTools.BuildAction("睡觉", "星露谷_状态栏_能量", 5.625f, 5.625f, true, "星露谷_状态栏_生命", 2.083f, 2.083f, true, CardModifications.DurabilityChanges, 32, FadeToBlackTypes.Full, "睡觉中...", false);
                    singleBed.DismantleActions = new List<DismantleCardAction> { action1, action2 };
                }

                //修改状态
                List<InGameStat> starVallyInGameStat = new List<InGameStat>();
                foreach (var stat in gm.AllStats)
                {
                    if (starVallyStats.Contains(stat.StatModel.GameName))
                    {
                        starVallyInGameStat.Add(stat);
                    }
                }
                gm.AllStats = starVallyInGameStat;

                //设置修改状态，避免反复加载重复修改
                hasModifyCardsAndStatus = true;
            }
        }

        //修改祖父的交互，为祖父新增剥咖啡豆及用金属废料造枪的交互动作
        public static void ModifyGrandFather()
        {
            if (StarVallyTools.cardDataDict.TryGetValue("Grandfather", out var p))
            {
                if (oldGrandFatherCardInteractionsLength == 0)
                {
                    oldGrandFatherCardInteractionsLength = p.CardInteractions.Length;
                }

                {
                    if (StarVallyTools.cardDataDict.TryGetValue("CoffeeBerries", out var a) && StarVallyTools.cardDataDict.TryGetValue("CoffeeBeans", out var b) && StarVallyTools.cardDataDict.TryGetValue("CoffeeBerryPulp", out var c))
                    {
                        StarVallyTools.AddCardOnCardAction(p, a, b, 1, "剥咖啡豆", 0, CardModifications.Transform, c);
                    }
                }

                {
                    if (StarVallyTools.cardDataDict.TryGetValue("MetalScrap", out var a) && StarVallyTools.cardDataDict.TryGetValue("Gun", out var b))
                    {
                        StarVallyTools.AddCardOnCardAction(p, a, b, 1, "造枪", 1);
                    }
                }
            }
        }

        public static void RevertGrandFather()
        {
            if (StarVallyTools.cardDataDict.TryGetValue("Grandfather", out var p))
            {
                Array.Resize(ref p.CardInteractions,oldGrandFatherCardInteractionsLength);
            }
        }

        //拦截主界面左侧加载的状态栏
        [HarmonyPrefix,HarmonyPatch(typeof(GraphicsManager), "CreateStatusGraphics")]
        public static bool GraphicsManagerCreateStatusGraphicsPrefix(StatStatus _Status, bool _Ascending)
        {
            Init();

            if ((hasMyPerk && !starVallyStats.Contains(_Status.GameName)) || (!hasMyPerk && starVallyStats.Contains(_Status.GameName)))
            {
                return false;
            }
            return true;
        }

        //拦截卡牌点击方法
        [HarmonyPrefix, HarmonyPatch(typeof(InGameCardBase), "OnPointerClick")]
        public static void InGameCardBaseOnPointerClickPrefix(InGameCardBase __instance, PointerEventData _Pointer)
        {
            if (__instance.CardModel.CardName.DefaultText.Equals("单人床"))
            {
                GameManager gm = StarVallyTools.gameManager;
                int day = gm.CurrentTickInfo.x;
                int tick = gm.CurrentTickInfo.y;
                int totalTick = gm.CurrentTickInfo.z;
                int miniTick = gm.CurrentMiniTicks;
                Debug.LogError($"当前天数为：{day}");
                Debug.LogError($"当前时刻为：{tick}");
                Debug.LogError($"当前总时刻为：{totalTick}");
                Debug.LogError($"当前mini时间为：{miniTick}");

                //重置时间至最近的tick（向下取整）
                //这么做是为了清除那些只用了3分钟的动作
                //gm.SetTimeTo(day,tick);
                Traverse.Create(gm).Property("CurrentMiniTicks").SetValue(0);
                Debug.LogError($"当前mini时间为：{miniTick}");

                
                    
                CardData singleBed = __instance.CardModel;
                    Debug.Log($"准备动手修改单人床");
                    DismantleCardAction action1 = singleBed.DismantleActions[0];
                    DismantleCardAction action2 = singleBed.DismantleActions[1];


                    if (tick > 79)
                    {
                        //action1.AlwaysShow = false;
                    }else
                    {
                        //action1.AlwaysShow = true;
                        Traverse.Create(action1).Field("DaytimeCost").SetValue(80 - tick);
                    }
                    Traverse.Create(action2).Field("DaytimeCost").SetValue(104 - tick);

                    singleBed.DismantleActions = new List<DismantleCardAction> {action1, action2 };
                singleBed.s

                //if (wantSetDayTime)
                //{
                //    Debug.LogError($"时间跃迁至次日06:00");
                //    gm.SetTimeTo(gm.CurrentTickInfo.x + 1, 8);
                //}

                //修改单人床睡眠时间
                //if (StarVallyTools.cardDataDict.TryGetValue("星露谷_物品_单人床", out var singleBed))
                //{
                //    Debug.Log($"取得星露谷_物品_单人床卡牌");
                //    DismantleCardAction action1 = StarVallyTools.BuildAction("打盹", "星露谷_状态栏_能量", 5.625f, 5.625f, true, "星露谷_状态栏_生命", 2.083f, 2.083f, true, CardModifications.DurabilityChanges, 16, FadeToBlackTypes.Full, "打盹中...", true);
                //    DismantleCardAction action2 = StarVallyTools.BuildAction("睡觉", "星露谷_状态栏_能量", 5.625f, 5.625f, true, "星露谷_状态栏_生命", 2.083f, 2.083f, true, CardModifications.DurabilityChanges, 32, FadeToBlackTypes.Full, "睡觉中...", false);
                //    singleBed.DismantleActions = new List<DismantleCardAction> { action1, action2 };
                //}
            }
        }

        public static void Init()
        {
            hasMyPerk = false;
            hasModifyCardsAndStatus = false;

            if (!hasMyPerk)
            {
                //初始化工具类
                starVallyTools.InitAnything();

                foreach (var characterPerk in GameManager.CurrentPlayerCharacter.CharacterPerks)
                {
                    if (characterPerk.PerkName.DefaultText.Equals(myPerk))
                    {
                        hasMyPerk = true;
                        break;
                    }
                }
            }

        }
    }
}