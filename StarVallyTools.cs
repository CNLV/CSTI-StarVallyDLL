using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace StarVally
{
    public class StarVallyTools
    {
        public static Dictionary<string, BookmarkGroup> bookmarkGroupDict = new Dictionary<string, BookmarkGroup>();
        public static Dictionary<string, CardData> cardDataDict = new Dictionary<string, CardData>();
        public static Dictionary<string, CharacterPerk> characterPerkDict = new Dictionary<string, CharacterPerk>();
        public static Dictionary<string, Encounter> encounterDict = new Dictionary<string, Encounter>();
        public static Dictionary<string, EndgameLogCategory> endgameLogCategoryDict = new Dictionary<string, EndgameLogCategory>();
        public static Dictionary<string, Gamemode> gamemodeDict = new Dictionary<string, Gamemode>();
        public static Dictionary<string, GameModifierPackage> gameModifierPackageDict = new Dictionary<string, GameModifierPackage>();
        public static Dictionary<string, GameStat> gameStatDict = new Dictionary<string, GameStat>();
        public static Dictionary<string, LocalTickCounter> localTickCounterDict = new Dictionary<string, LocalTickCounter>();
        public static Dictionary<string, Objective> objectiveDict = new Dictionary<string, Objective>();
        public static Dictionary<string, PerkGroup> perkGroupDict = new Dictionary<string, PerkGroup>();
        public static Dictionary<string, PerkTabGroup> perkTabGroupDict = new Dictionary<string, PerkTabGroup>();
        public static Dictionary<string, PlayerCharacter> playerCharacterDict = new Dictionary<string, PlayerCharacter>();
        public static Dictionary<string, SelfTriggeredAction> selfTriggeredActionDict = new Dictionary<string, SelfTriggeredAction>();

        // GM实例
        public static GameManager gameManager;
        public static bool hasModified = false;


        //初始化
        public void InitAnything()
        {
            gameManager = MBSingleton<GameManager>.Instance;
            if (cardDataDict.Count == 0)
            {
                Debug.Log($"===StarVallyTools开始初始化===");
                InitCardDict();
            }
        }

        public void InitCardDict()
        {
            foreach (var data in GameLoad.Instance.DataBase.AllData)
            {
                switch (data)
                {
                    case BookmarkGroup bookmarkGroup:
                        bookmarkGroupDict.Add(bookmarkGroup.name, bookmarkGroup);
                        break;
                    case CardData cardData:
                        cardDataDict.Add(cardData.name, cardData);
                        break;
                    case CharacterPerk characterPerk:
                        characterPerkDict.Add(characterPerk.name, characterPerk);
                        break;
                    case Encounter encounter:
                        encounterDict.Add(encounter.name, encounter);
                        break;
                    case EndgameLogCategory endgameLogCategory:
                        endgameLogCategoryDict.Add(endgameLogCategory.name, endgameLogCategory);
                        break;
                    case Gamemode gamemode:
                        gamemodeDict.Add(gamemode.name, gamemode);
                        break;
                    case GameModifierPackage gameModifierPackage:
                        gameModifierPackageDict.Add(gameModifierPackage.name, gameModifierPackage);
                        break;
                    case GameStat gameStat:
                        gameStatDict.Add(gameStat.name, gameStat);
                        break;
                    case LocalTickCounter localTickCounter:
                        localTickCounterDict.Add(localTickCounter.name, localTickCounter);
                        break;
                    case Objective objective:
                        objectiveDict.Add(objective.name, objective);
                        break;
                    case PerkGroup perkGroup:
                        perkGroupDict.Add(perkGroup.name, perkGroup);
                        break;
                    case PerkTabGroup perkTabGroup:
                        perkTabGroupDict.Add(perkTabGroup.name, perkTabGroup);
                        break;
                    case PlayerCharacter playerCharacter:
                        playerCharacterDict.Add(playerCharacter.name, playerCharacter);
                        break;
                    case SelfTriggeredAction selfTriggeredAction:
                        selfTriggeredActionDict.Add(selfTriggeredAction.name, selfTriggeredAction);
                        break;
                    default:
                        Debug.LogError($"未知类型");
                        break;
                }
            }
            Debug.Log($"===卡牌词典已加载完成，{bookmarkGroupDict}总数为{bookmarkGroupDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{cardDataDict}总数为{cardDataDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{characterPerkDict}总数为{characterPerkDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{encounterDict}总数为{encounterDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{endgameLogCategoryDict}总数为{endgameLogCategoryDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{gamemodeDict}总数为{gamemodeDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{gameModifierPackageDict}总数为{gameModifierPackageDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{gameStatDict}总数为{gameStatDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{localTickCounterDict}总数为{localTickCounterDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{objectiveDict}总数为{objectiveDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{perkGroupDict}总数为{perkGroupDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{perkTabGroupDict}总数为{perkTabGroupDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{playerCharacterDict}总数为{playerCharacterDict.Count}===");
            Debug.Log($"===卡牌词典已加载完成，{selfTriggeredActionDict}总数为{selfTriggeredActionDict.Count}===");
        }

        /*
            说明：新增卡对卡交互动作（拖拽式交互结果）
            cardData = 需要增加动作的卡片
            triggerCard = 触发的卡片
            productCard = 生产的卡片
            productNumber = 生产的卡片的数量
            actionName = 动作的名字
            tpCost = 动作的时间
            type = 卡片的修改类型（默认是损毁）
            tranCard = 转换的结果（默认是null）
         */
        public static void AddCardOnCardAction(CardData cardData, CardData triggerCard, CardData productCard, int productNumber, string actionName, int tpCost, CardModifications type = CardModifications.Destroy, CardData tranCard = null)
        {
            LocalizedString text = new LocalizedString();
            text.DefaultText = actionName;

            CardDrop[] drop = new CardDrop[1];
            drop[0] = new CardDrop(productCard, new UnityEngine.Vector2Int(productNumber, productNumber));

            CardsDropCollection dropCollection = new CardsDropCollection();
            dropCollection.SetDroppedCards(drop, false);
            dropCollection.CollectionName = "掉落集合";

            CardOnCardAction cardAction = new CardOnCardAction();
            cardAction.GivenCardChanges.ModType = type;
            if (type == CardModifications.Transform && tranCard != null)
            {
                cardAction.GivenCardChanges.TransformInto = tranCard;
            }

            Array.Resize<CardData>(ref cardAction.CompatibleCards.TriggerCards, 1);
            cardAction.CompatibleCards.TriggerCards[0] = triggerCard;

            Array.Resize<CardsDropCollection>(ref cardAction.ProducedCards, 1);
            cardAction.ProducedCards[0] = dropCollection;

            cardAction.ActionName = text;

            Traverse.Create(cardAction).Field("DaytimeCost").SetValue(tpCost);

            Array.Resize<CardOnCardAction>(ref cardData.CardInteractions, cardData.CardInteractions.Length + 1);
            cardData.CardInteractions[cardData.CardInteractions.Length - 1] = cardAction;
        }

        /*
         * 从AB包加载天气特效资源并为天气卡绑定特效
         * 
         * resourcePath：AB包位置，建议嵌入代码中，变量格式为“项目名.AB包名”
         * prefabName：预制件名称，该名称为Unity中创建好的预制件的名称，用于代码从AB包中定位指定预制件
         * cardName：卡牌名称，若为原版卡牌则为其英文名，若为模组天气，则大概率为中文名，可以通过Unity Explore查看
         */
        public void LoadWeatherEffectFromAB(string resourcePath, string prefabName, string cardName)
        {
            GameObject effectPrefab = null;
            GameObject effectObj = null;
            ParticleSystem particleSystem = null;

            Assembly assembly = Assembly.GetExecutingAssembly();
            var effectStream = assembly.GetManifestResourceStream(resourcePath);
            if (effectStream == null)
            {
                Debug.LogError($"未能获取到资源流，资源路径：{resourcePath}");
                return;
            }
            var effectAB = AssetBundle.LoadFromStream(effectStream);
            if (effectAB != null)
            {
                effectPrefab = effectAB.LoadAsset<GameObject>(prefabName);
                if (effectPrefab != null)
                {
                    effectObj = GameObject.Instantiate(effectPrefab);
                    if (effectObj == null)
                    {
                        Debug.LogError($"实例化预制体 {prefabName} 失败");
                        return;
                    }

                    effectObj.AddComponent<WeatherSpecialEffect>();
                    if (cardDataDict.TryGetValue(cardName, out var weatherPre))
                    {
                        if (weatherPre != null && weatherPre.WeatherEffects != null)
                        {
                            foreach (var item in weatherPre.WeatherEffects.EffectsToSpawn)
                            {
                                Debug.Log("捕获到一只野生的特效");
                                Debug.Log(item.name);
                                Debug.Log(item.tag);
                            }

                            WeatherSpecialEffect[] weatherSpecialEffects = new WeatherSpecialEffect[1];
                            WeatherSpecialEffect weatherSpecialEffect = effectObj.GetComponent<WeatherSpecialEffect>();
                            weatherSpecialEffects[0] = weatherSpecialEffect;
                            weatherPre.WeatherEffects.EffectsToSpawn = weatherSpecialEffects;
                        }
                        else
                        {
                            if(weatherPre == null)
                            {
                                Debug.LogError($"未找到名为{cardName}的卡牌数据");
                            }
                            else
                            {
                                Debug.LogError($"名为{cardName}的卡牌数据中WeatherEffects为空");

                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"卡牌词典中不存在名为 {cardName} 的卡牌");
                    }

                    particleSystem = effectObj.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        particleSystem.Stop();
                    }
                    else
                    {
                        Debug.LogError($"{prefabName}获取物理系统失败");
                    }
                }
                else
                {
                    Debug.LogError($"加载预制体{prefabName}失败");
                }
            }
            else
            {
                Debug.LogError($"加载AB包{resourcePath}失败");
            }
        }


        /**
         * 弹框数据结构示意：
         * --DismantleCardAction 点击动作
         *      --ActionName                LocalizedString     动作名
         *      --DaytimeCost               int                 消耗时间（单位：Tick）
         *      --StatModifications         StatModifier[]      状态修改列表
         *          --StatModification1      StatModifier        状态修改列表项
         *              --Stat              GameStat            状态
         *              --ValueModifier     Vector2             修改值
         *              --ApplyEachTick     bool                是否每Tick生效
         *      --ReceivingCardChanges      CardStateChange     状态修改方式
         *      --FadeToBlack               FadeToBlackTypes    遮蔽方式
         *      --FadeMessage               LocalizedString     遮蔽提示文字
         *      --cancelAble                bool                可取消
         */
        public static DismantleCardAction BuildAction(string actionName,string statName1, float valueModifierX1, float valueModifierY1, bool applyEachTick1, string statName2, float valueModifierX2, float valueModifierY2, bool applyEachTick2, CardModifications modType,int daytimeCost, FadeToBlackTypes fadeToBlack,string fadeMessage,bool cancelAble)
        {
            //ActionName
            LocalizedString ActionName = new LocalizedString();
            ActionName.DefaultText = actionName;

            //StatModifications
            StatModifier StatModification1 = new StatModifier();
            if (StarVallyTools.gameStatDict.TryGetValue(statName1, out var stat1))
            {
                Debug.Log($"取得{statName1}状态");
                StatModification1.Stat = stat1;
            }
            StatModification1.ValueModifier = new Vector2(valueModifierX1, valueModifierY1);
            StatModification1.ApplyEachTick = applyEachTick1;

            StatModifier StatModification2 = new StatModifier();
            if (StarVallyTools.gameStatDict.TryGetValue(statName2, out var stat2))
            {
                Debug.Log($"取得{statName2}状态");
                StatModification2.Stat = stat2;
            }
            StatModification2.ValueModifier = new Vector2(valueModifierX2, valueModifierY2);
            StatModification2.ApplyEachTick = applyEachTick2;

            StatModifier[] StatModifications = new StatModifier[] { StatModification1, StatModification2 };

            //ReceivingCardChanges
            CardStateChange ReceivingCardChanges = new CardStateChange();
            ReceivingCardChanges.ModType = modType;

            DismantleCardAction action = new DismantleCardAction();
            //DaytimeCost
            Traverse.Create(action).Field("DaytimeCost").SetValue(daytimeCost);

            action.ActionName = ActionName;
            action.StatModifications = StatModifications;
            action.ReceivingCardChanges = ReceivingCardChanges;

            //设置遮蔽
            action.FadeToBlack = fadeToBlack;
            LocalizedString FadeMessage = new LocalizedString();
            FadeMessage.DefaultText = fadeMessage;
            action.FadeMessage = FadeMessage;
            action.Cancellable = cancelAble;

            return action;
        }

    }
}
