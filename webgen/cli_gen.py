import argparse, time
import logging as log
from collections import OrderedDict

from util import enums_from_strlist
from enums import (MultiplayerGameType, ShareType, Variation, LogicPath, KeyMode, PathDifficulty, presets)
from seedbuilder.generator import SeedGenerator

FLAGLESS_VARS = [Variation.WARMTH_FRAGMENTS, Variation.WORLD_TOUR]

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
        parser.add_argument("--closed-dungeons", help="deactivate open mode within dungeons", action="store_true")
        parser.add_argument("--open-world", help="Activate open mode on the world map", action="store_true")
        parser.add_argument("--bonus-pickups", help="Adds some extra bonus pickups not balanced for competitive play", action="store_true")
        parser.add_argument("--easy", help="Add an extra copy of double jump, bash, stomp, glide, charge jump, dash, grenade, water, and wind", action="store_true")
        parser.add_argument("--free-mapstones", help="Don't require a mapstone to be placed when a map monument becomes accessible", action="store_true")
        parser.add_argument("--world-tour", help="Prevent Ori from entering the final escape until collecting one relic from each of the zones in the world. Recommended default: 8", type=int)
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
        parser.add_argument("--shared-items", help="What will be shared by sync, comma-separated: skills,worldevents,misc,teleporters,upgrades", default="skills,worldevents")
        parser.add_argument("--share-mode", help="How the server will handle shared pickups, one of: shared,swap,split,none", default="shared")
        parser.add_argument("--cloned", help="Make a split cloned seed instead of seperate seeds", action="store_true")
        parser.add_argument("--teams", help="Cloned seeds only: define teams. Format: 1|2,3,4|5,6. Each player must appear once", type=str)
        parser.add_argument("--hints", help="Cloned seeds only: display a hint with the item category on a shared location instead of 'Warmth Returned'", action="store_true")
        parser.add_argument("--do-reachability-analysis", help="Analyze how many locations are opened by various progression items in various inventory states", action="store_true")
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
            "force_mapstones": "ForceMapStones", "entrance": "Entrance", "open_world": "OpenWorld", "easy": "DoubleSkills", "free_mapstones": "FreeMapstones", 
            "warmth_frags": "WarmthFrags", "world_tour": "WorldTour", "closed_dungeons": "ClosedDungeons"
        }
        self.variations = []
        for argName, flagStr in varMap.iteritems():
            if getattr(args, argName, False):
                v = Variation.mk(flagStr)
                if v:
                    self.variations.append(v)
                else:
                    log.warning("Failed to make a Variation from %s" % flagStr)
        if Variation.WORLD_TOUR in self.variations:
            self.relic_count = args.world_tour
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
        self.do_loc_analysis = args.loc_analysis
        self.repeat_count = args.count

        base_seed = 0
        if self.repeat_count > 1:
            base_seed = hash(self.seed)

        if self.do_loc_analysis:
            self.locationAnalysis = {}
            self.itemsToAnalyze = {
                "WallJump": 0,
                "ChargeFlame": 0,
                "DoubleJump": 0,
                "Bash": 0,
                "Stomp": 0,
                "Glide": 0,
                "Climb": 0,
                "ChargeJump": 0,
                "Dash": 0,
                "Grenade": 0,
                "GinsoKey": 0,
                "ForlornKey": 0,
                "HoruKey": 0,
                "Water": 0,
                "Wind": 0,
                "WaterVeinShard": 0,
                "GumonSealShard": 0,
                "SunstoneShard": 0,
                "TPForlorn": 0,
                "TPGrotto": 0,
                "TPSorrow": 0,
                "TPGrove": 0,
                "TPSwamp": 0,
                "TPValley": 0,
                "TPGinso": 0,
                "TPHoru": 0,
                "Relic": 0
            }
            for i in range(1,10):
                self.locationAnalysis["MapStone " + str(i)] = self.itemsToAnalyze.copy()
                self.locationAnalysis["MapStone " + str(i)]["Zone"] = "MapStone"

        for count in range(0, args.count):

            if self.repeat_count > 1:
                self.seed = base_seed + count

            if self.do_loc_analysis:
                print(self.seed)

            sg = SeedGenerator()

            if args.do_reachability_analysis:
                sg.do_reachability_analysis(self)
                return

            raw = sg.setSeedAndPlaceItems(self, preplaced={})
            seeds = []
            spoilers = []
            if not raw:
                    log.error("Couldn't build seed!")
                    if self.do_loc_analysis:
                        continue
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
                    seedfile = "randomizer" + str(count) + ".dat"
                    spoilerfile = "spoiler" + str(count) + ".txt"

                if not self.do_analysis and not self.do_loc_analysis:
                    with open(args.output_dir+"/"+seedfile, 'w') as f:
                        f.write(seed)
                    with open(args.output_dir+"/"+spoilerfile, 'w') as f:
                        f.write(spoiler)

        if self.do_loc_analysis:
            output = open("analysis.csv", 'w')
            output.write("Location,Zone,WallJump,ChargeFlame,DoubleJump,Bash,Stomp,Glide,Climb,ChargeJump,Dash,Grenade,GinsoKey,ForlornKey,HoruKey,Water,Wind,WaterVeinShard,GumonSealShard,SunstoneShard,TPGrove,TPGrotto,TPSwamp,TPValley,TPSorrow,TPGinso,TPForlorn,TPHoru,Relic\n")
            for key in self.locationAnalysis.keys():
                line = key + ","
                line += str(self.locationAnalysis[key]["Zone"]) + ","
                line += str(self.locationAnalysis[key]["WallJump"]) + ","
                line += str(self.locationAnalysis[key]["ChargeFlame"]) + ","
                line += str(self.locationAnalysis[key]["DoubleJump"]) + ","
                line += str(self.locationAnalysis[key]["Bash"]) + ","
                line += str(self.locationAnalysis[key]["Stomp"]) + ","
                line += str(self.locationAnalysis[key]["Glide"]) + ","
                line += str(self.locationAnalysis[key]["Climb"]) + ","
                line += str(self.locationAnalysis[key]["ChargeJump"]) + ","
                line += str(self.locationAnalysis[key]["Dash"]) + ","
                line += str(self.locationAnalysis[key]["Grenade"]) + ","
                line += str(self.locationAnalysis[key]["GinsoKey"]) + ","
                line += str(self.locationAnalysis[key]["ForlornKey"]) + ","
                line += str(self.locationAnalysis[key]["HoruKey"]) + ","
                line += str(self.locationAnalysis[key]["Water"]) + ","
                line += str(self.locationAnalysis[key]["Wind"]) + ","
                line += str(self.locationAnalysis[key]["WaterVeinShard"]) + ","
                line += str(self.locationAnalysis[key]["GumonSealShard"]) + ","
                line += str(self.locationAnalysis[key]["SunstoneShard"]) + ","
                line += str(self.locationAnalysis[key]["TPGrove"]) + ","
                line += str(self.locationAnalysis[key]["TPGrotto"]) + ","
                line += str(self.locationAnalysis[key]["TPSwamp"]) + ","
                line += str(self.locationAnalysis[key]["TPValley"]) + ","
                line += str(self.locationAnalysis[key]["TPSorrow"]) + ","
                line += str(self.locationAnalysis[key]["TPGinso"]) + ","
                line += str(self.locationAnalysis[key]["TPForlorn"]) + ","
                line += str(self.locationAnalysis[key]["TPHoru"]) + ","
                line += str(self.locationAnalysis[key]["Relic"])

                output.write(line + "\n")

    def get_preset(self):
        pathset = set(self.logic_paths)
        for name, lps in presets.iteritems():
            if lps == pathset:
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
        if Variation.WORLD_TOUR in self.variations:
            flags.append("WorldTour=%s" % self.relic_count)
        flags += [v.value for v in self.variations if v not in FLAGLESS_VARS]
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
