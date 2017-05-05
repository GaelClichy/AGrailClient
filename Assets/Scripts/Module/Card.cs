﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AGrail
{
    public class Card
    {
        public uint ID { get; private set; }
        public CardType Type { get; private set; }
        public CardElement Element { get; private set; }
        public CardProperty Property { get; private set; }
        public CardName Name { get; private set; }
        public string AssetPath { get; private set; }
        public string Description { get; private set; }
        public int SkillNum { get; private set; }
        public List<string> SkillNames = new List<string>();

        private static List<string[]> cardStr = new List<string[]>();
        static Card()
        {
            var txt = (Resources.Load<TextAsset>("cardDB")).text;            
            var strs = txt.Split('\n');
            foreach (var v in strs)
            {
                var s = v.Trim(" \t\r\n".ToCharArray());
                cardStr.Add(s.Split('\t'));
            }                
        }

        public Card(uint cardID)
        {
            var t = cardStr[(int)cardID];
            ID = cardID;
            Type = (CardType)Enum.Parse(typeof(CardType), t[1], true);
            Element = (CardElement)Enum.Parse(typeof(CardElement), t[2], true);
            Property = (CardProperty)Enum.Parse(typeof(CardProperty), t[3], true);
            Name = (CardName)Enum.Parse(typeof(CardName), t[4], true);
            AssetPath = t[5];
            Description = t[6];
            SkillNum = int.Parse(t[7]);
            for (int i = 0; i < SkillNum; i++)
                SkillNames.Add(t[8 + i]);            
        }

        public bool HasSkill(string skillName)
        {
            foreach(var v in SkillNames)
            {
                if (v == skillName)
                    return true;
            }
            return false;
        }

        public bool HasSkill(uint skillID)
        {
            return false;
        }

        public enum CardType
        {
            magic,
            attack
        }

        public enum CardElement
        {
            earth,
            wind,
            light,
            fire,
            thunder,
            water,
            darkness,
        }

        public enum CardProperty
        {
            幻, 圣, 咏, 技, 血
        }

        public enum CardName
        {
            圣盾,
            虚弱,
            中毒,
            魔弹,
            地裂斩,
            风神斩,
            火焰斩,
            水涟斩,
            雷光斩,
            圣光,
            暗灭,            
        }




    }



}


