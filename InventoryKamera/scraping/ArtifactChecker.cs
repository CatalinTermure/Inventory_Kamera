using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryKamera
{
    public static class ArtifactChecker
    {
        private static readonly string[] _emSets =
            { "WanderersTroupe", "GildedDreams", "FlowerOfParadiseLost", "ViridescentVenerer", "DeepwoodMemories" };

        private static readonly string[] _damageBonusStats =
        {
            "physical_dmg_", "anemo_dmg_", "geo_dmg_", "electro_dmg_", "hydro_dmg_", "pyro_dmg_", "cryo_dmg_",
            "dendro_dmg_"
        };

        private static decimal GetCritRate(Artifact artifact) =>
            (from subStat in artifact.SubStats where subStat.stat == "critRate_" select subStat.value)
            .FirstOrDefault();

        private static decimal GetCritDamage(Artifact artifact) =>
            (from subStat in artifact.SubStats where subStat.stat == "critDMG_" select subStat.value)
            .FirstOrDefault();

        private static decimal GetEM(Artifact artifact) =>
            (from subStat in artifact.SubStats where subStat.stat == "eleMas" select subStat.value)
            .FirstOrDefault();

        private static decimal GetER(Artifact artifact) =>
            (from subStat in artifact.SubStats where subStat.stat == "enerRech_" select subStat.value)
            .FirstOrDefault();

        private static decimal GetHPPercent(Artifact artifact) =>
            (from subStat in artifact.SubStats where subStat.stat == "hp_" select subStat.value)
            .FirstOrDefault();

        private static bool IsEMSet(Artifact artifact) => _emSets.Contains(artifact.SetName);

        private static bool IsDamageBonusGoblet(Artifact artifact) => _damageBonusStats.Contains(artifact.MainStat);

        private static bool IsDamageBonusTrashable(Artifact artifact)
        {
            if (!IsDamageBonusGoblet(artifact)) return true;
            if (GetCritDamage(artifact) > 0 || GetCritRate(artifact) > 0 || GetER(artifact) > 0) return false;
            return true;
        }

        private static bool IsHPArtifactTrashable(Artifact artifact)
        {
            if (artifact.MainStat == "hp_" && artifact.GearSlot == "sands")
            {
                if (GetER(artifact) > 0) return false;
                if ((GetCritRate(artifact) > 0 || GetCritDamage(artifact) > 0) && artifact.SubStatsCount == 3)
                    return false;
                if (GetCritRate(artifact) > 0 && GetCritDamage(artifact) > 0) return false;
            }
            else if (artifact.MainStat == "heal_")
            {
                if (GetHPPercent(artifact) > 0 && GetER(artifact) > 0) return false;
            }

            return true;
        }

        private static bool IsCritArtifactTrashable(Artifact artifact)
        {
            if (artifact.MainStat == "critRate_" || artifact.MainStat == "critDMG_") return false;
            return true;
        }

        private static bool IsAttackArtifactTrashable(Artifact artifact)
        {
            if (artifact.MainStat == "atk_")
            {
                if (GetER(artifact) > 0) return false;
                if ((GetCritRate(artifact) > 0 || GetCritDamage(artifact) > 0) && artifact.SubStatsCount == 3)
                    return false;
                if (GetCritRate(artifact) > 0 && GetCritDamage(artifact) > 0) return false;
            }

            return true;
        }

        private static bool IsERArtifactTrashable(Artifact artifact)
        {
            if (artifact.MainStat == "enerRech_")
            {
                if ((GetCritRate(artifact) > 0 || GetCritDamage(artifact) > 0) && artifact.SubStatsCount == 3)
                    return false;
                if (GetCritRate(artifact) > 0 && GetCritDamage(artifact) > 0) return false;
            }

            return true;
        }

        private static bool IsEleMasArtifactTrashable(Artifact artifact)
        {
            if (artifact.MainStat == "eleMas" && GetER(artifact) > 0) return false;
            if (artifact.GearSlot == "flower" || artifact.GearSlot == "plume")
            {
                if (IsEMSet(artifact) && GetEM(artifact) > 0 && GetER(artifact) > 0) return false;
                if (IsEMSet(artifact) && GetEM(artifact) > 0 && artifact.SubStatsCount == 3) return false;
            }

            return true;
        }

        private static bool AreSubStatsTrashable(Artifact artifact)
        {
            if (!IsEleMasArtifactTrashable(artifact)) return false;
            if (artifact.SubStatsCount == 4)
            {
                if (GetCritRate(artifact) > 0 && GetCritDamage(artifact) > 0) return false;
                if (GetHPPercent(artifact) > 0 && GetER(artifact) > 0) return false;
            }
            else
            {
                if (GetCritRate(artifact) > 0 || GetCritDamage(artifact) > 0) return false;
                if (GetHPPercent(artifact) > 0 && GetER(artifact) > 0) return false;
            }

            return true;
        }

        private static bool IsHourglassTrashable(Artifact artifact)
        {
            if (!IsEleMasArtifactTrashable(artifact)) return false;
            if (!IsHPArtifactTrashable(artifact)) return false;
            if (!IsAttackArtifactTrashable(artifact)) return false;
            if (!IsERArtifactTrashable(artifact)) return false;
            return true;
        }

        private static bool IsGobletTrashable(Artifact artifact)
        {
            if (artifact.MainStat == "hp_" && GetER(artifact) > 0) return false;
            if (!IsDamageBonusTrashable(artifact)) return false;
            if (!IsEleMasArtifactTrashable(artifact)) return false;
            return true;
        }

        private static bool IsCrownTrashable(Artifact artifact)
        {
            if (!IsEleMasArtifactTrashable(artifact)) return false;
            if (!IsHPArtifactTrashable(artifact)) return false;
            if (!IsCritArtifactTrashable(artifact)) return false;
            return true;
        }

        public static bool IsTrashable(Artifact artifact, List<Artifact> inventory)
        {
            if (!String.IsNullOrWhiteSpace(artifact.EquippedCharacter)) return false;
            switch (artifact.GearSlot)
            {
                case "flower":
                case "plume":
                    return AreSubStatsTrashable(artifact);
                case "sands":
                    return IsHourglassTrashable(artifact);
                case "goblet":
                    return IsGobletTrashable(artifact);
                case "circlet":
                    return IsCrownTrashable(artifact);
                default:
                    return true;
            }
        }
    }
}