using UnityEngine;
using System.Collections;
using System;
using network;
using System.Collections.Generic;

namespace AGrail
{
    public class ShengNv : RoleBase
    {
        public override RoleID RoleID
        {
            get
            {
                return RoleID.ShengNv;
            }
        }

        public override string RoleName
        {
            get
            {
                return "圣女";
            }
        }

        public override Card.CardProperty RoleProperty
        {
            get
            {
                return Card.CardProperty.圣;
            }
        }

        public override string Knelt
        {
            get
            {
                return "LianMin";
            }
        }

        public ShengNv()
        {
            for (uint i = 601; i <= 605; i++)
                Skills.Add(i, Skill.GetSkill(i));
        }

        public override bool CanSelect(uint uiState, Card card)
        {
            switch (uiState)
            {
                case 601:
                case 602:
                    return card.HasSkill(uiState);
            }
            return base.CanSelect(uiState, card);
        }

        public override bool CanSelect(uint uiState, SinglePlayerInfo player)
        {
            switch (uiState)
            {
                case 601:
                case 602:
                case 603:
                case 605:
                    return true;
            }
            return base.CanSelect(uiState, player);
        }

        public override bool CanSelect(uint uiState, Skill skill)
        {
            switch (uiState)
            {
                case 601:
                case 602:
                case 605:
                    return true;
                case 10:
                    if(skill.SkillID == 601 || skill.SkillID == 602)
                        return true;
                    if (skill.SkillID == 605 && BattleData.Instance.MainPlayer.gem + BattleData.Instance.MainPlayer.crystal >= 1)
                        return true;
                    return false;
                case 11:
                    if (skill.SkillID == 601 || skill.SkillID == 602)
                        return true;
                    if (skill.SkillID == 605 && additionalState != 6054 && 
                        BattleData.Instance.MainPlayer.gem + BattleData.Instance.MainPlayer.crystal >= 1)
                        return true;                    
                    return false;
            }
            return base.CanSelect(uiState, skill);
        }

        public override uint MaxSelectPlayer(uint uiState)
        {
            switch (uiState)
            {
                case 601:
                case 603:
                case 605:
                    return 1;
                case 602:
                    return 3;
            }
            return base.MaxSelectPlayer(uiState);
        }

        public override uint MaxSelectCard(uint uiState)
        {
            switch (uiState)
            {
                case 601:
                case 602:                
                    return 1;
            }
            return base.MaxSelectCard(uiState);
        }

        public override bool CheckOK(uint uiState, List<uint> cardIDs, List<uint> playerIDs, uint? skillID)
        {
            switch (uiState)
            {
                case 604:
                    return true;
                case 603:
                case 605:
                    if (playerIDs.Count == 1)
                        return true;
                    return false;
                case 601:
                    if (playerIDs.Count == 1 && cardIDs.Count == 1)
                        return true;
                    return false;
                case 602:
                    if (playerIDs.Count >= 1 && cardIDs.Count == 1)
                        return true;
                    return false;
            }
            return base.CheckOK(uiState, cardIDs, playerIDs, skillID);
        }

        public override bool CheckCancel(uint uiState, List<uint> cardIDs, List<uint> playerIDs, uint? skillID)
        {
            switch (uiState)
            {
                case 601:
                case 602:
                case 604:
                case 605:
                    return true;
            }
            return base.CheckCancel(uiState, cardIDs, playerIDs, skillID);
        }

        public override void AdditionAction()
        {
            if (BattleData.Instance.Agent.SelectArgs.Count == 1 && BattleData.Instance.Agent.SelectArgs[0] == 605)
            {
                sendReponseMsg(606, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 1 });
                additionalState = 6054;
                return;
            }
            base.AdditionAction();
        }

        /// <summary>
        /// 暂时保存圣疗过程中的所有人选择
        /// </summary>
        private List<uint> selectPlayers = new List<uint>();
        public override void UIStateChange(uint state, UIStateMsg msg, params object[] paras)
        {
            if (state != 605 && !(additionalState == 6054 && state != 0))            
                additionalState = 0;

            switch (state)
            {
                case 601:
                case 602:
                    OKAction = () =>
                    {
                        sendActionMsg(BasicActionType.ACTION_MAGIC_SKILL, BattleData.Instance.MainPlayer.id,
                            BattleData.Instance.Agent.SelectPlayers, BattleData.Instance.Agent.SelectCards, state,
                            BattleData.Instance.Agent.SelectArgs);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    CancelAction = () => { BattleData.Instance.Agent.FSM.BackState(UIStateMsg.Init); };
                    return;
                case 603:
                    OKAction = () => { sendReponseMsg(state, BattleData.Instance.MainPlayer.id, 
                        BattleData.Instance.Agent.SelectPlayers, null, new List<uint>() { 1 }); };
                    return;
                case 604:
                    OKAction = () =>
                    {
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 1 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    CancelAction = () =>
                    {
                        sendReponseMsg(state, BattleData.Instance.MainPlayer.id, null, null, new List<uint>() { 0 });
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    return;
                case 605:
                    OKAction = () =>
                    {
                        if (additionalState == 0)
                        {
                            additionalState = 6051;
                            selectPlayers.Clear();
                            selectPlayers.Add(BattleData.Instance.Agent.SelectPlayers[0]);
                            BattleData.Instance.Agent.RemoveSelectPlayer(BattleData.Instance.Agent.SelectPlayers[0]);
                        }
                        else if(additionalState >= 6051 && additionalState <= 6052)
                        {
                            additionalState++;
                            selectPlayers.Add(BattleData.Instance.Agent.SelectPlayers[0]);
                            BattleData.Instance.Agent.RemoveSelectPlayer(BattleData.Instance.Agent.SelectPlayers[0]);
                        }
                        else if(additionalState == 6053)
                        {
                            additionalState++;
                            selectPlayers.Add(BattleData.Instance.Agent.SelectPlayers[0]);
                            BattleData.Instance.Agent.RemoveSelectPlayer(BattleData.Instance.Agent.SelectPlayers[0]);
                            BattleData.Instance.Agent.SelectArgs.Clear();                            
                            foreach (var v in selectPlayers)
                            {
                                var idx = BattleData.Instance.Agent.SelectPlayers.FindIndex((t) => { return t == v; });
                                if (idx >= 0)
                                    BattleData.Instance.Agent.SelectArgs[idx]++;
                                else
                                {
                                    BattleData.Instance.Agent.SelectPlayers.Add(v);
                                    BattleData.Instance.Agent.SelectArgs.Add(1);
                                }
                                sendActionMsg(BasicActionType.ACTION_MAGIC_SKILL, BattleData.Instance.MainPlayer.id,
                                    BattleData.Instance.Agent.SelectPlayers, null, 605, BattleData.Instance.Agent.SelectArgs);
                            }
                        }                        
                    };
                    CancelAction = () => { BattleData.Instance.Agent.FSM.BackState(UIStateMsg.Init); };
                    return;
            }
            base.UIStateChange(state, msg, paras);
        }
    }
}
