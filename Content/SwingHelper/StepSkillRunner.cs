namespace ArknightsMod.Content.SwingHelper
{
    public class StepSkillRunner
    {
        public StepSkillRunner(StepSkill skill)
        {
            CurrentSkill = skill;
            FristSkill = skill;
            CurrentSkill.onDone = false;
            CurrentSkill.IsOver = (r) => CurrentSkill.onDone = r;
        }
        public StepSkill CurrentSkill {get;set;}
		public StepSkill FristSkill {get;set;}
        public void Run()
        {
            if (CurrentSkill == null)
	            return;

            if (!CurrentSkill.onDone)
                CurrentSkill.stepResult = CurrentSkill.Func();
            else
                Advance();
        }
        private void Advance()
        {
            if (CurrentSkill.isEnd)
            {
                CurrentSkill.EndFunc?.Invoke();
                return;
            }
            var next = CurrentSkill.stepResult == StepResult.Next
                ? CurrentSkill.Next : CurrentSkill.Back;

            if (next != null)
            {
                next.onDone = false;
                next.IsOver = (r) => next.onDone = r;
            }
            CurrentSkill = next ?? CurrentSkill;
        }

        public StepSkillRunner ReSet() {
	        CurrentSkill = FristSkill;
	        return this;
        }

        public StepSkillRunner SkillChange(StepSkill skill) {
	        CurrentSkill = skill;
	        return this;
        }
    }
}

