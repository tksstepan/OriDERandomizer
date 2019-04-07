import sys, os
import logging as log
LIBS = os.path.join(os.path.dirname(os.path.realpath(__file__)), "lib")
if LIBS not in sys.path:
    sys.path.insert(0, LIBS)
from enum import Enum

class StrEnum(str, Enum):
    @classmethod
    def mk(cls, val, fuzzycase=True):
        if val is None:
            log.warning("None is not a valid %s, returning None" % cls)
            return None
        try:
            return cls(val)
        except ValueError:
            if fuzzycase:
                for possible_match in cls.__members__.values():
                    if possible_match._value_.lower() == val.lower():
                        return possible_match
                log.warning("%s could not be parsed into to any valid %s, returning None" % (val, cls))
            else:
                log.warning("%s is not a valid %s, returning None" % (val, cls))
                return None

class MultiplayerGameType(StrEnum):
    SHARED = "Shared"
    SPLITSHARDS = "SplitShards"
    SIMUSOLO = "None"
    BINGO="Bingo"

    def is_dedup(self): return self in [MultiplayerGameType.SHARED]

class ShareType(StrEnum):
    NOT_SHARED = "Unshareable"
    UPGRADE = "Upgrades"
    SKILL = "Skills"
    EVENT = "WorldEvents"
    TELEPORTER = "Teleporters"
    MISC = "Misc"

class Variation(StrEnum):
    ZERO_EXP = "0XP"
    DISCRETE_MAPSTONES = "NonProgressMapStones"
    ENTRANCE_SHUFFLE = "Entrance"
    FORCE_MAPSTONES = "ForceMaps"
    FORCE_TREES = "ForceTrees"
    HARDMODE = "Hard"
    ONE_HIT_KO = "OHKO"
    STARVED = "Starved"
    EXTRA_BONUS_PICKUPS = "BonusPickups"
    CLOSED_DUNGEONS = "ClosedDungeons"
    OPEN_WORLD = "OpenWorld"
    WORLD_TOUR = "WorldTour"
    WARMTH_FRAGMENTS = "WarmthFrags"
    DOUBLE_SKILL = "DoubleSkills"
    STRICT_MAPSTONES = "StrictMapstones"
    STOMP_TRIGGERS = "StompTriggers"
    NO_EXTRA_EXP = "NoExtraExp"

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
            LogicPath.GLITCHED, LogicPath.TIMED_LEVEL
        }
    }