import math
import random
import logging as log
import xml.etree.ElementTree as XML
from collections import OrderedDict, defaultdict, Counter
from operator import mul
from enums import KeyMode, PathDifficulty, ShareType, Variation, MultiplayerGameType
from seedbuilder.oriparse import ori_load_url
from seedbuilder.relics import relics

longform_to_code = {"Health": ["HC"], "Energy": ["EC"], "Ability": ["AC"], "Keystone": ["KS"], "Mapstone": ["MS"], "Free": []}
key_to_shards = {"GinsoKey": ["WaterVeinShard"] * 5, "ForlornKey": ["GumonSealShard"] * 5, "HoruKey": ["SunstoneShard"] * 5}

def ordhash(s):
    return reduce(mul, [ord(c) for c in s])

class Area:
    def __init__(self, name):
        self.name = name
        self.connections = []
        self.locations = []
        self.difficulty = 1
        self.has_location = False

    def add_connection(self, connection):
        self.connections.append(connection)

    def get_connections(self):
        return self.connections

    def remove_connection(self, connection):
        self.connections.remove(connection)

    def add_location(self, location):
        self.locations.append(location)
        self.has_location = True

    def get_locations(self):
        return self.locations

    def clear_locations(self):
        self.locations = []

    def remove_location(self, location):
        self.locations.remove(location)


class Connection:

    def __init__(self, home, target, sg):
        self.home = home
        self.target = target
        self.keys = 0
        self.mapstone = False
        self.requirements = []
        self.difficulties = []
        self.sg = sg

    def add_requirements(self, req, difficulty):
        def translate(req_part):
            """Helper function. Turns a req from areas.ori into
            the list of the things that req indicates are required"""
            if req_part in longform_to_code:
                return longform_to_code[req_part]
            if req_part in key_to_shards and self.sg.params.key_mode == KeyMode.SHARDS:
                return key_to_shards[req_part]
            if '=' not in req_part:
                return [req_part]
            item, _, count = req_part.partition("=")
            count = int(count)
            if item in longform_to_code:
                return count * longform_to_code[item]
            return count * [item]
        translated_req = []
        for part in req:
            translated_req += translate(part)

        self.requirements.append(translated_req)
        self.difficulties.append(difficulty)
        if not self.keys:
            self.keys = translated_req.count("KS")
        self.mapstone = "MS" in translated_req

    def get_requirements(self):
        return self.requirements

    def cost(self):
        minReqScore = 7777
        minDiff = 7777
        minReq = []
        for i in range(0, len(self.requirements)):
            score = 0
            items = {"EC": 0, "HC": 0, "AC": 0}
            for req_part in self.requirements[i]:
                if req_part in items:
                    items[req_part] += 1
                    if self.sg.inventory[req_part] < items[req_part]:
                        score += self.sg.costs[req_part]
                elif req_part == "MS":
                    if self.sg.inventory["MS"] < self.sg.mapstonesSeen:
                        score += self.sg.costs["MS"]
                else:
                    score += self.sg.costs.get(req_part, 0)
            if score < minReqScore:
                minReqScore = score
                minReq = self.requirements[i]
                minDiff = self.difficulties[i]
        return (minReqScore, minReq, minDiff)


class Location:
    factor = 4.0

    def __init__(self, x, y, area, orig, difficulty, zone):
        self.x = int(math.floor((x) / self.factor) * self.factor)
        self.y = int(math.floor((y) / self.factor) * self.factor)
        self.orig = orig
        self.area = area
        self.difficulty = difficulty
        self.zone = zone
        self.repeatable = orig[0:2] in ["EX", "EC", "HC", "AC", "MS"] or (orig.startswith("KS") and zone != "Misty")

    def get_key(self):
        return self.x * 10000 + self.y

    def __str__(self): return self.to_string()

    def to_string(self):
        return self.area + " " + self.orig + " (" + str(self.x) + " " + str(self.y) + ")"


class Door:
    factor = 4.0

    def __init__(self, name, x, y):
        self.x = x
        self.y = y
        self.name = name

    def get_key(self):
        return int(math.floor(self.x / self.factor) * self.factor) * 10000 + int(
            math.floor(self.y / self.factor) * self.factor)


class SeedGenerator:
    seedDifficultyMap = OrderedDict({"Dash": 2, "Bash": 2, "Glide": 3, "DoubleJump": 2, "ChargeJump": 1})

    # in order: 10 skills, then WV, Water, GS, Wind, Sunstone
    difficultyMap = OrderedDict({
        'casual-core': 1, 'casual-dboost': 1,
        'standard-core': 2, 'standard-dboost': 2, 'standard-lure': 2, 'standard-abilities': 2,
        'expert-core': 3, 'expert-dboost': 3, 'expert-lure': 3, 'expert-abilities': 3, 'dbash': 3,
        'master-core': 4, 'master-dboost': 4, 'master-lure': 4, 'master-abilities': 4, 'gjump': 4,
        'glitched': 5, 'timed-level': 5, 'insane': 7
    })
    skillsOutput = OrderedDict({
        "WallJump": "SK3", "ChargeFlame": "SK2", "Dash": "SK50", "Stomp": "SK4", "DoubleJump": "SK5",
        "Glide": "SK14", "Bash": "SK0", "Climb": "SK12", "Grenade": "SK51", "ChargeJump": "SK8"
    })
    eventsOutput = OrderedDict({
        "GinsoKey": "EV0", "Water": "EV1", "ForlornKey": "EV2", "Wind": "EV3", "HoruKey": "EV4", "Warmth": "EV5",
        "WaterVeinShard": "RB17", "GumonSealShard": "RB19", "SunstoneShard": "RB21"
    })

    def var(self, v):
        return v in self.params.variations

    def init_fields(self):
        """Part one of a reset. All initialization that doesn't
        require reading from params goes here."""
        self.limitKeysPool = [-3160308, -560160, 2919744, 719620, 7839588, 5320328, 8599904, -4600020, -6959592, -11880100, 5480952, 4999752, -7320236, -7200024, -5599400]

        self.costs = OrderedDict({
            "Free": 0, "MS": 0, "KS": 2, "AC": 12, "EC": 6, "HC": 12, "WallJump": 13,
            "ChargeFlame": 13, "DoubleJump": 13, "Bash": 31, "Stomp": 13,
            "Glide": 13, "Climb": 13, "ChargeJump": 23, "Dash": 13,
            "Grenade": 13, "GinsoKey": 12, "ForlornKey": 12, "HoruKey": 12,
            "Water": 31, "Wind": 31, "WaterVeinShard": 5, "GumonSealShard": 5,
            "SunstoneShard": 5, "TPForlorn": 67, "TPGrotto": 41,
            "TPSorrow": 59, "TPGrove": 41, "TPSwamp": 41, "TPValley": 53,
            "TPGinso": 61, "TPHoru": 71, "Open": 1, "OpenWorld": 1, "Relic": 1
        })
        self.inventory = OrderedDict([
            ("EX1", 0), ("EX*", 0), ("KS", 0), ("MS", 0), ("AC", 0), ("EC", 1),
            ("HC", 3), ("WallJump", 0), ("ChargeFlame", 0), ("Dash", 0),
            ("Stomp", 0), ("DoubleJump", 0), ("Glide", 0), ("Bash", 0),
            ("Climb", 0), ("Grenade", 0), ("ChargeJump", 0), ("GinsoKey", 0),
            ("ForlornKey", 0), ("HoruKey", 0), ("Water", 0), ("Wind", 0),
            ("Warmth", 0), ("RB0", 0), ("RB1", 0), ("RB6", 0), ("RB8", 0),
            ("RB9", 0), ("RB10", 0), ("RB11", 0), ("RB12", 0), ("RB13", 0),
            ("RB15", 0), ("WaterVeinShard", 0), ("GumonSealShard", 0),
            ("SunstoneShard", 0), ("TPForlorn", 0), ("TPGrotto", 0),
            ("TPSorrow", 0), ("TPGrove", 0), ("TPSwamp", 0), ("TPValley", 0),
            ("TPGinso", 0), ("TPHoru", 0), ("Open", 0), ("OpenWorld", 0), ("Relic", 0)
        ])

        self.mapstonesSeen = 1
        self.balanceLevel = 0
        self.balanceList = []
        self.balanceListLeftovers = []
        self.seedDifficulty = 0
        self.outputStr = ""
        self.eventList = []

        self.areas = OrderedDict()

        self.areasReached = OrderedDict([])
        self.currentAreas = []
        self.areasRemaining = []
        self.connectionQueue = []
        self.assignQueue = []
        self.spoiler = []

    def reset(self):
        """A full reset. Resets internal state completely (besides pRNG
        advancement), then sets initial values according to params."""
        self.init_fields()
        self.expRemaining = self.params.exp_pool
        self.forcedAssignments = self.preplaced.copy()
        self.forceAssignedLocs = set()
        self.itemPool = OrderedDict([
            ("EX1", 1), ("KS", 40), ("MS", 11), ("AC", 33),
            ("EC", 14), ("HC", 12), ("WallJump", 1), ("ChargeFlame", 1),
            ("Dash", 1), ("Stomp", 1), ("DoubleJump", 1), ("Glide", 1),
            ("Bash", 1), ("Climb", 1), ("Grenade", 1), ("ChargeJump", 1),
            ("GinsoKey", 1), ("ForlornKey", 1), ("HoruKey", 1), ("Water", 1),
            ("Wind", 1), ("Warmth", 1), ("RB0", 3), ("RB1", 3), ("RB6", 3),
            ("RB8", 0), ("RB9", 1), ("RB10", 1), ("RB11", 1), ("RB12", 1),
            ("RB13", 3), ("RB15", 3), ("WaterVeinShard", 0),
            ("GumonSealShard", 0), ("SunstoneShard", 0), ("TPForlorn", 1),
            ("TPGrotto", 1), ("TPSorrow", 1), ("TPGrove", 1), ("TPSwamp", 1),
            ("TPValley", 1), ("TPGinso", 0), ("TPHoru", 0), ("Open", 0), ("OpenWorld", 0), ("Relic", 0)
        ])
        if not self.var(Variation.CLOSED_DUNGEONS):
            self.inventory["Open"] = 1
            self.costs["Open"] = 0
            self.itemPool["TPGinso"] = 1
            self.itemPool["TPHoru"] = 1

        if self.var(Variation.OPEN_WORLD):
            self.inventory["OpenWorld"] = 1
            self.costs["OpenWorld"] = 0
            self.itemPool["KS"] -= 2

        if not self.var(Variation.STRICT_MAPSTONES):
            self.costs["MS"] = 11

        if self.var(Variation.HARDMODE):
            self.itemPool["AC"] = 0
            self.itemPool["HC"] = 0
            self.itemPool["EC"] = 3
            for bonus in [k for k in self.itemPool.keys() if k[:2] == "RB"]:
                del self.itemPool[bonus]

        if self.var(Variation.EXTRA_BONUS_PICKUPS):
            self.itemPool["RB6"] += 2
            self.itemPool["RB31"] = 1
            self.itemPool["RB32"] = 1
            self.itemPool["RB33"] = 3
            self.itemPool["RB36"] = 1
            self.itemPool["RB12"] += 4
            for bonus_skill in self.random.sample(["RB101", "RB102", "RB103", "WarpSkill", "RB106", "RB107"], 4):
                if bonus_skill == "WarpSkill":
                    bonus_skill = self.random.choice(["RB104", "RB105"])
                self.itemPool[bonus_skill] = 1

        if self.params.key_mode == KeyMode.SHARDS:
            shard_count = 5
            if self.params.sync.mode == MultiplayerGameType.SPLITSHARDS:
                shard_count = 2 + 3*(self.params.players)
            for shard in ["WaterVeinShard", "GumonSealShard", "SunstoneShard"]:
                self.itemPool[shard] = shard_count
                self.costs[shard] = shard_count
            for key in ["GinsoKey", "HoruKey", "ForlornKey"]:
                self.itemPool[key] = 0

        if self.var(Variation.DOUBLE_SKILL):
            self.itemPool["DoubleJump"] += 1
            self.itemPool["Bash"] += 1
            self.itemPool["Stomp"] += 1
            self.itemPool["Glide"] += 1
            self.itemPool["ChargeJump"] += 1
            self.itemPool["Dash"] += 1
            self.itemPool["Grenade"] += 1
            self.itemPool["Water"] += 1
            self.itemPool["Wind"] += 1

        if self.var(Variation.WARMTH_FRAGMENTS):
            self.costs["RB28"] = 3 * self.params.frag_count
            self.inventory["RB28"] = 0
            self.itemPool["RB28"] = self.params.frag_count
            self.itemPool["Warmth"] = 0

        if self.params.key_mode == KeyMode.FREE:
            for key in ["GinsoKey", "HoruKey", "ForlornKey"]:
                self.costs[key] = 0
                self.itemPool[key] = 0
                self.inventory[key] = 1

        if self.params.key_mode == KeyMode.LIMITKEYS:
            dungeonLocs = {"GinsoKey": {5480952, 5320328}, "ForlornKey": {-7320236}, "HoruKey": set()}
            names = {-3160308: "SKWallJump", -560160: "SKChargeFlame", 2919744: "SKDash", 719620: "SKGrenade", 7839588: "SKDoubleJump", 5320328: "SKBash", 8599904: "SKStomp", -4600020: "SKGlide",
                    -6959592: "SKChargeJump", -11880100: "SKClimb", 5480952: "EVWater", 4999752: "EVGinsoKey", -7320236: "EVWind", -7200024: "EVForlornKey", -5599400: "EVHoruKey"}
            for key in self.random.sample(dungeonLocs.keys(), 3):
                loc = self.random.choice([l for l in self.limitKeysPool if l not in dungeonLocs[key]])
                self.limitKeysPool.remove(loc)
                for key_to_update in ["GinsoKey", "ForlornKey"]:
                    if loc in dungeonLocs[key_to_update]:
                        dungeonLocs[key_to_update] |= dungeonLocs[key]
                self.forcedAssignments[loc] = key

        # paired setup for subsequent players
        self.unplaced_shared = 0
        if self.playerID > 1:
            for item in self.sharedList:
                self.itemPool[item] = 0
            self.unplaced_shared = self.sharedCounts[self.playerID]

    def __init__(self):
        self.init_fields()
        self.codeToName = OrderedDict([(v, k) for k, v in self.skillsOutput.items() + self.eventsOutput.items()])

    def reach_area(self, target):
        if self.playerID > 1 and target in self.sharedMap:
            for item, player in self.sharedMap[target]:
                if player == self.playerID:
                    self.assignQueue.append(item)
                    self.unplaced_shared -= 1
                else:
                    self.assign(item)
                    self.spoilerGroup[item].append(item + " from Player " + str(player) + "\n")
        if self.areas[target].has_location:
            self.currentAreas.append(target)
        self.areasReached[target] = True

    def open_free_connections(self):
        found = False
        keystoneCount = 0
        mapstoneCount = 0
        # python 3 wont allow concurrent changes
        # list(areasReached.keys()) is a copy of the original list
        for area in list(self.areasReached.keys()):
            for connection in self.areas[area].get_connections():
                cost = connection.cost()
                reached = connection.target in self.areasReached
                if cost[0] <= 0:
                    if not reached:
                        self.areas[connection.target].difficulty = cost[2]
                        if len(self.areas[connection.target].locations) > 0:
                            self.areas[connection.target].difficulty += self.areas[area].difficulty
                    if connection.keys > 0:
                        if area not in self.doorQueue.keys():
                            self.doorQueue[area] = connection
                            keystoneCount += connection.keys
                    elif connection.mapstone and self.var(Variation.STRICT_MAPSTONES):
                        if not reached:
                            visitMap = True
                            for mp in self.mapQueue.keys():
                                if mp == area or self.mapQueue[mp].target == connection.target:
                                    visitMap = False
                            if visitMap:
                                self.mapQueue[area] = connection
                                mapstoneCount += 1
                    else:
                        if not reached:
                            self.seedDifficulty += cost[2] * cost[2]
                            self.reach_area(connection.target)
                            if connection.mapstone and not self.var(Variation.STRICT_MAPSTONES):
                                self.mapstonesSeen += 1
                                if self.mapstonesSeen >= 9:
                                    self.mapstonesSeen = 11
                                if self.mapstonesSeen == 8:
                                    self.mapstonesSeen = 9
                        if connection.target in self.areasRemaining:
                            self.areasRemaining.remove(connection.target)
                        self.connectionQueue.append((area, connection))
                        found = True
        return (found, keystoneCount, mapstoneCount)

    def choose_relic_for_zone(self, zone):
        self.random.shuffle(relics[zone])
        return relics[zone][0]

    def get_all_accessible_locations(self):
        locations = []
        forced_placement = False
        for area in self.areasReached.keys():
            currentLocations = self.areas[area].get_locations()
            for location in currentLocations:
                location.difficulty += self.areas[area].difficulty
            if self.forcedAssignments:
                reachable_forced_ass_locs = [l for l in currentLocations if l.get_key() in self.forcedAssignments]
                for loc in reachable_forced_ass_locs:
                    currentLocations.remove(loc)
                    key = loc.get_key()
                    if key in self.forceAssignedLocs:
                        continue
                    self.forceAssignedLocs.add(key)
                    self.force_assign(self.forcedAssignments[key], loc)
                    forced_placement = True
            locations.extend(currentLocations)
            self.areas[area].clear_locations()
        if self.reservedLocations:
            locations.append(self.reservedLocations.pop(0))
            locations.append(self.reservedLocations.pop(0))
        if self.locations() > 2 and len(locations) >= 2:
            self.reservedLocations.append(locations.pop(self.random.randrange(len(locations))))
            self.reservedLocations.append(locations.pop(self.random.randrange(len(locations))))
        return locations, forced_placement

    def prepare_path(self, free_space):
        abilities_to_open = OrderedDict()
        totalCost = 0.0
        free_space += len(self.balanceList)
        # find the sets of abilities we need to get somewhere
        for area in self.areasReached.keys():
            for connection in self.areas[area].get_connections():
                if connection.target in self.areasReached:
                    continue
                req_sets = connection.get_requirements()
                for req_set in req_sets:
                    requirements = []
                    cost = 0
                    cnts = defaultdict(lambda: 0)
                    for req in req_set:
                        if not req:
                            log.warning(req, req_set, str(connection), connection.target)
                            continue
                        if self.costs[req] > 0:
                            # if the item isn't in your itemPool (due to co-op or an unprocessed forced assignment), skip it
                            if self.itemPool[req] == 0:
                                requirements = []
                                break
                            if req in ["HC", "EC", "WaterVeinShard", "GumonSealShard", "SunstoneShard"]:
                                cnts[req] += 1
                                if cnts[req] > self.inventory[req]:
                                    requirements.append(req)
                                    cost += self.costs[req]
                            else:
                                requirements.append(req)
                                cost += self.costs[req]
                    # decrease the rate of multi-ability paths
                    cost *= max(1, len(requirements) - 1)
                    if len(requirements) <= free_space:
                        for req in requirements:
                            if req not in abilities_to_open:
                                abilities_to_open[req] = (cost, requirements)
                            elif abilities_to_open[req][0] > cost:
                                abilities_to_open[req] = (cost, requirements)
        # pick a random path weighted by cost
        for path in abilities_to_open:
            totalCost += 1.0 / abilities_to_open[path][0]
        position = 0
        target = self.random.random() * totalCost
        path_selected = None
        for path in abilities_to_open:
            position += 1.0 / abilities_to_open[path][0]
            if target <= position:
                path_selected = abilities_to_open[path]
                break
        # if a connection will open with a subset of skills in the selected path, use that instead
        subsetCheck = abilities_to_open.keys()
        self.random.shuffle(subsetCheck)
        for path in subsetCheck:
            isSubset = abilities_to_open[path][0] < path_selected[0]
            if isSubset:
                for req in abilities_to_open[path][1]:
                    if req not in path_selected[1]:
                        isSubset = False
                        break
                if isSubset:
                    path_selected = abilities_to_open[path]
        if path_selected:
            for req in path_selected[1]:
                if self.itemPool[req] > 0:
                    self.assignQueue.append(req)
            return path_selected[1]
        return None

    def get_location_from_balance_list(self):
        target = int(pow(self.random.random(), 1.0 / self.balanceLevel) * len(self.balanceList))
        location = self.balanceList.pop(target)
        self.balanceListLeftovers.append(location[0])
        return location[1]

    def cloned_item(self, item, player):
        name = self.codeToName.get(item, item)  # TODO: get upgrade names lol
        if item in self.skillsOutput:
            item = self.skillsOutput[item]
        if item in self.eventsOutput:
            item = self.eventsOutput[item]
        if not self.params.sync.hints:
            return "EV5"
        hint_text = {"SK": "Skill", "TP": "Teleporter", "RB": "Upgrade", "EV": "World Event"}.get(item[:2], "?Unknown?")
        if item in ["RB17", "RB19", "RB21"]:
            name = "a " + name  # grammar
            hint_text = "Shard"
        elif item == "RB28":
            name = "a Warmth Fragment"
            hint_text = "Warmth Fragment"
        elif item == "Relic":
            name = "a Relic"
            hint_text = "Relic"

        owner = ("Team " if self.params.sync.teams else "Player ") + str(player)
        msg = "HN%s-%s-%s" % (name, hint_text, owner)
        return msg

    def assign_random(self, recurseCount=0):
        value = self.random.random()
        position = 0.0
        denom = float(sum(self.itemPool.values()))
        if denom == 0.0:
            log.warning("%s: itemPool was empty! locations: %s, balanced items: %s", self.params.flag_line(), self.locations(), self.items() - self.items(False))
            return self.assign("EX*")
        for key in self.itemPool.keys():
            position += self.itemPool[key] / denom
            if value <= position:
                if self.var(Variation.STARVED) and key in self.skillsOutput and recurseCount < 3:
                    return self.assign_random(recurseCount=recurseCount + 1)
                return self.assign(key)

    def assign(self, item, preplaced=False):
        if item[0:2]  in ["MU", "RP"]:
            for multi_item in self.get_multi_items(item):
                self.assign(multi_item, preplaced)
        else:
            if not preplaced:
                self.itemPool[item] = max(self.itemPool[item] - 1, 0) if item in self.itemPool else 0
            if item in ["KS", "EC", "HC", "AC", "WaterVeinShard", "GumonSealShard", "SunstoneShard"]:
                if self.costs[item] > 0:
                    self.costs[item] -= 1
            elif item == "RB28":
                if self.costs[item] > 0:
                    self.costs[item] -= min(3, self.costs[item])
            elif item in self.costs and self.itemPool[item] == 0:
                self.costs[item] = 0
            self.inventory[item] = 1 + self.inventory.get(item, 0)
        return item

    # for use in limitkeys mode
    def force_assign(self, item, location):
        self.assign(item, True)
        self.assign_to_location(item, location)

    # for use in world tour mode
    # TODO: replace this with generalized preplacement
    def relic_assign(self, location):
        self.force_assign("Relic", location)
        self.areas[location.area].remove_location(location)

    def assign_to_location(self, item, location):
        zone = location.zone
        hist_written = False
        at_mapstone = not self.var(Variation.DISCRETE_MAPSTONES) and location.orig == "MapStone"
        has_cost = item in self.costs.keys()

        loc = location.get_key()

        if at_mapstone:
            self.mapstonesAssigned += 1
            loc = 20 + self.mapstonesAssigned * 4
            zone = "Mapstone"

        # if this is the first player of a paired seed, construct the map
        if self.playerCount > 1 and self.playerID == 1 and item in self.sharedList:
            player = self.random.randint(1, self.playerCount)
            if self.params.sync.cloned:
                # handle cloned seed placement
                adjusted_item = self.adjust_item(item, zone)
                self.split_locs[loc] = (player, adjusted_item)
                self.spoilerGroup[item].append("%s from Player %s at %s\n" % (item, player, location.to_string()))
                hist_written = True
                if player != self.playerID:
                    item = self.cloned_item(item, player=player)
            else:
                if location.area not in self.sharedMap:
                    self.sharedMap[location.area] = []
                self.sharedMap[location.area].append((item, player))
                if player != self.playerID:
                    self.sharedCounts[player] += 1
                    self.spoilerGroup[item].append("%s from Player %s\n" % (item, player))
                    item = "EX*"
                    self.expSlots += 1
        # if mapstones are progressive, set a special location

        if has_cost and not hist_written:
            if at_mapstone:
                self.spoilerGroup[item].append(item + " from MapStone " + str(self.mapstonesAssigned) + "\n")
            else:
                self.spoilerGroup[item].append(item + " from " + location.to_string() + "\n")

        fixed_item = self.adjust_item(item, zone)
        assignment = self.get_assignment(loc, fixed_item, zone)

        if item in self.eventsOutput:
            self.eventList.append(assignment)
        elif self.params.balanced and not has_cost and location.orig != "MapStone" and loc not in self.forceAssignedLocs:
            self.balanceList.append((fixed_item, location, assignment))
        else:
            self.outputStr += assignment

        if self.params.do_loc_analysis:
            key = location.to_string()
            if location.orig == "MapStone":
                key = "MapStone " + str(self.mapstonesAssigned)
            if item in self.params.locationAnalysisCopy[key]:
                self.params.locationAnalysisCopy[key][item] += 1
                self.params.locationAnalysisCopy[location.zone][item] += 1

    def adjust_item(self, item, zone):
        if item in self.skillsOutput:
            item = self.skillsOutput[item]
        elif item in self.eventsOutput:
            item = self.eventsOutput[item]
        elif item == "Relic":
            relic = self.choose_relic_for_zone(zone)
            item = "WT#" + relic[0] + "#\\n" + relic[1]
        elif item == "EX*":
            value = self.get_random_exp_value()
            self.expRemaining -= value
            self.expSlots -= 1
            item = "EX%s" % value
        return item

    def get_assignment(self, loc, item, zone):
        pickup = ""
        if item[2:]:
            pickup = "%s|%s" % (item[:2], item[2:])
        else:
            pickup = "%s|1" % item[:2]
        return "%s|%s|%s\n" % (loc, pickup, zone)

    def get_random_exp_value(self):
        minExp = self.random.randint(2, 9)
        if self.expSlots <= 1:
            return max(self.expRemaining, minExp)
        return int(max(self.expRemaining * (self.inventory["EX*"] + self.expSlots / 4) * self.random.uniform(0.0, 2.0) / (self.expSlots * (self.expSlots + self.inventory["EX*"])), minExp))

    def preferred_difficulty_assign(self, item, locationsToAssign):
        total = 0.0
        for loc in locationsToAssign:
            if self.params.path_diff == PathDifficulty.EASY:
                total += (20 - loc.difficulty) * (20 - loc.difficulty)
            else:
                total += (loc.difficulty * loc.difficulty)
        value = self.random.random()
        position = 0.0
        for i in range(0, len(locationsToAssign)):
            if self.params.path_diff == PathDifficulty.EASY:
                position += (20 - locationsToAssign[i].difficulty) * (20 - locationsToAssign[i].difficulty) / total
            else:
                position += locationsToAssign[i].difficulty * locationsToAssign[i].difficulty / total
            if value <= position:
                self.assign_to_location(item, locationsToAssign[i])
                break
        del locationsToAssign[i]

    def form_areas(self):
        if self.params.do_loc_analysis:
            self.params.locationAnalysis["FinalEscape EVWarmth (-240 512)"] = self.params.itemsToAnalyze.copy()
            self.params.locationAnalysis["FinalEscape EVWarmth (-240 512)"]["Zone"] = "Horu"

        # sorry for this - only intended to last as long as 3.0 beta lasts
        areas_dot_ori = 'http://raw.githubusercontent.com/sigmasin/OriDERandomizer/3.0/seed_gen/areas.ori'
        meta = ori_load_url(areas_dot_ori)
        logic_paths = [lp.value for lp in self.params.logic_paths]
        for loc_name, loc_info in meta["locs"].iteritems():
            area = Area(loc_name)
            self.areasRemaining.append(loc_name)
            self.areas[loc_name] = area

            loc = Location(
                int(loc_info["x"]),
                int(loc_info["y"]),
                loc_name,
                loc_info["item"],
                int(loc_info["difficulty"]),
                loc_info["zone"]
            )
            area.add_location(loc)

            if self.params.do_loc_analysis:
                key = loc.to_string()
                if key not in self.params.locationAnalysis:
                    self.params.locationAnalysis[key] = self.params.itemsToAnalyze.copy()
                    self.params.locationAnalysis[key]["Zone"] = loc.zone
                zoneKey = loc.zone
                if zoneKey not in self.params.locationAnalysis:
                    self.params.locationAnalysis[zoneKey] = self.params.itemsToAnalyze.copy()
                    self.params.locationAnalysis[zoneKey]["Zone"] = loc.zone

        for home_name, home_info in meta["homes"].iteritems():
            area = Area(home_name)
            self.areasRemaining.append(home_name)
            self.areas[home_name] = area

            for conn_target_name, conn_info in home_info["conns"].iteritems():
                connection = Connection(home_name, conn_target_name, self)

                # can't actually be used yet but this is roughly how this will be implemented
                # entranceConnection = True if "entrance" in conn_info else False
                # if self.var(Variation.ENTRANCE_SHUFFLE) and entranceConnection:
                #   continue

                if not conn_info["paths"]:
                    connection.add_requirements(["Free"], 1)
                for path in conn_info["paths"]:
                    if path[0] in logic_paths:
                        connection.add_requirements(list(path[1:]), self.difficultyMap[path[0]])
                if connection.get_requirements():
                    area.add_connection(connection)

    def connect_doors(self, door1, door2, requirements=["Free"]):
        connection1 = Connection(door1.name, door2.name, self)
        connection1.add_requirements(requirements, 1)
        self.areas[door1.name].add_connection(connection1)
        connection2 = Connection(door2.name, door1.name, self)
        connection2.add_requirements(requirements, 1)
        self.areas[door2.name].add_connection(connection2)
        return str(door1.get_key()) + "|EN|" + str(door2.x) + "|" + str(door2.y) + "\n" + str(door2.get_key()) + "|EN|" + str(door1.x) + "|" + str(door1.y) + "\n"

    def randomize_entrances(self):
        tree = XML.parse("seedbuilder/doors.xml")
        root = tree.getroot()

        outerDoors = [[], [], [], [], [], [], [], [], [], [], [], [], []]
        innerDoors = [[], [], [], [], [], [], [], [], [], [], [], [], []]

        for child in root:
            inner = child.find("Inner")
            innerDoors[int(inner.find("Group").text)].append(Door(child.attrib["name"] + "InnerDoor", int(inner.find("X").text), int(inner.find("Y").text)))

            outer = child.find("Outer")
            outerDoors[int(outer.find("Group").text)].append(Door(child.attrib["name"] + "OuterDoor", int(outer.find("X").text), int(outer.find("Y").text)))

        self.random.shuffle(outerDoors[0])
        self.random.shuffle(innerDoors[12])

        firstDoors = []
        lastDoors = []

        firstDoors.append(outerDoors[0].pop(0))
        firstDoors.append(outerDoors[0].pop(0))

        lastDoors.append(innerDoors[12].pop(0))
        lastDoors.append(innerDoors[12].pop(0))

        doorStr = ""

        # activeGroups = [0, 1, 2]
        # targets = [3, 4, 5, 6, 7, 8, 9, 10, 12]
        # for now, make R1 vanilla

        doorStr += self.connect_doors(outerDoors[2].pop(0), innerDoors[7].pop(0))

        activeGroups = [0, 1, 8]
        targets = [3, 4, 5, 6, 8, 9, 10, 12]

        self.random.shuffle(targets)

        horuEntryGroup = self.random.randint(4, 9)
        if horuEntryGroup >= 7:
            horuEntryGroup += 2
        if horuEntryGroup == 11:
            horuEntryGroup = 1
            if self.random.random() > 0.5:
                doorStr += self.connect_doors(
                    firstDoors[0], innerDoors[1].pop(0))
                outerDoors[0].append(firstDoors[1])
            else:
                doorStr += self.connect_doors(
                    firstDoors[0], outerDoors[1].pop(0))
                outerDoors[0].append(firstDoors[1])
                outerDoors[0].append(innerDoors[1].pop(0))
        else:
            requirements = ["Free"]
            if firstDoors[1].name == "GinsoDoorOuter":
                requirements = ["GinsoKey"]
            if firstDoors[1].name == "ForlornDoorOuter":
                requirements = ["ForlornKey"]
            doorStr += self.connect_doors(
                firstDoors[0], outerDoors[horuEntryGroup].pop(0), requirements)
            doorStr += self.connect_doors(firstDoors[1], innerDoors[horuEntryGroup - 1].pop(0))
            targets.remove(horuEntryGroup - 1)

        while len(targets) > 0:
            index = self.random.randrange(len(activeGroups))
            group = activeGroups[index]
            if not outerDoors[group]:
                del activeGroups[index]
                continue

            target = targets[0]
            if not innerDoors[target]:
                del targets[0]
                continue

            if target < 12:
                activeGroups.append(target + 1)

            if (target == 6 and 10 not in targets) or (target == 10 and 6 not in targets):
                activeGroups.append(12)

            doorStr += self.connect_doors(
                outerDoors[group].pop(0), innerDoors[target].pop(0))

        lastDoorIndex = 0

        for group in range(13):
            if innerDoors[group]:
                doorStr += self.connect_doors(
                    innerDoors[group].pop(0), lastDoors[lastDoorIndex])
                lastDoorIndex += 1
            if outerDoors[group]:
                doorStr += self.connect_doors(
                    outerDoors[group].pop(0), lastDoors[lastDoorIndex])
                lastDoorIndex += 1

        return doorStr

    def setSeedAndPlaceItems(self, params, preplaced={}, retries=10, verbose_paths=False):
        self.verbose_paths = verbose_paths
        self.params = params

        self.sharedList = []
        self.random = random.Random()
        self.random.seed(self.params.seed)
        self.preplaced = {k: self.codeToName.get(v, v) for k, v in preplaced.iteritems()}
        self.do_multi = self.params.sync.enabled and self.params.sync.mode == MultiplayerGameType.SHARED

        if self.var(Variation.WORLD_TOUR):
            self.relicZones = self.random.sample(["Glades", "Grove", "Grotto", "Blackroot", "Swamp", "Ginso", "Valley", "Misty", "Forlorn", "Sorrow", "Horu"], self.params.relic_count)

        self.playerCount = 1
        if self.do_multi:
            if self.params.sync.teams:
                self.playerCount = len(self.params.sync.teams)
            else:
                self.playerCount = self.params.players

        if self.do_multi:
            shared = self.params.sync.shared
            if ShareType.SKILL in shared:
                self.sharedList += ["WallJump", "ChargeFlame", "Dash", "Stomp", "DoubleJump", "Glide", "Bash", "Climb", "Grenade", "ChargeJump"]
            if ShareType.EVENT in shared:
                if self.params.key_mode == KeyMode.SHARDS:
                    self.sharedList += ["WaterVeinShard", "GumonSealShard", "SunstoneShard"]
                else:
                    self.sharedList += ["GinsoKey", "ForlornKey", "HoruKey"]
                self.sharedList += ["Water", "Wind", "Warmth"]
            if ShareType.TELEPORTER in shared:
                self.sharedList += ["TPForlorn", "TPGrotto", "TPSorrow", "TPGrove", "TPSwamp", "TPValley", "TPGinso", "TPHoru"]
            if ShareType.UPGRADE in self.params.sync.shared:
                self.sharedList += ["RB6", "RB8", "RB9", "RB10", "RB11", "RB12", "RB13", "RB15"]
                if self.var(Variation.EXTRA_BONUS_PICKUPS):
                    self.sharedList += ["RB31", "RB32", "RB33", "RB101", "RB102", "RB103"]
            if ShareType.MISC in shared:
                if self.var(Variation.WARMTH_FRAGMENTS):
                    self.sharedList.append("RB28")
                # TODO: figure out relic sharing
                # if self.var(Variation.WORLD_TOUR):
                #      self.sharedList.append("Relic")
        return self.placeItemsMulti(retries)

    def placeItemsMulti(self, retries=5):
        placements = []
        self.sharedMap = {}
        self.sharedCounts = Counter()
        self.split_locs = {}
        self.playerID = 1

        placement = self.placeItems(0)
        if not placement:
            if retries > 0:
                retries -= 1
            else:
                log.error("""Seed not completeable with these params and placements.
                            ItemCount: %s
                            Items remaining: %s,
                            Areas reached: %s,
                            AreasRemaining: %s,
                            Inventory: %s,
                            Forced Assignments: %s,
                            Nonzero Costs:%s,
                            Partial Seed: %s""",
                            self.locations(),
                            {k: v for k, v in self.itemPool.iteritems() if v > 0},
                            [x for x in self.areasReached],
                            [x for x in self.areasRemaining],
                            {k: v for k, v in self.inventory.iteritems() if v != 0},
                            self.forcedAssignments,
                            {k: v for k, v in self.costs.iteritems() if v != 0},
                            self.outputStr)
                return None
            return self.placeItemsMulti(retries)
        placements.append(placement)
        if self.params.sync.cloned:
            lines = placement[0].split("\n")
            spoiler = placement[1]
        while self.playerID < self.playerCount:
            self.playerID += 1
            if self.params.sync.cloned:
                outlines = [lines[0]]
                for line in lines[1:-1]:
                    loc, _, _, zone = tuple(line.split("|"))
                    if int(loc) in self.split_locs:
                        player, split_item = self.split_locs[int(loc)]
                        if self.playerID != player:  # theirs
                            hint = self.cloned_item(split_item, player)
                            outlines.append("|".join([loc, hint[:2], hint[2:], zone]))
                        else:  # ours
                            outlines.append("|".join([loc, split_item[:2], split_item[2:], zone]))
                    else:
                        outlines.append(line)
                placements.append(("\n".join(outlines) + "\n", spoiler))
            else:
                placement = self.placeItems(0)
                if not placement:
                    if retries > 0:
                        retries -= 1
                    else:
                        log.error("Coop seed not completeable with these params and placements")
                        return None
                    return self.placeItemsMulti(retries)
                placements.append(placement)

        return placements

    def locations(self):
        """Number of remaining locations that can have items in them"""
        remaining_fass = len(self.forcedAssignments) - len(self.forceAssignedLocs)
        return sum([len(area.locations) for area in self.areas.values()]) - remaining_fass + len(self.reservedLocations)

    def items(self, include_balanced=True):
        """Number of items left to place"""
        balanced = len(self.balanceListLeftovers) if include_balanced else 0
        return sum([v for v in self.itemPool.values()]) + balanced + self.unplaced_shared

    def placeItems(self, depth=0):
        self.reset()
        keystoneCount = 0
        mapstoneCount = 0

        self.form_areas()

        if self.params.do_loc_analysis:
            self.params.locationAnalysisCopy = {}
            for location in self.params.locationAnalysis:
                self.params.locationAnalysisCopy[location] = {}
                for item in self.params.locationAnalysis[location]:
                    self.params.locationAnalysisCopy[location][item] = self.params.locationAnalysis[location][item]

        # flags line
        self.outputStr += (self.params.flag_line(self.verbose_paths) + "\n")

        self.spoilerGroup = defaultdict(list)

        if self.var(Variation.ENTRANCE_SHUFFLE):
            self.outputStr += self.randomize_entrances()

        if self.var(Variation.WORLD_TOUR):
            locations_by_zone = OrderedDict({zone: [] for zone in self.relicZones})
            for area in self.areas.values():
                for location in area.locations:
                    if location.zone in locations_by_zone:
                        locations_by_zone[location.zone].append(location)

            for locations in locations_by_zone.values():
                self.random.shuffle(locations)

                relic_loc = None

                while not relic_loc and len(locations):
                    next_loc = locations.pop()
                    # Can't put a relic on a map turn-in
                    if next_loc.orig == "MapStone":
                        continue
                    # Can't put a relic on a reserved preplacement location
                    # TODO: re-impl relics via preplacement
                    if next_loc.get_key() in self.forcedAssignments:
                        continue
                    relic_loc = next_loc
                self.relic_assign(relic_loc)
            # Capture relic spoilers before the spoiler group is overwritten
            relicSpoiler = self.spoilerGroup["Relic"]

        if self.var(Variation.EXTRA_BONUS_PICKUPS):
            warps = self.random.randint(4,8)
            warp_locs = []
            for area in self.areas.values():
                for loc in area.locations:
                    if loc.repeatable:
                        warp_locs.append(loc.get_key())
            warp_targets = [
                (915, -115),    # stomp miniboss
                (790, -195),   # below swamp swim
                (561, -410),   # grotto miniboss
                (585, -68),    # outer swamp health cell
                (510, 910),    # top of ginso escape
                (499, -505),   # lost grove laser lever
                (480, -252),   # moon grotto, just below the bridge
                (417, -435),   # lower blackroot right lasers
                (330, -63),    # Hollow Grove main AC
                (127, 20),     # Horu Fields Plant
                (-13, -96),    # Above cflame tree exp
                (-224, -85),   # Valley entry (upper)
                (-358, 65),    # Stompless AC
                (-500, 587),   # Top of sorrow
                (-570, 156),   # Wilhelm exp
                (-605, -255),  # Forlorn enterance
            ]
            for loc, target in zip(self.random.sample(warp_locs, warps), self.random.sample(warp_targets, warps)):
                self.forcedAssignments[loc] = "RPSH/Warp/WP/%s,%s" % target

        # handle the fixed pickups: first energy cell, the glitchy 100 orb at spirit tree, and the forlorn escape plant
        for loc, item, zone in [(-280256, "EC1", "Glades"), (-1680104, "EX100", "Grove"), (-12320248, "RB81", "Forlorn")]:
            if loc in self.forcedAssignments:
                item = self.forcedAssignments[loc]
                del self.forcedAssignments[loc]  # don't count these ones
            if item not in ["EX100", "EC1", "RB81"] and item not in self.itemPool:
                log.warning("Preplaced item %s was not in pool. Translation may be necessary." % item)
            ass = self.get_assignment(loc, self.adjust_item(item, zone), zone)
            self.outputStr += ass

        if 2 in self.forcedAssignments:
            item = self.forcedAssignments[2]
            self.assign(item)
            del self.forcedAssignments[2]
            ass = self.get_assignment(2, self.adjust_item(item, "Glades"), "Glades")
            if self.params.key_mode == KeyMode.FREE:
                splitAss = ass.split("|")
                if splitAss[1] in ["MU", "RP"]:
                    splitAss[2] = "EV/0/EV/2/EV/4/%s" % splitAss[2]
                else:
                    splitAss[2] = "EV/0/EV/2/EV/4/%s/%s" % (splitAss[1], splitAss[2])
                splitAss[1] = "MU"
                ass = "|".join(splitAss)
            self.outputStr += ass
        elif self.params.key_mode == KeyMode.FREE:
            self.outputStr += "2|MU|EV/0/EV/2/EV/4|Glades\n"
            

        for v in self.forcedAssignments.values():
            if v[0:2] in ["MU", "RP"]:
                for item in self.get_multi_items(v):
                    if item in self.itemPool:
                        self.itemPool[item] -= 1
            if v in self.itemPool:
                self.itemPool[v] -= 1

        locationsToAssign = []
        self.connectionQueue = []
        self.reservedLocations = []

        self.skillCount = 10
        self.mapstonesAssigned = 0

        self.doorQueue = OrderedDict()
        self.mapQueue = OrderedDict()
        spoilerPath = []

        self.reach_area("SunkenGladesRunaway")
        if self.var(Variation.OPEN_WORLD):
            self.reach_area("GladesMain")
            for connection in list(self.areas["SunkenGladesRunaway"].connections):
                if connection.target == "GladesMain":
                    self.areas["SunkenGladesRunaway"].remove_connection(connection)

        self.itemPool["EX*"] = self.locations() - sum([v for v in self.itemPool.values()]) - self.unplaced_shared + 1  # add 1 for warmth returned (:
        self.expSlots = self.itemPool["EX*"]

        while self.locations() > 0:
            if self.locations() != self.items() - 1:
                log.warning("Item (%s) /Location (%s) desync!", self.items(), self.locations())
            self.balanceLevel += 1
            # open all paths that we can already access
            opening = True
            while opening:
                (opening, keys, mapstones) = self.open_free_connections()
                keystoneCount += keys
                mapstoneCount += mapstones
                if mapstoneCount >= 9:
                    mapstoneCount = 11
                if mapstoneCount == 8:
                    mapstoneCount = 9
                for connection in self.connectionQueue:
                    self.areas[connection[0]].remove_connection(connection[1])
                self.connectionQueue = []
            reset_loop = False
            locationsToAssign, reset_loop = self.get_all_accessible_locations()

            # if there aren't any doors to open, it's time to get a new skill
            # consider -- work on stronger anti-key-lock logic so that we don't
            # have to give keys out right away (this opens up the potential of
            # using keys in the wrong place, will need to be careful)
            if not (self.doorQueue and self.inventory["KS"] >= keystoneCount) and not (self.mapQueue and self.inventory["MS"] >= mapstoneCount) and not reset_loop and len(locationsToAssign) == 0:
                if self.reservedLocations:
                    locationsToAssign.append(self.reservedLocations.pop(0))
                    locationsToAssign.append(self.reservedLocations.pop(0))
                spoilerPath = self.prepare_path(len(locationsToAssign) + len(self.balanceList))
                if self.params.balanced:
                    for item in self.assignQueue:
                        if len(self.balanceList) == 0:
                            break
                        locationsToAssign.append(self.get_location_from_balance_list())
                if not self.assignQueue:
                    # we've painted ourselves into a corner, try again
                    if self.playerID == 1:
                        self.split_locs = {}
                        self.sharedMap = {}
                        self.sharedCounts = Counter()
                    if depth > self.playerCount * self.playerCount:
                        return
                    return self.placeItems(depth + 1)

            # pick what we're going to put in our accessible space
            itemsToAssign = []
            if len(locationsToAssign) < len(self.assignQueue) + max(keystoneCount - self.inventory["KS"], 0) + max(mapstoneCount - self.inventory["MS"], 0):
                # we've painted ourselves into a corner, try again
                if not self.reservedLocations:
                    if self.playerID == 1:
                        self.split_locs = {}
                        self.sharedMap = {}
                        self.sharedCounts = Counter()
                    if depth > self.playerCount * self.playerCount:
                        return
                    return self.placeItems(depth + 1)
                locationsToAssign.append(self.reservedLocations.pop(0))
                locationsToAssign.append(self.reservedLocations.pop(0))
            for i in range(0, len(locationsToAssign)):
                locationCount = self.locations()
                if self.assignQueue:
                    itemsToAssign.append(self.assign(self.assignQueue.pop(0)))
                elif self.inventory["KS"] < keystoneCount:
                    itemsToAssign.append(self.assign("KS"))
                elif self.inventory["MS"] < mapstoneCount:
                    itemsToAssign.append(self.assign("MS"))
                elif self.inventory["HC"] * self.params.cell_freq < (252 - locationCount) and self.itemPool["HC"] > 0:
                    itemsToAssign.append(self.assign("HC"))
                elif self.inventory["EC"] * self.params.cell_freq < (252 - locationCount) and self.itemPool["EC"] > 0:
                    itemsToAssign.append(self.assign("EC"))
                elif self.itemPool.get("RB28", 0) > 0 and self.itemPool["RB28"] >= locationCount:
                    itemsToAssign.append(self.assign("RB28"))
                elif self.balanceListLeftovers and self.items(include_balanced=False) < 2:
                    itemsToAssign.append(self.balanceListLeftovers.pop(0))
                else:
                    itemsToAssign.append(self.assign_random())

            # force assign things if using --prefer-path-difficulty
            if self.params.path_diff != PathDifficulty.NORMAL:
                for item in list(itemsToAssign):
                    if item in self.skillsOutput or item in self.eventsOutput:
                        self.preferred_difficulty_assign(item, locationsToAssign)
                        itemsToAssign.remove(item)

            # shuffle the items around and put them somewhere
            self.random.shuffle(itemsToAssign)
            for i in range(0, len(locationsToAssign)):
                self.assign_to_location(itemsToAssign[i], locationsToAssign[i])

            self.spoiler.append((self.currentAreas, spoilerPath, self.spoilerGroup))

            # open all reachable doors (for the next iteration)
            if self.inventory["KS"] >= keystoneCount:
                for area in self.doorQueue.keys():
                    if self.doorQueue[area].target not in self.areasReached:
                        difficulty = self.doorQueue[area].cost()[2]
                        self.seedDifficulty += difficulty * difficulty
                    self.reach_area(self.doorQueue[area].target)
                    if self.doorQueue[area].target in self.areasRemaining:
                        self.areasRemaining.remove(self.doorQueue[area].target)
                    self.areas[area].remove_connection(self.doorQueue[area])

            if self.inventory["MS"] >= mapstoneCount:
                for area in self.mapQueue.keys():
                    if self.mapQueue[area].target not in self.areasReached:
                        difficulty = self.mapQueue[area].cost()[2]
                        self.seedDifficulty += difficulty * difficulty
                    self.reach_area(self.mapQueue[area].target)
                    if self.mapQueue[area].target in self.areasRemaining:
                        self.areasRemaining.remove(self.mapQueue[area].target)
                    self.areas[area].remove_connection(self.mapQueue[area])

            locationsToAssign = []
            self.spoilerGroup = defaultdict(list)
            self.currentAreas = []

            self.doorQueue = OrderedDict()
            self.mapQueue = OrderedDict()
            spoilerPath = []

        spoilerStr = self.form_spoiler()

        if self.var(Variation.WORLD_TOUR):
            spoilerStr += "Relics: {\n"
            for instance in relicSpoiler:
                spoilerStr += "    " + instance
            spoilerStr += "}\n"

        if self.params.balanced:
            for item in self.balanceList:
                self.outputStr += item[2]

        spoilerStr = self.params.flag_line(self.verbose_paths) + "\n" + "Difficulty Rating: " + str(self.seedDifficulty) + "\n" + spoilerStr

        # place the last item on the final escape
        balanced = self.params.balanced
        self.params.balanced = False
        for item in self.itemPool:
            if self.itemPool[item] > 0:
                self.assign_to_location(item, Location(-240, 512, 'FinalEscape', 'EVWarmth', 0, 'Horu'))
                break
        else:  # In python, the else clause of a for loop triggers if the loop completed without breaking, e.g. we found nothing in the item pool
            if len(self.balanceListLeftovers) > 0:
                item = self.balanceListLeftovers.pop(0)
                log.warning("Empty item pool: placing %s from balanceListLeftovers onto warmth returned.", item)
                self.assign_to_location(item, Location(-240, 512, 'FinalEscape', 'EVWarmth', 0, 'Horu'))
            else:
                log.warning("%s: No item found for warmth returned! Placing EXP", self.params.flag_line())
                self.assign_to_location("EX*", Location(-240, 512, 'FinalEscape', 'EVWarmth', 0, 'Horu'))
        self.params.balanced = balanced

        self.random.shuffle(self.eventList)
        for event in self.eventList:
            self.outputStr += event

        if len(self.balanceListLeftovers) > 0:
            log.warning("%s: Balance list was not empty! %s", self.params.flag_line(), self.balanceListLeftovers)

        if self.params.do_loc_analysis:
            self.params.locationAnalysis = self.params.locationAnalysisCopy

        return (self.outputStr, spoilerStr)

    def get_multi_items(self, multi_item):
        multi_parts = multi_item[2:].split("/")
        multi_items = []
        while len(multi_parts) > 1:
            item = multi_parts.pop(0) + multi_parts.pop(0)
            if item[0:2] in ["AC", "EC", "KS", "MS", "HC"]:
                item = item[0:2]
            multi_items.append(self.codeToName.get(item, item))
        return multi_items

    def form_spoiler(self):
        i = 0
        groupDepth = 0
        spoilerStr = ""

        while i < len(self.spoiler):

            sets_forced = 1 if self.spoiler[i][1] else 0
            groupDepth += 1
            self.currentAreas = self.spoiler[i][0]
            spoilerPath = self.spoiler[i][1]
            self.spoilerGroup = self.spoiler[i][2]
            while i + 1 < len(self.spoiler) and len(self.spoiler[i + 1][0]) == 0:
                spoilerPath += self.spoiler[i + 1][1]
                sets_forced += 1 if self.spoiler[i + 1][1] else 0
                for item in self.spoiler[i + 1][2]:
                    self.spoilerGroup[item] += self.spoiler[i + 1][2][item]
                i += 1
            i += 1
            currentGroupSpoiler = ""

            if spoilerPath:
                currentGroupSpoiler += ("    " + str(sets_forced) + " forced pickup set" + ("" if sets_forced == 1 else "s") + ": " + str(spoilerPath) + "\n")

            for skill in self.skillsOutput:
                if skill in self.spoilerGroup:
                    for instance in self.spoilerGroup[skill]:
                        currentGroupSpoiler += "    " + instance
                    if skill in self.seedDifficultyMap:
                        self.seedDifficulty += groupDepth * self.seedDifficultyMap[skill]

            for event in self.eventsOutput:
                if event in self.spoilerGroup:
                    for instance in self.spoilerGroup[event]:
                        currentGroupSpoiler += "    " + instance

            for key in self.spoilerGroup:
                if key[:2] == "TP":
                    for instance in self.spoilerGroup[key]:
                        currentGroupSpoiler += "    " + instance

            for instance in self.spoilerGroup["MS"]:
                currentGroupSpoiler += "    " + instance

            for instance in self.spoilerGroup["KS"]:
                currentGroupSpoiler += "    " + instance

            for instance in self.spoilerGroup["HC"]:
                currentGroupSpoiler += "    " + instance

            for instance in self.spoilerGroup["EC"]:
                currentGroupSpoiler += "    " + instance

            for instance in self.spoilerGroup["AC"]:
                currentGroupSpoiler += "    " + instance

            self.currentAreas.sort()

            spoilerStr += str(groupDepth) + ": " + str(self.currentAreas) + " {\n"

            spoilerStr += currentGroupSpoiler

            spoilerStr += "}\n"

        return spoilerStr

    def do_reachability_analysis(self, params):
        self.params = params
        self.preplaced = {}
        self.playerID = 1
        self.mapQueue = {}
        self.reservedLocations = []
        self.doorQueue = {}
        self.random = random.Random()
        #items = ["WallJump", "Dash", "ChargeFlame", "DoubleJump", "Bash", "Stomp", "Grenade", "Glide", "Climb", "ChargeJump"]
        items = ["Glide", "Stomp", "DoubleJump", "ChargeFlame", "WallJump"]
        #items = ["Climb", "WallJump"]
        #items = ["Grenade", "ChargeFlame"]
        #items = ["Bash", "ChargeJump", "Glide", "DoubleJump"]
        #items = ["ChargeJump", "Stomp"]
        #items = ["TPHoru"]
        fill_items = ["WallJump", "Dash", "TPGrove", "ChargeFlame", "TPSwamp", "TPGrotto", "DoubleJump", "GinsoKey", "Bash", "TPGinso", "Water", "Stomp", "Grenade", "Glide", "TPValley", "Climb", "ForlornKey", "TPForlorn", "Wind", "ChargeJump", "TPSorrow", "HoruKey", "TPHoru"]
        overlap_items = []
        for item in overlap_items:
            fill_items.remove(item)
        #fill_items = ["TPGrove", "TPSwamp", "TPGrotto", "GinsoKey", "TPGinso", "Water", "TPValley", "ForlornKey", "TPForlorn", "Wind", "TPSorrow", "HoruKey", "TPHoru"]
        #fill_items = ["ForlornKey"]
        scores = []
        for item in items:
            self.reset()
            for item2 in fill_items:
                if item2 not in items:
                    self.inventory[item2] = 1
                    self.costs[item2] = 0
            score = 0
            for item2 in items:
                print(item + " " + item2)
                self.inventory["KS"] = 40
                self.inventory["MS"] = 11
                self.inventory["AC"] = 33
                self.inventory["EC"] = 15
                self.inventory["HC"] = 15
                self.costs["KS"] = 0
                self.costs["MS"] = 0
                self.costs["AC"] = 0
                self.costs["EC"] = 0
                self.costs["HC"] = 0
                self.form_areas()
                self.reservedLocations = []
                self.inventory[item2] = 1
                self.costs[item2] = 0
                self.inventory[item] = 0
                self.costs[item] = 1
                self.reach_area("SunkenGladesRunaway")
                if self.var(Variation.OPEN_WORLD):
                    self.reach_area("GladesMain")
                    for connection in list(self.areas["SunkenGladesRunaway"].connections):
                        if connection.target == "GladesMain":
                            self.areas["SunkenGladesRunaway"].remove_connection(connection)
                locations = 1
                while locations > 0:
                    opening = True
                    while opening:
                        (opening, keys, mapstones) = self.open_free_connections()
                        for connection in self.connectionQueue:
                            self.areas[connection[0]].remove_connection(connection[1])
                        self.connectionQueue = []
                    locationsToAssign, reset_loop = self.get_all_accessible_locations()
                    locations = len(locationsToAssign)
                self.inventory[item] = 1
                self.costs[item] = 0
                locations = 1
                while locations > 0:
                    opening = True
                    while opening:
                        (opening, keys, mapstones) = self.open_free_connections()
                        for connection in self.connectionQueue:
                            self.areas[connection[0]].remove_connection(connection[1])
                        self.connectionQueue = []
                    locationsToAssign, reset_loop = self.get_all_accessible_locations()
                    string = ""
                    for loc in locationsToAssign:
                        string += loc.to_string() + " "
                    print(string)
                    locations = len(locationsToAssign)
                    score += locations
            scores.append(score)
        for item, score in zip(items, scores):
            print("%s %d" % (item, score))