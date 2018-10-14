import sys,os
import logging as log
LIBS = os.path.join(os.path.dirname(os.path.realpath(__file__)), "lib")
if LIBS not in sys.path:
    sys.path.insert(0, LIBS)

from enum import Enum

class StrEnum(str, Enum):
    @classmethod
    def mk(cls, val, try_others=True):
        if val is None:
            log.warning("None is not a valid %s, returning None" % cls)
            return None
        try:
            return cls(val)
        except ValueError:
            if try_others:
                for other in [val.lower(), val.capitalize(), val.upper()]:
                    res = cls.mk(other, False)
                    if res:
                        return res
            log.warning("%s is not a valid %s, returning None" % (val, cls))
            return None

class MultiplayerGameType(StrEnum):
    SHARED = "Shared"
    SPLITSHARDS = "SplitShards"
    SIMUSOLO = "None"

    def is_dedup(self): return self in [MultiplayerGameType.SHARED]



class ShareType(StrEnum):
    NOT_SHARED = "Unshareable"
    UPGRADE = "Upgrades"
    SKILL = "Skills"
    EVENT = "WorldEvents"
    TELEPORTER = "Teleporters"
    MISC = "Misc"

oldFlags = {"starved": "Starved", "hardmode": "Hard", "ohko": "OHKO", "0xp": "0XP",  "noplants": "NoPlants", "forcetrees": "ForceTrees", "discmaps": "NonProgressMapStones", "notp": "NoTeleporters", "entshuf": "Entrance", "wild": "BonusPickups", "forcemapstones": "ForceMapStones", "forcerandomescape": "ForceRandomEscape"}

class Variation(StrEnum):
    ZERO_EXP = "0XP"
    DISCRETE_MAPSTONES = "NonProgressMapStones"
    ENTRANCE_SHUFFLE = "Entrance"
    FORCE_MAPSTONES = "ForceMapStones"
    FORCE_TREES = "ForceTrees"
    HARDMODE = "Hard"
    ONE_HIT_KO = "OHKO"
    STARVED = "Starved"
    EXTRA_BONUS_PICKUPS = "BonusPickups"
    OPEN_MODE = "Open"
    WORLD_TOUR = "WorldTour"
    WARMTH_FRAGMENTS = "WarmthFrags"
    DOUBLE_SKILL = "DoubleSkills"
    FREE_MAPSTONES = "FreeMapstones"
    @staticmethod
    def from_old(old):
        low = old.lower()
        if low not in oldFlags:
            return None
        return Variation(oldFlags[old])

class LogicPath(StrEnum):
    CASUAL_CORE = 'casual-core'
    CASUAL_DBOOST = 'casual-dboost'
    STANDARD_CORE = 'standard-core'
    STANDARD_DBOOST = 'standard-dboost'
    STANDARD_LURE = 'standard-lure'
    STANDARD_ABILITIES = 'standard-abilities'
    EXPERT_CORE = 'expert-core'
    EXPERT_DBOOST = 'expert-dboost'
    EXPERT_LURE = 'expert-lure'
    EXPERT_ABILITIES = 'expert-abilities'
    DBASH = 'dbash'
    MASTER_CORE = 'master-core'
    MASTER_DBOOST = 'master-dboost'
    MASTER_LURE = 'master-lure'
    MASTER_ABILITIES = 'master-abilities'
    GJUMP = 'gjump'
    GLITCHED = 'glitched'
    TIMED_LEVEL = 'timed-level'
    INSANE = 'insane'

class KeyMode(StrEnum):
    SHARDS = "Shards"
    CLUES = "Clues"
    LIMITKEYS = "Limitkeys"
    FREE = "Free"
    NONE = "Default"

class PathDifficulty(StrEnum):
    EASY = "Easy"
    NORMAL = "Normal"
    HARD = "Hard"

presets = {
    "Casual": {LogicPath.CASUAL_CORE, LogicPath.CASUAL_DBOOST},
    "Standard": {
        LogicPath.CASUAL_CORE, LogicPath.CASUAL_DBOOST, 
        LogicPath.STANDARD_CORE, LogicPath.STANDARD_DBOOST, LogicPath.STANDARD_LURE, LogicPath.STANDARD_ABILITIES
        },
    "Expert": {
        LogicPath.CASUAL_CORE, LogicPath.CASUAL_DBOOST, 
        LogicPath.STANDARD_CORE, LogicPath.STANDARD_DBOOST, LogicPath.STANDARD_LURE, LogicPath.STANDARD_ABILITIES,
        LogicPath.EXPERT_CORE, LogicPath.EXPERT_DBOOST, LogicPath.EXPERT_LURE, LogicPath.EXPERT_ABILITIES, LogicPath.DBASH
        },
    "Master": {
            LogicPath.CASUAL_CORE, LogicPath.CASUAL_DBOOST, 
            LogicPath.STANDARD_CORE, LogicPath.STANDARD_DBOOST, LogicPath.STANDARD_LURE, LogicPath.STANDARD_ABILITIES,
            LogicPath.EXPERT_CORE, LogicPath.EXPERT_DBOOST, LogicPath.EXPERT_LURE, LogicPath.EXPERT_ABILITIES, LogicPath.DBASH,
            LogicPath.MASTER_CORE, LogicPath.MASTER_DBOOST, LogicPath.MASTER_LURE, LogicPath.MASTER_ABILITIES, LogicPath.GJUMP
        },
    "Glitched": {
            LogicPath.CASUAL_CORE, LogicPath.CASUAL_DBOOST, 
            LogicPath.STANDARD_CORE, LogicPath.STANDARD_DBOOST, LogicPath.STANDARD_LURE, LogicPath.STANDARD_ABILITIES,
            LogicPath.EXPERT_CORE, LogicPath.EXPERT_DBOOST, LogicPath.EXPERT_LURE, LogicPath.EXPERT_ABILITIES, LogicPath.DBASH,
            LogicPath.MASTER_CORE, LogicPath.MASTER_DBOOST, LogicPath.MASTER_LURE, LogicPath.MASTER_ABILITIES, LogicPath.GJUMP,
            LogicPath.GLITCHED, LogicPath.TIMED_LEVEL
        }
    }

