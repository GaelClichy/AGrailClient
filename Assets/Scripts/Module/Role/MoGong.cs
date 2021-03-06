using System.Collections.Generic;
using network;
using UnityEngine;
using Framework.Message;
using System;

namespace AGrail
{
    //写的特别闹心...许多地方其实没处理好
    //先这么凑合吧
    public class MoGong : RoleBase
    {
        public override RoleID RoleID
        {
            get
            {
                return RoleID.MoGong;
            }
        }

        public override string RoleName
        {
            get
            {
                return "魔弓";
            }
        }

        public override Card.CardProperty RoleProperty
        {
            get
            {
                return Card.CardProperty.幻;
            }
        }

        public override bool HasCoverd
        {
            get
            {
                return true;
            }
        }
        
        public override bool IsStart
        {
            get
            {
                return base.IsStart;
            }

            set
            {
                if (!value)
                {
                    additionalState = 0;
                    chongNengCnt = 0;
                    isChongNengUsed = false;
                }
                base.IsStart = value;
            }
        }

        public override string HeroName
        {
            get
            {
                return "泰罗莎";
            }
        }

        public MoGong()
        {
            for (uint i = 2601; i <= 2605; i++)
                Skills.Add(i, Skill.GetSkill(i));
        }

        public override bool CanSelect(uint uiState, Card card, bool isCovered)
        {
            switch (uiState)
            {
                case (uint)SkillID.魔贯冲击:
                case (uint)SkillID.魔贯冲击命中:
                    return card.Element == Card.CardElement.fire && isCovered;
                case (uint)SkillID.雷光散射:
                    return card.Element == Card.CardElement.thunder && isCovered;               
                case (uint)SkillID.充能:
                    return !isCovered;
                case (uint)SkillID.充能盖牌:
                case (uint)SkillID.魔眼盖牌:
                    return !isCovered;
                case 11:
                case 1:
                    if (additionalState == 26031)
                        return card.Element == Card.CardElement.wind && isCovered;
                    break;
            }
            return base.CanSelect(uiState, card, isCovered);
        }

        public override bool CanSelect(uint uiState, SinglePlayerInfo player)
        {
            switch (uiState)
            {
                case (uint)SkillID.魔眼:
                    return true;
                case (uint)SkillID.雷光散射:
                    return BattleData.Instance.Agent.SelectCards.Count > 1 && player.team != BattleData.Instance.MainPlayer.team;
            }
            return base.CanSelect(uiState, player);
        }

        public override bool CanSelect(uint uiState, Skill skill)
        {
            switch (uiState)
            {
                case 10:
                case 11:
                case (uint)SkillID.雷光散射:
                    if (skill.SkillID == (uint)SkillID.雷光散射 && !isChongNengUsed)
                        return Util.HasCard(Card.CardElement.thunder, BattleData.Instance.MainPlayer.covereds);
                    return false;
            }
            return base.CanSelect(uiState, skill);
        }

        public override uint MaxSelectCard(uint uiState)
        {
            switch (uiState)
            {
                case (uint)SkillID.雷光散射:
                    return BattleData.Instance.MainPlayer.covered_count;
                case (uint)SkillID.魔贯冲击:
                case (uint)SkillID.魔贯冲击命中:
                case (uint)SkillID.魔眼盖牌:
                    return 1;
                case (uint)SkillID.充能:
                    return (uint)Mathf.Max(0, (int)BattleData.Instance.MainPlayer.hand_count - 4);
                case (uint)SkillID.充能盖牌:
                    return chongNengCnt;
            }
            return base.MaxSelectCard(uiState);
        }

        public override uint MaxSelectPlayer(uint uiState)
        {
            switch (uiState)
            {
                case (uint)SkillID.雷光散射:
                case (uint)SkillID.魔眼:
                    return 1;
            }
            return base.MaxSelectPlayer(uiState);
        }        

        public override void AdditionAction()
        {
            if (BattleData.Instance.Agent.SelectArgs.Count == 1)
            {
                switch (BattleData.Instance.Agent.SelectArgs[0])
                {
                    case (uint)SkillID.多重射击:
                        additionalState = 26031;
                        break;
                }
            }
            base.AdditionAction();
        }

        public override bool CheckOK(uint uiState, List<uint> cardIDs, List<uint> playerIDs, uint? skillID)
        {
            switch (uiState)
            {
                case (uint)SkillID.魔眼:
                    return true;
                case (uint)SkillID.魔眼盖牌:
                case (uint)SkillID.魔贯冲击:
                case (uint)SkillID.魔贯冲击命中:
                    return cardIDs.Count == 1;
                case (uint)SkillID.充能盖牌:
                    return cardIDs.Count <= chongNengCnt;
                case (uint)SkillID.充能魔眼:
                    return true;
                case (uint)SkillID.雷光散射:
                    return cardIDs.Count == 1 || (cardIDs.Count > 1 && playerIDs.Count == 1);
				//case (uint)SkillID.充能:
					//return (BattleData.Instance.MainPlayer.hand_count < 4 || BattleData.Instance.MainPlayer.hand_count - cardIDs.Count == 4);
            }
            return base.CheckOK(uiState, cardIDs, playerIDs, skillID);
        }

        public override bool CheckCancel(uint uiState, List<uint> cardIDs, List<uint> playerIDs, uint? skillID)
        {
            switch (uiState)
            {
                case (uint)SkillID.充能:
                case (uint)SkillID.充能盖牌:
                case (uint)SkillID.魔眼:
                case (uint)SkillID.魔眼盖牌:
                case (uint)SkillID.充能魔眼:
                case (uint)SkillID.雷光散射:
                case (uint)SkillID.魔贯冲击:
                case (uint)SkillID.魔贯冲击命中:
				case (uint)SkillID.多重射击:
                    return true;
            }
            return base.CheckCancel(uiState, cardIDs, playerIDs, skillID);
        }

        public override void UIStateChange(uint state, UIStateMsg msg, params object[] paras)
        {
            var selectList = new List<List<uint>>();
            var mList = new List<string>();
            switch (state)
            {
                case 1:
                    if (BattleData.Instance.Agent.SelectPlayers.Count == 1 && BattleData.Instance.Agent.SelectCards.Count == 1)
                    {
                        if (additionalState == 26031)
                        {
                            sendActionMsg(BasicActionType.ACTION_ATTACK_SKILL, BattleData.Instance.MainPlayer.id,
                                BattleData.Instance.Agent.SelectPlayers, BattleData.Instance.Agent.SelectCards, (uint)SkillID.多重射击);
                            MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.AgentHandChange, false);
                        }                            
                        else
                            Attack(BattleData.Instance.Agent.SelectCards[0], BattleData.Instance.Agent.SelectPlayers[0]);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                        return;
                    }
                    if(additionalState == 26031)
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.AgentHandChange, true);
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        "请选择目标");
                    return;
			case (uint)SkillID.充能:
				MessageSystem<Framework.Message.MessageType>.Notify (Framework.Message.MessageType.CloseNewArgsUI);
				if(msg == UIStateMsg.ClickArgs)
					if ((BattleData.Instance.MainPlayer.hand_count < 4 || BattleData.Instance.MainPlayer.hand_count - BattleData.Instance.Agent.SelectCards.Count == 4))
					{
						IsStart = true;
						isChongNengUsed = true;
						chongNengCnt = BattleData.Instance.Agent.SelectArgs [0];
						sendReponseMsg (state, BattleData.Instance.MainPlayer.id, null,
							BattleData.Instance.Agent.SelectCards, new List<uint> () {
							1,
							(uint)BattleData.Instance.Agent.SelectCards.Count,
							BattleData.Instance.Agent.SelectArgs [0]
						});
						BattleData.Instance.Agent.FSM.ChangeState<StateIdle> (UIStateMsg.Init, true);
						return;
					}
                    CancelAction = () =>
                    {
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseNewArgsUI);
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 0 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
					if (BattleData.Instance.MainPlayer.hand_count > 4)
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        string.Format("{0}: 弃到4张牌,并选择之后摸牌的张数", Skills[state].SkillName));
					else
					MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
						string.Format("{0}: 选择摸牌的张数", Skills[state].SkillName));

                        selectList.Clear();
                        mList.Clear();
                        for (uint i = 1; i < 5; i++)
                        {
                            selectList.Add(new List<uint>() { i });
                            mList.Add("摸" + i.ToString() + "张牌");
                        }
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.ShowNewArgsUI, selectList, mList);            
                    return;
                case (uint)SkillID.充能盖牌:
                    OKAction = () => 
                    {
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null,
                            BattleData.Instance.Agent.SelectCards, new List<uint>() { 1 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    CancelAction = () =>
                    {
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 0 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        string.Format("充能: 请选择要作为充能的盖牌"));
                    return;
                case (uint)SkillID.魔眼:
                    if(BattleData.Instance.Agent.SelectPlayers.Count == 1)
                    {
                        IsStart = true;
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, BattleData.Instance.Agent.SelectPlayers,
                            null, new List<uint>() { 1, 1 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                        return;
                    }
                    else
                    {
                        OKAction = () =>
                        {
                            IsStart = true;
                            sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 1, 0 });
                            BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                        };
                    }
                    CancelAction = () =>
                    {
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 0 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        string.Format("{0}: 选择玩家弃牌；不选择视为摸3张", Skills[state].SkillName));
                    return;
                case (uint)SkillID.魔眼盖牌:
                    if(BattleData.Instance.Agent.SelectCards.Count == 1)
                    {
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, 
                            BattleData.Instance.Agent.SelectCards, new List<uint>() { 1 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                        return;
                    }
                    CancelAction = () =>
                    {
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 0 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        string.Format("魔眼: 选择1张手牌作为能量"));
                    return;
                case (uint)SkillID.充能魔眼:
                    if(msg == UIStateMsg.ClickArgs)
                    {
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseNewArgsUI);
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, BattleData.Instance.Agent.SelectArgs);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                        return;
                    }
                    CancelAction = () =>
                    {
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseNewArgsUI);
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 0 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseNewArgsUI);
                    selectList.Clear();
                    mList.Clear();
                    if(BattleData.Instance.MainPlayer.crystal + BattleData.Instance.MainPlayer.gem > 0)
                    {
                        selectList.Add(new List<uint>() { 1 });
                        mList.Add("充能");
                    }
                    if(BattleData.Instance.MainPlayer.gem > 0)
                    {
                        selectList.Add(new List<uint>() { 2 });
                        mList.Add("魔眼");
                    }
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.ShowNewArgsUI, selectList, mList);
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        string.Format("选择要发动的技能"));
                    return;
                case (uint)SkillID.雷光散射:
                    OKAction = () =>
                    {
                        sendActionMsg(BasicActionType.ACTION_MAGIC_SKILL, BattleData.Instance.MainPlayer.id,
                            BattleData.Instance.Agent.SelectPlayers, BattleData.Instance.Agent.SelectCards, state);
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.AgentHandChange, false);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    CancelAction = () => {
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.AgentHandChange, false);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.AgentHandChange, true);
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        string.Format("{0}: 选择雷系充能。大于1张时再选择一名敌方玩家", Skills[state].SkillName));
                    return;
                case (uint)SkillID.魔贯冲击:
                case (uint)SkillID.魔贯冲击命中:
                    if(BattleData.Instance.Agent.SelectCards.Count == 1)
                    {
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, 
                            BattleData.Instance.Agent.SelectCards, new List<uint>() { 1 });
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.AgentHandChange, false);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                        return;
                    }
                    CancelAction = () =>
                    {
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 0 });
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.AgentHandChange, false);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.AgentHandChange, true);
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        string.Format("魔贯冲击: 选择火系充能"));
                    return;
            }
            base.UIStateChange(state, msg, paras);
        }
     
        private bool isChongNengUsed = false;
        private uint chongNengCnt = 0;
        private enum SkillID
        {
            魔贯冲击 = 2601,
            雷光散射,
            多重射击,
            充能,
            魔眼,
            充能魔眼,
            魔贯冲击命中 = 26011,
            充能盖牌 = 26041,
            魔眼盖牌 = 26051,
        }
    }
}


