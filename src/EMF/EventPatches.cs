using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EMF
{
    [StaticConstructorOnStartup]
    public static class EventPatches
    {
        //# Combat and Defense
        //- Successful melee attacks(Melee skill)
        //- Successful ranged attacks(Shooting skill)
        //- Successfully dodging attacks(Melee skill)
        //- Using shields/blocking(Melee skill)
        //- Taking damage while wearing armor(potential armor skill)

        //# Medical and Social
        //- Successful medicine application(Medicine skill)
        //- Social interactions/negotiations(Social skill)
        //- Successful recruitment attempts(Social skill)
        //- Successful arrest attempts(Social skill)
        //- Tending to wounds without medicine(Medicine skill)
        //- Mental break management(Social skill)

        //# Construction and Crafting
        //- Building completion events(Construction skill)
        //- Item crafting completion(Crafting skill)
        //- Art creation completion(Artistic skill)
        //- Quality level achievements
        //- Deconstruction activities

        //# Research and Intellectual
        //- Research completion events
        //- Scanning/analyzing artifacts
        //- Using study desks/facilities
        //- Teaching other pawns
        //- Reading/studying activities

        //# Natural and Survival
        //- Animal taming success/failure(Animals skill)
        //- Animal training progress(Animals skill)
        //- Successful hunting(Shooting/Melee skills)
        //- Plant harvesting(Plants skill)
        //- Plant sowing(Plants skill)
        //- Successful cooking(Cooking skill)
        //- Mining completion(Mining skill)


        [HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyToPawn")]
        public static class Patch_DamageWorker_AddInjury_ApplyToPawn
        {
            public static DamageWorker.DamageResult Postfix(DamageWorker.DamageResult __result, DamageInfo dinfo, Pawn pawn)
            {
                if (Current.ProgramState != ProgramState.Playing) return __result;
                //DebugDamage(__result, dinfo, pawn);
                return EventManager.Instance.RaiseDamageDealt(pawn, dinfo.Instigator, dinfo, __result);
            }
        }


        [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
        public static class Patch_Thing_TakeDamage
        {
            public static void Prefix(Thing __instance, DamageInfo dinfo)
            {
                if (__instance is Pawn pawn && !pawn.Dead)
                {
                    EventManager.Instance.RaisePawnDamageTaken(pawn, dinfo);
                }
                else
                {
                    if (!__instance.Destroyed)
                    {
                        EventManager.Instance.RaiseThingDamageTaken(__instance, dinfo);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.PreApplyDamage))]
        public static class Patch_Thing_PreApplyDamage
        {
            public static void Prefix(Thing __instance, ref DamageInfo dinfo, out bool absorbed)
            {
                absorbed = false;
                if (!__instance.Destroyed)
                {
                    absorbed = EventManager.Instance.RaiseOnBeforeThingDamageTaken(__instance, ref dinfo);
                }
            }
        }


        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult) })]
        public static class Patch_Pawn_HediffAdded
        {
            public static void Prefix(Pawn_HealthTracker __instance, Hediff hediff, Pawn ___pawn, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
            {
                if (__instance == null || hediff == null || ___pawn == null)
                {
                    return;
                }

                EventManager.Instance.RaiseOnPawnHediffGained(___pawn, dinfo, hediff);
            }
        }


        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff))]
        public static class Patch_Pawn_HediffRemoved
        {
            public static void Prefix(Pawn_HealthTracker __instance, Hediff hediff, Pawn ___pawn)
            {
                if (__instance == null || hediff == null || ___pawn == null)
                {
                    return;
                }

                EventManager.Instance.RaiseOnPawnHediffRemoved(___pawn, hediff);
            }
        }


        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        public static class Patch_Pawn_Kill
        {
            public static void Prefix(Pawn __instance, DamageInfo dinfo, Hediff exactCulprit)
            {
                EventManager.Instance.RaiseOnKilled(__instance, dinfo, exactCulprit);
            }
        }
        [HarmonyPatch(typeof(Pawn_PathFollower), "SetupMoveIntoNextCell")]
        public static class Patch_Pawn_PathFollower_SetupMoveIntoNextCell
        {
            public static void Postfix(Pawn_PathFollower __instance, ref IntVec3 ___nextCell, Pawn ___pawn)
            {
                if (___pawn != null)
                {
                    EventManager.Instance.PawnArrivedAtPathDestination(___pawn, ___nextCell);
                }
            }
        }
        [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
        public static class Patch_StartJob
        {
            public static void Prefix(Pawn_JobTracker __instance, Job newJob)
            {
                if (newJob?.def != null && __instance != null)
                {
                    Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                    if (pawn != null)
                    {
                        EventManager.Instance.RaiseJobStarted(pawn, newJob);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob))]
        public static class Patch_EndJob
        {
            public static void Prefix(Pawn_JobTracker __instance, JobCondition condition)
            {
                if (__instance?.curJob != null)
                {
                    Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                    if (pawn != null)
                    {
                        EventManager.Instance.RaiseJobEnded(pawn, __instance?.curJob, condition);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
        public static class Patch_CleanupJob
        {
            public static void Prefix(Pawn_JobTracker __instance, JobCondition condition)
            {
                if (__instance?.curJob != null)
                {
                    Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                    if (pawn != null)
                    {
                        EventManager.Instance.RaiseJobCleanedUp(pawn, __instance?.curJob, condition);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
        public static class Patch_SkillRecord_Learn
        {
            public static void Postfix(SkillRecord __instance, float xp, bool direct)
            {
                if (direct && __instance?.Pawn != null)
                {
                    EventManager.Instance.RaiseSkillGained(__instance.Pawn, __instance.def, xp);
                }
            }
        }



        [HarmonyPatch(typeof(Verb_CastAbility), "TryCastShot")]
        public static class Patch_Ability_Activate
        {
            public static void Postfix(Verb_CastAbility __instance, int ___burstShotsLeft, bool __result)
            {
                if (__result && ___burstShotsLeft == 1)
                {
                    EventManager.Instance.RaiseAbilityCompleted(__instance.Ability.pawn, __instance.Ability);
                }
            }
        }

        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("Notify_UsedVerb")]
        public static class Patch_Pawn_UsedVerb
        {
            public static void Postfix(Pawn __instance, Pawn pawn, Verb verb)
            {
                if (pawn != null && verb != null)
                {
                    EventManager.Instance.RaiseVerbUsed(pawn, verb);
                }
            }
        }

        [HarmonyPatch(typeof(Verb_MeleeAttackDamage))]
        [HarmonyPatch("DamageInfosToApply")]
        public static class Patch_Verb_MeleeAttackDamage_DamageInfosToApply
        {
            public static void Postfix(Verb_MeleeAttackDamage __instance, LocalTargetInfo target, ref IEnumerable<DamageInfo> __result)
            {
                if (__instance?.CasterPawn != null && __result != null)
                {
                    __result = ModifyDamageInfos(__instance, target, __result);
                }
            }

            private static IEnumerable<DamageInfo> ModifyDamageInfos(Verb_MeleeAttackDamage verb, LocalTargetInfo target, IEnumerable<DamageInfo> originalDamageInfos)
            {
                foreach (DamageInfo damageInfo in originalDamageInfos)
                {
                    DamageInfo modifiedDamageInfo = damageInfo;
                    EventManager.Instance.RaiseBeforeMeleeDamageInfo(verb.CasterPawn, target, ref modifiedDamageInfo);
                    yield return modifiedDamageInfo;
                }
            }
        }
    }
}
