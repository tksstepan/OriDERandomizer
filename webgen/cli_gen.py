import argparse, time
import logging as log
from collections import OrderedDict

from util import enums_from_strlist
from enums import (MultiplayerGameType, ShareType, Variation, LogicPath, KeyMode, PathDifficulty, presets)
from seedbuilder.generator import SeedGenerator

def vals(enumType):
    return [v.value for v in enumType.__members__.values()]


class CLIMultiOptions(object):
    def __init__(self, mode=MultiplayerGameType.SIMUSOLO, shared=[], enabled=False, cloned=False, hints=False, teams={}):
        self.mode = mode
        self.shared = shared
        self.enabled = enabled
        self.cloned = cloned
        self.hints = hints
        self.teams = teams

class CLISeedParams(object):
    def __init__(self):
        pass

    def from_cli(self):
        parser = argparse.ArgumentParser()
        parser.add_argument("--output-dir", help="directory to put the seeds in", type=str, default=".")
        parser.add_argument("--preset", help="Choose a preset group of paths for the generator to use")
        parser.add_argument("--custom-logic", help="Customize paths that the generator will use, comma-separated: %s" % ", ".join(vals(LogicPath)))
        parser.add_argument("--seed", help="Seed value (default 'test')", type=str, default="test")
        parser.add_argument("--keymode", help="""Changes how the dungeon keys (The Water Vein, Gumon Seal, and Sunstone) are handled:
        Default: The dungeon keys are placed without any special consideration.
        Clues: For each 3 trees visited, the zone of a random dungeon key will be revealed
        Shards: The dungeon keys will be awarded after 3/5 shards are found
        LimitKeys: The Water Vein, Gumon Seal, and Sunstone will only appear at skill trees or event sources
        Free: The dungeon keys are given to the player upon picking up the first Energy Cell.
        """, type=str)
        # variations
        parser.add_argument("--hard", help="Enable hard mode", action="store_true")
        parser.add_argument("--ohko", help="Enable one-hit-ko mode", action="store_true")
        parser.add_argument("--zeroxp", help="Enable 0xp mode", action="store_true")
        parser.add_argument("--starved", help="Reduces the rate at which skills will appear when not required to advance", action="store_true")
        parser.add_argument("--non-progressive-mapstones", help="Map Stones will retain their behaviour from before v1.2, having their own unique drops", action="store_true")
        parser.add_argument("--force-trees", help="Prevent Ori from entering the final escape room until all skill trees have been visited", action="store_true")
        parser.add_argument("--force-mapstones", help="Prevent Ori from entering the final escape room until all mapstone altars have been activated", action="store_true")
        parser.add_argument("--entrance", help="Randomize entrances", action="store_true")
        parser.add_argument("--open", help="Activate open mode", action="store_true")
        parser.add_argument("--bonus-pickups", help="Adds some extra bonus pickups not balanced for competitive play", action="store_true")
        parser.add_argument("--easy", help="Add an extra copy of double jump, bash, stomp, glide, charge jump, dash, grenade, water, and wind", action="store_true")
        parser.add_argument("--free-mapstones", help="Don't require a mapstone to be placed when a map monument becomes accessible", action="store_true")
        parser.add_argument("--world-tour", help="Prevent Ori from entering the final escape until collecting one relic from each of the zones in the world", action="store_true")
        parser.add_argument("--warmth-frags", help="Prevent Ori from entering the final escape until collecting some number of warmth fragments. Recommended default: 40", type=int)

        # misc
        parser.add_argument("--verbose-paths", help="print every logic path in the flagline for debug purposes", action="store_true")
        parser.add_argument("--exp-pool", help="Size of the experience pool (default 10000)", type=int, default=10000)
        parser.add_argument("--extra-frags", help="""Sets the number of extra warmth fragments. Total frag number is still the value passed to --warmth-frags;
        --warmth-frags 40 --extra-frags 10 will place 40 total frags, 30 of which will be required to finish""", type=int, default=10)
        parser.add_argument("--prefer-path-difficulty", help="Increase the chances of putting items in more convenient (easy) or less convenient (hard) locations", choices=["easy", "hard"])
        parser.add_argument("--balanced", help="Reduce the value of newly discovered locations for progression placements", action="store_true")
        parser.add_argument("--force-cells", help="Force health and energy cells to appear every N pickups, if they don't randomly", type=int, default=256)
        # anal TODO: IMPL
        parser.add_argument("--analysis", help="Report stats on the skill order for all seeds generated", action="store_true")
        parser.add_argument("--loc-analysis", help="Report stats on where skills are placed over multiple seeds", action="store_true")
        parser.add_argument("--count", help="Number of seeds to generate (default 1)", type=int, default=1)
        # sync
        parser.add_argument("--players", help="Player count for paired randomizer", type=int, default=1)
        parser.add_argument("--tracking", help="Place a sync ID in a seed for tracking purposes", action="store_true")
        parser.add_argument("--sync-id", help="Team identifier number for paired randomizer", type=int)
        parser.add_argument("--shared-items", help="What will be shared by sync, comma-separated: skills,keys,events,teleporters,upgrades", default="skills,keys")
        parser.add_argument("--share-mode", help="How the server will handle shared pickups, one of: shared,swap,split,none", default="shared")
        parser.add_argument("--cloned", help="Make a split cloned seed instead of seperate seeds", action="store_true")
        parser.add_argument("--teams", help="Cloned seeds only: define teams. Format: 1|2,3,4|5,6. Each player must appear once", type=str)
        parser.add_argument("--hints", help="Cloned seeds only: display a hint with the item category on a shared location instead of 'Warmth Returned'", action="store_true")
        args = parser.parse_args()

        """
        path_diff = property(get_pathdiff, set_pathdiff)
        exp_pool = ndb.IntegerProperty(default=10000)
        balanced = ndb.BooleanProperty(default=True)
        tracking = ndb.BooleanProperty(default=True)
        players = ndb.IntegerProperty(default=1)
        sync = ndb.LocalStructuredProperty(MultiplayerOptions)
        frag_count = ndb.IntegerProperty(default=40)
        frag_extra = ndb.IntegerProperty(default=10)
        cell_freq = ndb.IntegerProperty(default=256)
        """
        self.seed = args.seed
        if args.preset:
            self.logic_paths = presets[args.preset.capitalize()]
        elif args.custom_logic:
            self.logic_paths = enums_from_strlist(LogicPath, args.custom_logic.split(","))
        else:
            self.logic_paths = presets["Standard"]
        self.key_mode = KeyMode.mk(args.keymode) or KeyMode.NONE
        # variations (help)
        varMap = {
            "zeroxp": "0XP", "hard": "Hard", "non_progressive_mapstones": "NonProgressMapStones", "ohko": "OHKO", "force_trees": "ForceTrees", "starved": "Starved",
            "force_mapstones": "ForceMapStones", "entrance": "Entrance", "open": "Open", "easy": "DoubleSkills", "free_mapstones": "FreeMapstones", 
            "warmth_frags": "WarmthFrags", "world_tour": "WorldTour"
            }
        self.variations = []
        for argName, flagStr in varMap.iteritems():
            if getattr(args, argName, False):
                v = Variation.mk(flagStr)
                if v:
                    self.variations.append(v)
                else:
                    log.warning("Failed to make a Variation from %s" % flagStr)
        if Variation.WARMTH_FRAGMENTS in self.variations:
            self.frag_count = args.warmth_frags
            self.frag_extra = args.extra_frags
        #misc
        self.exp_pool = args.exp_pool
        if args.prefer_path_difficulty:
            if args.prefer_path_difficulty == "easy":
                self.path_diff = PathDifficulty.EASY
            else:
                self.path_diff = PathDifficulty.HARD
        else:
            self.path_diff = PathDifficulty.NORMAL
        self.balanced = args.balanced or False
        self.cell_freq = args.force_cells
        self.players = args.players
        self.tracking = args.tracking or False
        self.sync = CLIMultiOptions()
        if self.players > 1 or self.tracking:
            self.sync_id = args.sync_id or int(time.time() * 1000 % 1073741824)
        if self.players > 1:
            self.sync.enabled = True
            self.sync.mode = MultiplayerGameType.mk(args.share_mode) or MultiplayerGameType.SIMUSOLO
            self.sync.shared = enums_from_strlist(ShareType, args.shared_items.split(","))
            self.sync.cloned = args.cloned or False
            if self.sync.cloned:
                self.sync.hints = args.hints or False
                if args.teams:
                    cnt = 1
                    self.sync.teams = {}
                    for team in args.teams.split("|"):
                        self.sync.teams[cnt] = [int(p) for p in team.split(",")]
                        cnt += 1

        # todo: respect these LMAO
        self.do_analysis = args.analysis
        self.do_log_analysis = args.loc_analysis
        self.repeat_count = args.count

        sg = SeedGenerator()
        raw = sg.setSeedAndPlaceItems(self, preplaced={})
        seeds = []
        spoilers = []
        if not raw:
                log.error("Couldn't build seed!")
                return
        player = 0
        for player_raw in raw:
            player += 1
            seed, spoiler = tuple(player_raw)
            if self.tracking:
                seed = "Sync%s.%s," % (self.sync_id, player) + seed
            seedfile = "randomizer_%s.dat" % player
            spoilerfile = "spoiler_%s.txt" % player
            if self.players == 1:
                seedfile = "randomizer.dat"
                spoilerfile = "spoiler.txt"

            with open(args.output_dir+"/"+seedfile, 'w') as f:
                f.write(seed)
            with open(args.output_dir+"/"+spoilerfile, 'w') as f:
                f.write(spoiler)

    def get_preset(self):
        pathset = set(self.logic_paths)
        for name, lps in presets.iteritems():
            if lps == pathset:
                if name == "Standard" and Variation.ZERO_EXP in self.variations:
                    return "0xp"
                return name
        return "Custom"

    def flag_line(self, verbose_paths=False):
        flags = []
        if verbose_paths:
            flags.append("lps=%s" % "+".join([lp.capitalize() for lp in self.logic_paths]))
        else:
            flags.append(self.get_preset())
        flags.append(self.key_mode)
        if Variation.WARMTH_FRAGMENTS in self.variations:
            flags.append("Frags/%s/%s" % (self.frag_count, self.frag_extra))
        flags += [v.value for v in self.variations]
        if self.path_diff != PathDifficulty.NORMAL:
            flags.append("prefer_path_difficulty=%s" % self.path_diff.value)
        if self.sync.enabled:
            flags.append("mode=%s" % self.sync.mode.value)
            if self.sync.shared:
                flags.append("shared=%s" % "+".join(self.sync.shared))
        if self.balanced:
            flags.append("balanced")
        return "%s|%s" % (",".join(flags), self.seed)


if __name__ == "__main__":
    params = CLISeedParams()
    params.from_cli()
