using RimWorld;
using System.Text;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_Levelable : CompProperties
    {
        public int maxLevel = 10;
        public int baseXpRequired = 100;
        public float xpRequiredMultiplier = 1.5f;

        public CompProperties_Levelable()
        {
            this.compClass = typeof(CompLevelable);
        }
    }

    public abstract class CompLevelable : ThingComp
    {
        protected int currentLevel = 1;
        protected float currentXp = 0f;

        public delegate void LevelChangedDelegate(int oldLevel, int newLevel);
        public event LevelChangedDelegate OnLevelChanged;

        public delegate void XpGainedDelegate(float amount);
        public event XpGainedDelegate OnXpGained;

        protected CompProperties_Levelable Props => (CompProperties_Levelable)props;

        public int CurrentLevel => currentLevel;
        public float CurrentXp => currentXp;

        public float XpRequiredForNextLevel
        {
            get
            {
                if (currentLevel >= Props.maxLevel)
                    return 0f;

                return Props.baseXpRequired * Mathf.Pow(Props.xpRequiredMultiplier, currentLevel - 1);
            }
        }

        public float XpProgress => currentLevel >= Props.maxLevel ? 1f : currentXp / XpRequiredForNextLevel;

        public virtual void AddXp(float amount)
        {
            if (currentLevel >= Props.maxLevel)
                return;

            currentXp += amount;
            OnXpGained?.Invoke(amount);

            Messages.Message("XpGained".Translate(parent.LabelCap, amount), MessageTypeDefOf.SilentInput);

            CheckLevelUp();
        }

        protected virtual void CheckLevelUp()
        {
            while (currentXp >= XpRequiredForNextLevel && currentLevel < Props.maxLevel)
            {
                currentXp -= XpRequiredForNextLevel;
                int oldLevel = currentLevel;
                currentLevel++;

                OnLevelUp(oldLevel, currentLevel);
                OnLevelChanged?.Invoke(oldLevel, currentLevel);

                Find.SignalManager.SendSignal(new Signal("ThingLeveledUp", parent.Named("THING"), currentLevel.Named("LEVEL")));

                Messages.Message("LevelUp".Translate(parent.LabelCap, currentLevel), MessageTypeDefOf.PositiveEvent);
            }

            if (currentLevel >= Props.maxLevel)
                currentXp = 0f;
        }

        protected abstract void OnLevelUp(int oldLevel, int newLevel);

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentLevel, "currentLevel", 1);
            Scribe_Values.Look(ref currentXp, "currentXp", 0f);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Level: {currentLevel}/{Props.maxLevel}");

            if (currentLevel < Props.maxLevel)
                sb.AppendLine($"XP: {currentXp:F0}/{XpRequiredForNextLevel:F0}");

            return sb.ToString().TrimEndNewlines();
        }
    }
}
