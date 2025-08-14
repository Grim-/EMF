using Verse.AI;

namespace EMF
{
    public class DutyDef_GuardMaster : DutyDef
    {
        public DutyDef_GuardMaster()
        {
            defName = "EMF_GuardMaster";
            alwaysShowWeapon = true;
            hook = ThinkTreeDutyHook.HighPriority;
        }
    }
}