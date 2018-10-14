import math
import random
import logging as log
import xml.etree.ElementTree as XML
from collections import OrderedDict, defaultdict, Counter
from operator import mul
from enums import KeyMode, PathDifficulty, ShareType, Variation, MultiplayerGameType
from seedbuilder.areas import get_areas


def ordhash(s):
    return reduce(mul, [ord(c) for c in s])

class Area:
    def __init__(self, name):
        self.name = name
        self.connections = []
        self.locations = []
        self.difficulty = 1

    def add_connection(self, connection):
        self.connections.append(connection)

    def get_connections(self):
        return self.connections

    def remove_connection(self, connection):
        self.connections.remove(connection)

    def add_location(self, location):
        self.locations.append(location)

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
        if "GinsoKey" in req or "ForlornKey" in req or "HoruKey" in req:
            if self.sg.params.key_mode == KeyMode.SHARDS:
                if "GinsoKey" in req:
                    req.remove("GinsoKey")
                    req += ["WaterVeinShard"] * 5
                if "ForlornKey" in req:
                    req.remove("ForlornKey")
                    req += ["GumonSealShard"] * 5
                if "HoruKey" in req:
                    req.remove("HoruKey")
                    req += ["SunstoneShard"] * 5
            elif Variation.WARMTH_FRAGMENTS in self.sg.params.variations:
                if "GinsoKey" in req and "ForlornKey" in req and "HoruKey" in req:
                    req.remove("GinsoKey")
                    req.remove("ForlornKey")
                    req.remove("HoruKey")
                    req += ["RB28"]*self.sg.params.frag_count

        if "Free" in req:
            req.remove("Free")
        self.requirements.append(req)
        self.difficulties.append(difficulty)
        if not self.keys:
            self.keys = req.count("KS")
        self.mapstone = "MS" in req

    def get_requirements(self):
        return self.requirements

    def cost(self):
        minReqScore = 7777
        minDiff = 7777
        minReq = []
        for i in range(0, len(self.requirements)):
            score = 0
            energy = 0
            health = 0
            ability = 0
            warmth = 0
            for abil in self.requirements[i]:
                if abil == "EC":
                    energy += 1
                    if self.sg.inventory["EC"] < energy:
                        score += self.sg.costs[abil.strip()]
                elif abil == "HC":
                    health += 1
                    if self.sg.inventory["HC"] < health:
                        score += self.sg.costs[abil.strip()]
                elif abil == "AC":
                    ability += 1
                    if self.sg.inventory["AC"] < ability:
                        score += self.sg.costs[abil.strip()]
                elif abil == "MS":
                    if self.sg.inventory["MS"] < self.sg.mapstonesSeen:
                        score += self.sg.costs[abil.strip()]
                elif abil == "RB28":
                    warmth += 1
                    if self.sg.inventory["RB28"] < warmth:
                        score += self.sg.costs[abil.strip()]
                else:
                    score += self.sg.costs.get(abil.strip(), 0)
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

    limitKeysPool = [
        "SKWallJump", "SKChargeFlame", "SKDash", "SKStomp", "SKDoubleJump", "SKGlide", "SKClimb", "SKGrenade",
        "SKChargeJump", "EVGinsoKey", "EVForlornKey", "EVHoruKey", "SKBash", "EVWater", "EVWind"
    ]
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
        self.costs = OrderedDict({
            "Free": 0, "MS": 0, "KS": 4, "AC": 12, "EC": 6, "HC": 12, "WallJump": 13,
            "ChargeFlame": 13, "DoubleJump": 13, "Bash": 41, "Stomp": 29,
            "Glide": 17, "Climb": 41, "ChargeJump": 59, "Dash": 13,
            "Grenade": 29, "GinsoKey": 12, "ForlornKey": 12, "HoruKey": 12,
            "Water": 80, "Wind": 80, "WaterVeinShard": 5, "GumonSealShard": 5,
            "SunstoneShard": 5, "TPForlorn": 120, "TPGrotto": 60,
            "TPSorrow": 90, "TPGrove": 60, "TPSwamp": 60, "TPValley": 90,
            "TPGinso": 150, "TPHoru": 180, "Open": 1
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
            ("TPGinso", 0), ("TPHoru", 0), ("Open", 0)
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

        self.itemCount = 252.0

    def reset(self):
        """A full reset. Resets internal state completely (besides pRNG
        advancement), then sets initial values according to params."""
        self.init_fields()
        self.expRemaining = self.params.exp_pool
        self.forcedAssignments = self.preplaced
        self.forceAssignedLocs = set()
        self.itemPool = OrderedDict([
            ("EX1", 1), ("EX*", 99), ("KS", 40), ("MS", 11), ("AC", 33),
            ("EC", 14), ("HC", 12), ("WallJump", 1), ("ChargeFlame", 1),
            ("Dash", 1), ("Stomp", 1), ("DoubleJump", 1), ("Glide", 1),
            ("Bash", 1), ("Climb", 1), ("Grenade", 1), ("ChargeJump", 1),
            ("GinsoKey", 1), ("ForlornKey", 1), ("HoruKey", 1), ("Water", 1),
            ("Wind", 1), ("Warmth", 1), ("RB0", 3), ("RB1", 3), ("RB6", 3),
            ("RB8", 1), ("RB9", 1), ("RB10", 1), ("RB11", 1), ("RB12", 1),
            ("RB13", 3), ("RB15", 3), ("WaterVeinShard", 0),
            ("GumonSealShard", 0), ("SunstoneShard", 0), ("TPForlorn", 1),
            ("TPGrotto", 1), ("TPSorrow", 1), ("TPGrove", 1), ("TPSwamp", 1),
            ("TPValley", 1), ("TPGinso", 0), ("TPHoru", 0), ("Open", 0)
        ])
        if self.var(Variation.OPEN_MODE):
            self.inventory["Open"] = 1
            self.itemPool["TPGinso"] = 1
            self.itemPool["TPHoru"] = 1
            self.itemPool["KS"] -= 2

        if self.var(Variation.FREE_MAPSTONES):
            self.costs["MS"] = 11

        if self.var(Variation.HARDMODE):
            self.itemPool["AC"] = 0
            self.itemPool["HC"] = 0
            self.itemPool["EC"] = 3
            self.itemPool["EX*"] = 175
            for bonus in [k for k in self.itemPool.keys() if k[:2] == "RB"]:
                del self.itemPool[bonus]

        if self.var(Variation.EXTRA_BONUS_PICKUPS):
            self.itemPool["RB6"] += 2
            self.itemPool["RB31"] = 3
            self.itemPool["RB32"] = 3
            self.itemPool["RB33"] = 3
            self.itemPool["RB12"] += 6
            self.itemPool["RB101"] = 1
            self.itemPool["RB102"] = 1
            self.itemPool["RB103"] = 1
            self.itemPool["EX*"] -= 20

        if self.params.key_mode == KeyMode.SHARDS:
            self.itemPool["WaterVeinShard"] = 5
            self.itemPool["GumonSealShard"] = 5
            self.itemPool["SunstoneShard"] = 5
            self.itemPool["GinsoKey"] = 0
            self.itemPool["ForlornKey"] = 0
            self.itemPool["HoruKey"] = 0
            self.itemPool["EX*"] -= 12

        if Variation.WARMTH_FRAGMENTS in self.params.variations:
            self.costs["RB28"] = 3 * self.params.frag_count
            self.inventory["RB28"] = 0
            self.itemPool["RB28"] = self.params.frag_count
            self.itemPool["EX*"] -= self.params.frag_count

        if self.params.key_mode == KeyMode.FREE:
            for key in ["GinsoKey", "HoruKey", "ForlornKey"]:
                self.costs[key] = 0
                self.itemPool[key] = 0
                self.inventory[key] = 1
                self.itemPool["EX*"] += 1

        if self.params.key_mode == KeyMode.LIMITKEYS:
            satisfied = False
            while not satisfied:
                ginso = self.random.randint(0, 12)
                if ginso == 12:
                    ginso = 14
                forlorn = self.random.randint(0, 13)
                horu = self.random.randint(0, 14)
                if ginso != forlorn and ginso != horu and forlorn != horu and ginso + forlorn < 26:
                    satisfied = True
            self.keySpots = {self.limitKeysPool[ginso]: "GinsoKey", self.limitKeysPool[forlorn]: "ForlornKey",
                             self.limitKeysPool[horu]: "HoruKey"}
            self.itemPool["GinsoKey"] = 0
            self.itemPool["ForlornKey"] = 0
            self.itemPool["HoruKey"] = 0
            self.itemCount -= 3

        # paired setup for subsequent players
        if self.playerID > 1:
            for item in self.sharedList:
                self.itemPool["EX*"] += self.itemPool[item]
                self.itemPool[item] = 0
            if self.playerID not in self.sharedMap:
                self.sharedMap[self.playerID] = 0
            self.itemPool["EX*"] -= self.sharedMap[self.playerID]
            self.itemCount -= self.sharedMap[self.playerID]

    def __init__(self):
        self.init_fields()
        self.codeToName = OrderedDict(
            [(v, k) for k, v in self.skillsOutput.items() + self.eventsOutput.items()])

    def reach_area(self, target):
        if self.playerID > 1 and target in self.sharedMap:
            for sharedItem in self.sharedMap[target]:
                if sharedItem[1] == self.playerID:
                    self.assignQueue.append(sharedItem[0])
                    self.itemCount += 1
                else:
                    self.assign(sharedItem[0])
                    self.spoilerGroup[sharedItem[0]].append(sharedItem[0] + " from Player " + str(sharedItem[1]) + "\n")
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
                    elif connection.mapstone and not self.var(Variation.FREE_MAPSTONES):
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
                            if connection.mapstone and self.var(Variation.FREE_MAPSTONES):
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

    def get_all_accessible_locations(self):
        locations = []
        forced_placement = False
        for area in self.areasReached.keys():
            currentLocations = self.areas[area].get_locations()
            for location in currentLocations:
                location.difficulty += self.areas[area].difficulty
            if self.params.key_mode == KeyMode.LIMITKEYS:
                loc = ""
                for location in currentLocations:
                    if location.orig in self.keySpots.keys():
                        loc = location
                        break
                if loc:
                    self.force_assign(self.keySpots[loc.orig], loc)
                    currentLocations.remove(loc)
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
        if self.itemCount > 2 and len(locations) >= 2:
            self.reservedLocations.append(locations.pop(self.random.randrange(len(locations))))
            self.reservedLocations.append(locations.pop(self.random.randrange(len(locations))))
        return locations, forced_placement

    def prepare_path(self, free_space):
        preplaced_reduced_counts = Counter({k: self.itemPool.get(v, 0) - v for k, v in Counter(self.forcedAssignments.values()).iteritems()})
        modified_counts_keyset = set(preplaced_reduced_counts.keys())
        abilities_to_open = OrderedDict()
        totalCost = 0.0
        free_space += len(self.balanceList)
        # find the sets of abilities we need to get somewhere
        for area in self.areasReached.keys():
            for connection in self.areas[area].get_connections():
                if connection.target in self.areasReached:
                    continue
                req_sets = connection.get_requirements()
                if self.params.key_mode == KeyMode.LIMITKEYS and req_sets and not set(req_sets[0]).isdisjoint(set(["GinsoKey", "ForlornKey", "HoruKey"])):
                    continue
                for req_set in req_sets:
                    # any([v > 0 for v in preplaced_reduced_counts-Counter(req_set)])
                    if not set(req_set).isdisjoint(modified_counts_keyset) and any([log.warning("Not enough free %s left in itemCount after adjusting for preplacements! Can't open path to %s", k, connection.target) and False for k, v in (preplaced_reduced_counts-Counter(req_set)).iteritems() if v < 0]):
                        continue
                    requirements = []
                    cost = 0
                    cnts = defaultdict(lambda: 0)
                    for req in req_set:
                        if not req:
                            log.warning(req, req_set, str(connection), connection.target)
                            continue
                        if self.costs[req] > 0:
                            # for paired randomizer -- if the item isn't yours to assign, skip connection
                            if self.itemPool[req] == 0:
                                requirements = []
                                break
                            if req in ["HC", "EC", "WaterVeinShard", "GumonSealShard", "SunstoneShard", "RB28"]:
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
        for path in abilities_to_open:
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
        target = int(pow(self.random.random(), 1.0 /
                         self.balanceLevel) * len(self.balanceList))
        location = self.balanceList.pop(target)
        self.balanceListLeftovers.append(location[0])
        return location[1]

    def cloned_item(self, item, player):
        name = item  # TODO: get upgrade names lol
        if item in self.skillsOutput:
            item = self.skillsOutput[item]
        if item in self.eventsOutput:
            item = self.eventsOutput[item]
        if not self.params.sync.hints:
            return "EV5"
        hint_text = {"SK": "Skill", "TP": "Teleporter", "RB": "Upgrade", "EV": "World Event"}.get(item[:2], "?Unknown?")
        if item in ["RB17", "RB19", "RB21"]:
            name = "a " + name  # grammar lol
            hint_text = "Shard"
        elif item == "RB28":
            name = "a Warmth Fragment"
            hint_text = "Warmth Fragment"
        elif item[:2] == "WT":
            name = "a Relic"
            hint_text = "Relic"

        owner = ("Team " if self.params.sync.teams else "Player ") + str(player)
        msg = "HN%s/%s/%s" % (name, hint_text, owner)
        return msg

    def assign_random(self, recurseCount=0):
        value = self.random.random()
        position = 0.0
        denom = float(sum(self.itemPool.values()))
        if denom == 0.0:
            log.warning("itemPool was empty! itemSum: %s itemPool: %s itemCount %s", denom, {
                        k: v for k, v in self.itemPool.iteritems() if v > 0}, self.itemCount)
            return self.assign("EX*")
        for key in self.itemPool.keys():
            position += self.itemPool[key] / denom
            if value <= position:
                if self.var(Variation.STARVED) and key in self.skillsOutput and recurseCount < 3:
                    return self.assign_random(recurseCount=recurseCount + 1)
                return self.assign(key)

    def assign(self, item):
        self.itemPool[item] = max(
            self.itemPool[item] - 1, 0) if item in self.itemPool else 0
        if item == "KS":
            if self.costs[item] > 0:
                self.costs[item] -= 2
        elif item in ["EC", "HC", "AC", "WaterVeinShard", "GumonSealShard", "SunstoneShard"]:
            if self.costs[item] > 0:
                self.costs[item] -= 1
        elif item == "RB28":
            if self.costs[item] > 0:
                self.costs[item] -= min(3, self.costs[item])
        elif item in self.costs and self.itemPool[item] == 0:
            self.costs[item] = 0
        self.inventory[item] = 1 + (self.inventory[item] if item in self.inventory else 0)

        return item

    # for use in limitkeys mode
    def force_assign(self, item, location):
        self.assign(item)
        self.assign_to_location(item, location)

    def assign_to_location(self, item, location):
        zone = location.zone
#        value = 0
        hist_written = False
        at_mapstone = not self.var(
            Variation.DISCRETE_MAPSTONES) and location.orig == "MapStone"
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
                adjusted_item = self.adjust_item(item)
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
                    if player not in self.sharedMap:
                        self.sharedMap[player] = 0
                    self.sharedMap[player] += 1
                    self.spoilerGroup[item].append("%s from Player %s\n" % (item, player))
                    item = "EX*"
                    self.expSlots += 1
        # if mapstones are progressive, set a special location

        if has_cost and not hist_written:
            if at_mapstone:
                self.spoilerGroup[item].append(item + " from MapStone " + str(self.mapstonesAssigned) + "\n")
            else:
                self.spoilerGroup[item].append(item + " from " + location.to_string() + "\n")

        fixed_item = self.adjust_item(item)
        assignment = self.get_assignment(loc, fixed_item, zone)

        if item in self.eventsOutput:
            self.eventList.append(assignment)
        elif self.params.balanced and not has_cost and location.orig != "MapStone" and loc not in self.forceAssignedLocs:
            self.balanceList.append((fixed_item, location, assignment))
        else:
            self.outputStr += assignment

    def adjust_item(self, item):
        if item in self.skillsOutput:
            item = self.skillsOutput[item]
        elif item in self.eventsOutput:
            item = self.eventsOutput[item]
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

        return int(max(self.expRemaining * (self.inventory["EX*"] + self.expSlots / 4) * self.random.uniform(0.0, 2.0) / (
            self.expSlots * (self.expSlots + self.inventory["EX*"])), minExp))

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
                position += (20 - locationsToAssign[i].difficulty) * (
                    20 - locationsToAssign[i].difficulty) / total
            else:
                position += locationsToAssign[i].difficulty * \
                    locationsToAssign[i].difficulty / total
            if value <= position:
                self.assign_to_location(item, locationsToAssign[i])
                break
        del locationsToAssign[i]

    def connect_doors(self, door1, door2, requirements=["Free"]):
        connection1 = Connection(door1.name, door2.name, self)
        connection1.add_requirements(requirements, 1)
        self.areas[door1.name].add_connection(connection1)
        connection2 = Connection(door2.name, door1.name, self)
        connection2.add_requirements(requirements, 1)
        self.areas[door2.name].add_connection(connection2)
        return str(door1.get_key()) + "|EN|" + str(door2.x) + "|" + str(door2.y) + "\n" + str(
            door2.get_key()) + "|EN|" + str(door1.x) + "|" + str(door1.y) + "\n"

    def randomize_entrances(self):
        tree = XML.parse("seedbuilder/doors.xml")
        root = tree.getroot()

        outerDoors = [[], [], [], [], [], [], [], [], [], [], [], [], []]
        innerDoors = [[], [], [], [], [], [], [], [], [], [], [], [], []]

        for child in root:
            inner = child.find("Inner")
            innerDoors[int(inner.find("Group").text)].append(
                Door(child.attrib["name"] + "InnerDoor", int(inner.find("X").text), int(inner.find("Y").text)))

            outer = child.find("Outer")
            outerDoors[int(outer.find("Group").text)].append(
                Door(child.attrib["name"] + "OuterDoor", int(outer.find("X").text), int(outer.find("Y").text)))

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

        doorStr += self.connect_doors(outerDoors[2].pop(0),
                                      innerDoors[7].pop(0))

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
            doorStr += self.connect_doors(firstDoors[1],
                                          innerDoors[horuEntryGroup - 1].pop(0))
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

        self.sharedMap = {}
        self.sharedList = []
        self.random = random.Random()
        self.random.seed(self.params.seed)
        self.preplaced = {k: (
            self.codeToName[v] if v in self.codeToName else v) for k, v in preplaced.iteritems()}
        self.do_multi = self.params.sync.enabled and self.params.sync.mode == MultiplayerGameType.SHARED

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
                if self.var(Variation.WORLD_TOUR):
                    self.sharedList.append("Relic")
                    # TODO: is this the proper generator stringform? Probably not.
        return self.placeItemsMulti(retries)

    def placeItemsMulti(self, retries=5):
        placements = []
        self.sharedMap = {}
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
                            self.itemCount,
                            {k: v for k, v in self.itemPool.iteritems() if v > 0},
                            [x for x in self.areasReached],
                            [x for x in self.areasRemaining],
                            {k: v for k, v in self.inventory.iteritems() if v != 0},
                            self.forcedAssignments,
                            {k: v for k, v in self.costs.iteritems() if v != 0},
                            self.outputStr
                        )
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
                placements.append(("\n".join(outlines)+"\n", spoiler))
            else:
                placement = self.placeItems(0)
                if not placement:
                    if self.preplaced:
                        if retries > 0:
                            retries -= 1
                        else:
                            log.error("Coop seed not completeable with these params and placements")
                            return None
                    return self.placeItemsMulti(retries)
                placements.append(placement)

        return placements

    def placeItems(self, depth=0):
        self.reset()

        spoilerStr = ""
        groupDepth = 0
        keystoneCount = 0
        mapstoneCount = 0
        plants = []

        tree = get_areas()
        root = tree.getroot()
        logic_paths = [lp.value for lp in self.params.logic_paths]
        for child in root:
            area = Area(child.attrib["name"])
            self.areasRemaining.append(child.attrib["name"])

            for location in child.find("Locations"):
                loc = Location(int(location.find("X").text), int(location.find("Y").text), area.name,
                               location.find("Item").text, int(
                                   location.find("Difficulty").text),
                               location.find("Zone").text)
                area.add_location(loc)
            if child.find("Connections") is None:
                log.error("No connections found for child %s, (name %s)" % (child, child.attrib["name"]))
            for conn in child.find("Connections"):
                connection = Connection(conn.find("Home").attrib["name"], conn.find("Target").attrib["name"], self)
                entranceConnection = conn.find("Entrance")
                if self.var(Variation.ENTRANCE_SHUFFLE) and entranceConnection is not None:
                    continue
                for req in conn.find("Requirements"):
                    if req.attrib["mode"] in logic_paths:
                        connection.add_requirements(req.text.split('+'), self.difficultyMap[req.attrib["mode"]])
                if connection.get_requirements():
                    area.add_connection(connection)
            self.areas[area.name] = area

        # flags line
        self.outputStr += (self.params.flag_line(self.verbose_paths) + "\n")

        if self.var(Variation.ENTRANCE_SHUFFLE):
            self.outputStr += self.randomize_entrances()

        # handle the fixed pickups: first energy cell, the glitchy 100 orb at spirit tree, the forlorn escape plant, and the 2nd keystone in misty

        for loc, item, zone in [(-280256, "EC1", "Glades"), (-1680104, "EX100", "Grove"), (-12320248, "Grenade", "Forlorn"), (-10440008, "EX100", "Misty")]:
            if loc in self.forcedAssignments:
                item = self.forcedAssignments[loc]
                del self.forcedAssignments[loc]  # don't count these ones
            ass = self.get_assignment(loc, self.adjust_item(item), zone)
            if loc == -280256 and self.params.key_mode == KeyMode.FREE:
                splitAss = ass.split("|")
                splitAss[2] = "EV/0/EV/2/EV/4/%s/%s" % (splitAss[1], splitAss[2])
                splitAss[1] = "MU"
                ass = "|".join(splitAss)
            self.outputStr += ass

        for v in self.forcedAssignments.values():
            if v in self.itemPool:
                self.itemPool[v] -= 1
        self.itemCount -= len(self.forcedAssignments)


        locationsToAssign = []
        self.connectionQueue = []
        self.reservedLocations = []

        self.skillCount = 10
        self.mapstonesAssigned = 0
        self.expSlots = self.itemPool["EX*"]

        self.spoilerGroup = defaultdict(
            list, {"MS": [], "KS": [], "EC": [], "HC": []})

        self.doorQueue = OrderedDict()
        self.mapQueue = OrderedDict()
        spoilerPath = ""

        self.reach_area("SunkenGladesRunaway")
        if self.var(Variation.OPEN_MODE):
            self.reach_area("GladesMain")
            for connection in list(self.areas["SunkenGladesRunaway"].connections):
                if connection.target == "GladesMain":
                    self.areas["SunkenGladesRunaway"].remove_connection(connection)

        while self.itemCount > 0 or (self.params.balanced and self.balanceListLeftovers):

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
            if not self.doorQueue and not self.mapQueue and not reset_loop:
                spoilerPath = self.prepare_path(len(locationsToAssign))
                if not self.assignQueue:
                    # we've painted ourselves into a corner, try again
                    if not self.reservedLocations:
                        if self.playerID == 1:
                            self.split_locs = {}
                            self.sharedMap = {}
                        if depth > self.playerCount * self.playerCount:
                            return
                        return self.placeItems(depth + 1)
                    locationsToAssign.append(self.reservedLocations.pop(0))
                    locationsToAssign.append(self.reservedLocations.pop(0))
                    spoilerPath = self.prepare_path(len(locationsToAssign))
                if self.params.balanced:
                    for item in self.assignQueue:
                        if len(self.balanceList) == 0:
                            break
                        locationsToAssign.append(self.get_location_from_balance_list())
                        
            # pick what we're going to put in our accessible space
            itemsToAssign = []
            if len(locationsToAssign) < len(self.assignQueue) + max(keystoneCount - self.inventory["KS"], 0) + max(
                    mapstoneCount - self.inventory["MS"], 0):
                # we've painted ourselves into a corner, try again
                if not self.reservedLocations:
                    if self.playerID == 1:
                        self.split_locs = {}
                        self.sharedMap = {}
                    if depth > self.playerCount * self.playerCount:
                        return
                    return self.placeItems(depth + 1)
                locationsToAssign.append(self.reservedLocations.pop(0))
                locationsToAssign.append(self.reservedLocations.pop(0))
            for i in range(0, len(locationsToAssign)):
                if self.assignQueue:
                    itemsToAssign.append(self.assign(self.assignQueue.pop(0)))
                elif self.inventory["KS"] < keystoneCount:
                    itemsToAssign.append(self.assign("KS"))
                elif self.inventory["MS"] < mapstoneCount:
                    itemsToAssign.append(self.assign("MS"))
                elif self.inventory["HC"] * self.params.cell_freq < (252 - self.itemCount) and self.itemPool["HC"] > 0:
                    itemsToAssign.append(self.assign("HC"))
                elif self.inventory["EC"] * self.params.cell_freq < (252 - self.itemCount) and self.itemPool["EC"] > 0:
                    itemsToAssign.append(self.assign("EC"))
                elif self.params.balanced and self.itemCount == 0:
                    itemsToAssign.append(self.balanceListLeftovers.pop(0))
                    self.itemCount += 1
                else:
                    itemsToAssign.append(self.assign_random())
                self.itemCount -= 1

            # force assign things if using --prefer-path-difficulty
            if self.params.path_diff != PathDifficulty.NORMAL:
                for item in list(itemsToAssign):
                    if item in self.skillsOutput or item in self.eventsOutput:
                        self.preferred_difficulty_assign(
                            item, locationsToAssign)
                        itemsToAssign.remove(item)

            # shuffle the items around and put them somewhere
            self.random.shuffle(itemsToAssign)
            for i in range(0, len(locationsToAssign)):
                self.assign_to_location(itemsToAssign[i], locationsToAssign[i])

            currentGroupSpoiler = ""

            if spoilerPath:
                currentGroupSpoiler += ("	Forced pickups: " + str(spoilerPath) + "\n")

            for skill in self.skillsOutput:
                if skill in self.spoilerGroup:
                    for instance in self.spoilerGroup[skill]:
                        currentGroupSpoiler += "	" + instance
                    if skill in self.seedDifficultyMap:
                        self.seedDifficulty += groupDepth * self.seedDifficultyMap[skill]

            for event in self.eventsOutput:
                if event in self.spoilerGroup:
                    for instance in self.spoilerGroup[event]:
                        currentGroupSpoiler += "	" + instance

            for key in self.spoilerGroup:
                if key[:2] == "TP":
                    for instance in self.spoilerGroup[key]:
                        currentGroupSpoiler += "	" + instance

            for instance in self.spoilerGroup["MS"]:
                currentGroupSpoiler += "	" + instance

            for instance in self.spoilerGroup["KS"]:
                currentGroupSpoiler += "	" + instance

            for instance in self.spoilerGroup["HC"]:
                currentGroupSpoiler += "	" + instance

            for instance in self.spoilerGroup["EC"]:
                currentGroupSpoiler += "	" + instance

            if currentGroupSpoiler:
                groupDepth += 1
                self.currentAreas.sort()

                spoilerStr += str(groupDepth) + ": " + str(self.currentAreas) + " {\n"

                spoilerStr += currentGroupSpoiler

                spoilerStr += "}\n"

            self.currentAreas = []

            # open all reachable doors (for the next iteration)
            for area in self.doorQueue.keys():
                if self.doorQueue[area].target not in self.areasReached:
                    difficulty = self.doorQueue[area].cost()[2]
                    self.seedDifficulty += difficulty * difficulty
                self.reach_area(self.doorQueue[area].target)
                if self.doorQueue[area].target in self.areasRemaining:
                    self.areasRemaining.remove(self.doorQueue[area].target)
                self.areas[area].remove_connection(self.doorQueue[area])

            for area in self.mapQueue.keys():
                if self.mapQueue[area].target not in self.areasReached:
                    difficulty = self.mapQueue[area].cost()[2]
                    self.seedDifficulty += difficulty * difficulty
                self.reach_area(self.mapQueue[area].target)
                if self.mapQueue[area].target in self.areasRemaining:
                    self.areasRemaining.remove(self.mapQueue[area].target)
                self.areas[area].remove_connection(self.mapQueue[area])

            locationsToAssign = []
            self.spoilerGroup = defaultdict(
                list, {"MS": [], "KS": [], "EC": [], "HC": []})

            self.doorQueue = OrderedDict()
            self.mapQueue = OrderedDict()
            spoilerPath = ""

        if self.params.balanced:
            for item in self.balanceList:
                self.outputStr += item[2]

        spoilerStr = self.params.flag_line(self.verbose_paths) + "\n" + "Difficulty Rating: " + str(self.seedDifficulty) + "\n" + spoilerStr
        self.random.shuffle(self.eventList)
        for event in self.eventList:
            self.outputStr += event

        return (self.outputStr, spoilerStr)
