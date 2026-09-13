using System;

namespace ArknightsMod.Content.SwingHelper
{
    public class StepSkill(bool isEnd)
    {
        /// <summary>
        /// 技能步骤 -- 下一步
        /// </summary>
        public StepSkill Next{get;set;}
        /// <summary>
        /// 技能步骤 -- 返回
        /// </summary>
        public StepSkill Back{get;set;}
        /// <summary>
        /// 技能实现本体 -- 返回下一个步骤选项
        /// </summary>
        public Func<StepResult> Func {get;set;}
        /// <summary>
        /// 技能是否结束 -- 回调
        /// </summary>
        public Action<bool> IsOver {get;set;}
        /// <summary>
        /// 技能选项
        /// </summary>
        public StepResult stepResult;
        /// <summary>
        /// 回调值
        /// </summary>
        public bool onDone;
        /// <summary>
        /// 是否为末尾技能
        /// </summary>
        public bool isEnd = isEnd;


        /// <summary>
        /// 末尾技能方法
        /// </summary>
        public Action EndFunc{get;set;}

        public StepSkill NextSkill(StepSkill next)
        {
            Next = next;
            return Next;
        }

        public StepSkill BackSkill(StepSkill back)
        {
            Back = back;
            return Back;
        }
    }
}

