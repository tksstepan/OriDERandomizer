from google.appengine.ext import ndb

import logging as log

from util import enums_from_strlist
from enums import (MultiplayerGameType, ShareType, Variation, LogicPath, KeyMode, PathDifficulty, presets)
from collections import OrderedDict
from seedbuilder.generator import SeedGenerator

FLAGLESS_VARS = [Variation.WARMTH_FRAGMENTS, Variation.WORLD_TOUR]

class Stuff(ndb.Model):
    code = ndb.StringProperty()
    id = ndb.StringProperty()
    player = ndb.StringProperty()

class Placement(ndb.Model):
    location = ndb.StringProperty()
    zone = ndb.StringProperty()
    stuff = ndb.LocalStructuredProperty(Stuff, repeated=True)

class MultiplayerOptions(ndb.Model):
    str_mode = ndb.StringProperty(default="None")
    str_shared = ndb.StringProperty(repeated=True)

    def get_mode(self): return MultiplayerGameType.mk(self.str_mode) or MultiplayerGameType.SIMUSOLO

    def set_mode(self, mode):         self.str_mode = mode.value

    def get_shared(self): return enums_from_strlist(ShareType, self.str_shared)

    def set_shared(self, shared):    self.str_shared = [s.value for s in shared]

    mode = property(get_mode, set_mode)
    shared = property(get_shared, set_shared)
    enabled = ndb.BooleanProperty(default=False)
    cloned = ndb.BooleanProperty(default=True)
    hints = ndb.BooleanProperty(default=True)
    teams = ndb.JsonProperty(default={})

    @staticmethod
    def from_url(qparams):
        opts = MultiplayerOptions()
        opts.enabled = int(qparams.get("players", 1)) > 1
        if opts.enabled:
            opts.mode = MultiplayerGameType(qparams.get("sync_mode", "None"))
            opts.cloned = qparams.get("sync_gen") != "disjoint"
            opts.hints = bool(opts.cloned and qparams.get("sync_hints"))
            opts.shared = enums_from_strlist(ShareType, qparams.getall("sync_shared"))
            teamsRaw = qparams.get("teams")
            if teamsRaw and opts.mode == MultiplayerGameType.SHARED and opts.cloned:
                cnt = 1
                teams = {}
                for teamRaw in teamsRaw.split("|"):
                    teams[cnt] = [int(p) for p in teamRaw.split(",")]
                    cnt += 1
                opts.teams = teams
        return opts


class SeedGenParams(ndb.Model):
    str_vars = ndb.StringProperty(repeated=True)
    str_paths = ndb.StringProperty(repeated=True)
    str_pathdiff = ndb.StringProperty(default=PathDifficulty.NORMAL.value)
    str_keymode = ndb.StringProperty(default=KeyMode.CLUES.value)

    def get_pathdiff(self): return PathDifficulty(self.str_pathdiff)

    def set_pathdiff(self, pathdiff): self.str_pathdiff = pathdiff.value

    def get_vars(self): return enums_from_strlist(Variation, self.str_vars)

    def set_vars(self, vars): self.str_vars = [v.value for v in vars]

    def get_paths(self): return enums_from_strlist(LogicPath, self.str_paths)

    def set_paths(self, paths): self.str_paths = [p.value for p in paths]

    def get_keymode(self): return KeyMode(self.str_keymode)

    def set_keymode(self, key_mode):     self.str_keymode = key_mode.value

    seed = ndb.StringProperty(required=True)
    variations = property(get_vars, set_vars)
    logic_paths = property(get_paths, set_paths)
    key_mode = property(get_keymode, set_keymode)
    path_diff = property(get_pathdiff, set_pathdiff)
    exp_pool = ndb.IntegerProperty(default=10000)
    balanced = ndb.BooleanProperty(default=True)
    tracking = ndb.BooleanProperty(default=True)
    players = ndb.IntegerProperty(default=1)
    created_on = ndb.DateTimeProperty(auto_now_add=True)
    sync = ndb.LocalStructuredProperty(MultiplayerOptions)
    frag_count = ndb.IntegerProperty(default=30)
    frag_req = ndb.IntegerProperty(default=20)
    relic_count = ndb.IntegerProperty(default=8)
    cell_freq = ndb.IntegerProperty(default=256)
    placements = ndb.LocalStructuredProperty(Placement, repeated=True, compressed=True)
    spoilers = ndb.TextProperty(repeated=True, compressed=True)
    sense = ndb.StringProperty()
    is_plando = ndb.BooleanProperty(default=False)
    plando_flags = ndb.StringProperty(repeated=True)
    do_loc_analysis = False

    @staticmethod
    def from_plando(plando, tracking=True):
        params = SeedGenParams(
            seed = plando.name,
            players = plando.players,
            tracking = tracking,
            is_plando=True, 
            plando_flags = plando.flags,
            placements = plando.placements,
            spoilers = [plando.description]
            )
        params.sync = MultiplayerOptions()
        if plando.mode():
            params.sync.mode = plando.mode()
        params.sync.shared = plando.shared()

        
        params.put()
        return params

    @staticmethod
    def from_url(qparams):
        params = SeedGenParams()
        params.seed = qparams.get("seed")
        if not params.seed:
            log.error("No seed in %r! returning None" % qparams)
            return None
        params.variations = enums_from_strlist(Variation, qparams.getall("var"))
        params.logic_paths = enums_from_strlist(LogicPath, qparams.getall("path"))
        if not params.logic_paths:
            log.error("No logic paths in %r! returning None" % qparams)
            return None
        params.key_mode = KeyMode(qparams.get("key_mode", "Clues"))
        params.path_diff = PathDifficulty(qparams.get("path_diff", "Normal"))
        params.exp_pool = int(qparams.get("exp_pool", 10000))
        params.balanced = qparams.get("gen_mode") != "Classic"
        params.players = int(qparams.get("players", 1))
        params.tracking = qparams.get("tracking") != "Disabled"
        params.frag_count = int(qparams.get("frags", 30))
        params.frag_req = int(qparams.get("frags_req", 20))
        params.relic_count = int(qparams.get("relics", 8))
        params.cell_freq = int(qparams.get("cell_freq", 256))
        params.sync = MultiplayerOptions.from_url(qparams)
        params.sense = qparams.get("sense")
        raw_fass = qparams.get("fass")
        if raw_fass:
            params.placements = []
            for fass in raw_fass.split("|"):
                loc, _, item = fass.partition(":")
                stuff = [Stuff(code=item[:2], id=item[2:], player="")]
                params.placements.append(Placement(location=loc, zone="", stuff=stuff))
        return params.put()

    def generate(self, preplaced={}):
        if self.placements:
            preplaced = {}
            for placement in self.placements:
                s = placement.stuff[0]
                preplaced[int(placement.location)] = s.code + s.id
            self.placements = []
        sg = SeedGenerator()
        raw = sg.setSeedAndPlaceItems(self, preplaced=preplaced)
        placemap = OrderedDict()
        spoilers = []
        if not raw:
                return False
        player = 0
        for player_raw in raw:
            player += 1
            seed, spoiler = tuple(player_raw)
            spoilers.append(spoiler)
            for line in seed.split("\n")[1:-1]:
                loc, stuff_code, stuff_id, zone = tuple(line.split("|"))
                stuff = Stuff(code=stuff_code, id=stuff_id, player=str(player))
                if loc not in placemap:
                    placemap[loc] = Placement(location=loc, zone=zone, stuff=[stuff])
                else:
                    placemap[loc].stuff.append(stuff)
        if self.sync.mode in [MultiplayerGameType.SIMUSOLO, MultiplayerGameType.SPLITSHARDS]:
            if player != 1:
                log.error("seed count mismatch! Should only be 1 seed for this mode and instead found  %s", player)
                return False
        elif player != self.players and player != len(self.sync.teams):
            log.error("seed count mismatch!, %s != %s or %s", player, self.players, len(self.sync.teams))
            return False
        self.spoilers = spoilers
        self.placements = placemap.values()
        self.put()
        return True

    def teams_inv(self):  # generates {pid: tid}
        return {pid: tid for tid, pids in self.sync.teams.iteritems() for pid in pids}

    def team_pid(self, pid):  # given pid, get team or return pid if no teams exist
        return int(self.teams_inv()[pid]) if self.sync.teams else pid

    def get_seed(self, player=1, game_id=None, verbose_paths=False):
        flags = self.flag_line(verbose_paths)
        if self.tracking:
            if not game_id:
                log.warning("Trying to get a tracked seed with no gameId! paramId %s", self.key.id())
            flags = "Sync%s.%s," % (game_id, player) + flags
        outlines = [flags]
        outlines += ["|".join(line) for line in self.get_seed_data(player)]
        return "\n".join(outlines) + "\n"

    def get_seed_data(self, player=1):
        player = int(player)
        if self.sync.mode in [MultiplayerGameType.SIMUSOLO, MultiplayerGameType.SPLITSHARDS]:
            player = 1
        return [(str(p.location), s.code, s.id, p.zone) for p in self.placements for s in p.stuff if int(s.player) == self.team_pid(player)]

    def get_spoiler(self, player=1):
        if self.sync.mode in [MultiplayerGameType.SIMUSOLO, MultiplayerGameType.SPLITSHARDS]:
            player = 1
        if self.is_plando:
            return self.spoilers[0]
        return self.spoilers[self.team_pid(player) - 1]

    def get_preset(self):
        pathset = set(self.logic_paths)
        for name, lps in presets.iteritems():
            if lps == pathset:
                return name
        return "Custom"

    def flag_line(self, verbose_paths=False):
        flags = []
        if self.is_plando:
            flags = self.plando_flags
        else:
            if verbose_paths:
                flags.append("lps=%s" % "+".join([lp.capitalize() for lp in self.logic_paths]))
            else:
                flags.append(self.get_preset())
            flags.append(self.key_mode)
            if Variation.WARMTH_FRAGMENTS in self.variations:
                flags.append("Frags/%s/%s" % (self.frag_req, self.frag_count))
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
            if self.sense:
                flags.append("sense=%s" % self.sense.replace(" ", "+"))
        return "%s|%s" % (",".join(flags), self.seed)

    @staticmethod
    def with_id(id):
        return SeedGenParams.get_by_id(int(id))
