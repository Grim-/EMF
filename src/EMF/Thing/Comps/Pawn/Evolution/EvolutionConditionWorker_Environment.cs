using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{   
    // Environmental conditions
    public class EnvironmentEvolutionProperties : EvolutionConditionProperties
    {
        public List<BiomeDef> requiredBiomes = new List<BiomeDef>();
        public float minTemperature = -100f;
        public float maxTemperature = 100f;
        public bool requiresNight = false;
        public bool requiresDay = false;
        public Season? requiredSeason = null;
    }
    public class EvolutionConditionWorker_Environment : BaseEvolutionConditionWorker
    {
        public override bool MeetsCriteria(Pawn pawn)
        {
            var envProps = props as EnvironmentEvolutionProperties;
            if (envProps == null)
                return true;

            if (envProps.requiredBiomes != null && envProps.requiredBiomes.Count > 0)
            {
                if (!envProps.requiredBiomes.Contains(pawn.Map?.Biome))
                    return false;
            }

            float temp = pawn.AmbientTemperature;
            if (temp < envProps.minTemperature || temp > envProps.maxTemperature)
                return false;

            float dayPercent = GenLocalDate.DayPercent(pawn.Map);
            if (envProps.requiresNight && (dayPercent < 0.2f || dayPercent > 0.8f))
                return false;
            if (envProps.requiresDay && (dayPercent > 0.2f && dayPercent < 0.8f))
                return false;

            if (envProps.requiredSeason.HasValue)
            {
                var currentSeason = GenLocalDate.Season(pawn.Map);
                if (currentSeason != envProps.requiredSeason.Value)
                    return false;
            }

            return true;
        }

        public override string GetFailureReason(Pawn pawn)
        {
            var envProps = props as EnvironmentEvolutionProperties;
            if (envProps == null) return "Invalid environment properties";

            if (envProps.requiredBiomes != null && envProps.requiredBiomes.Count > 0)
            {
                if (!envProps.requiredBiomes.Contains(pawn.Map?.Biome))
                    return $"Wrong biome (requires {string.Join(", ", envProps.requiredBiomes.Select(b => b.label))})";
            }

            float temp = pawn.AmbientTemperature;
            if (temp < envProps.minTemperature)
                return $"Too cold (requires {envProps.minTemperature}°C)";
            if (temp > envProps.maxTemperature)
                return $"Too hot (maximum {envProps.maxTemperature}°C)";

            float dayPercent = GenLocalDate.DayPercent(pawn.Map);
            if (envProps.requiresNight && (dayPercent < 0.2f || dayPercent > 0.8f))
                return "Can only evolve at night";
            if (envProps.requiresDay && (dayPercent > 0.2f && dayPercent < 0.8f))
                return "Can only evolve during day";

            if (envProps.requiredSeason.HasValue)
            {
                var currentSeason = GenLocalDate.Season(pawn.Map);
                if (currentSeason != envProps.requiredSeason.Value)
                    return $"Wrong season (requires {envProps.requiredSeason.Value})";
            }

            return null;
        }
    }
}