import re
import math
import xml.etree.ElementTree as XML
import argparse
import time
from collections import OrderedDict


# A custom implementation of a Mersenne Twister
# (since javascript hates everything)
# https://en.wikipedia.org/wiki/Mersenne_Twister
class Random:
    def seed(self, seed):
        self.index = 624
        self.mt = [0] * 624
        self.mt[0] = hash(seed)
        for i in range(1, 624):
            self.mt[i] = int(0xFFFFFFFF & (1812433253 * (self.mt[i - 1] ^ self.mt[i - 1] >> 30) + i))

    def generate_sequence(self):
        for i in range(624):
            # Get the most significant bit and add it to the less significant
            # bits of the next number
            y = int(0xFFFFFFFF & (self.mt[i] & 0x80000000) + (self.mt[(i + 1) % 624] & 0x7fffffff))
            self.mt[i] = self.mt[(i + 397) % 624] ^ y >> 1

            if y % 2 != 0:
                self.mt[i] = self.mt[i] ^ 0x9908b0df
        self.index = 0

    def random(self):
        if self.index >= 624:
            self.generate_sequence()

        y = self.mt[self.index]

        # Right shift by 11 bits
        y = y ^ y >> 11
        # Shift y left by 7 and take the bitwise and of 2636928640
        y = y ^ y << 7 & 2636928640
        # Shift y left by 15 and take the bitwise and of y and 4022730752
        y = y ^ y << 15 & 4022730752
        # Right shift by 18 bits
        y = y ^ y >> 18

        self.index += 1

        return int(0xFFFFFFFF & y) / float(0x100000000)

    def randrange(self, length):
        return int(self.random() * length)

    def randint(self, low, high):
        return int(low + self.random() * (high - low + 1))

    def uniform(self, low, high):
        return self.random() * (high - low) + low

    def shuffle(self, items):
        original = list(items)
        for i in range(len(items)):
            items[i] = original.pop(self.randrange(len(original)))


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
    def __init__(self, home, target):
        self.home = home
        self.target = target
        self.keys = 0
        self.mapstone = False
        self.requirements = []
        self.difficulties = []

    def add_requirements(self, req, difficulty):
        if shards:
            match = re.match(".*GinsoKey.*", str(req))
            if match:
                req.remove("GinsoKey")
                for _ in range(5):
                    req.append("WaterVeinShard")
            match = re.match(".*ForlornKey.*", str(req))
            if match:
                req.remove("ForlornKey")
                for _ in range(5):
                    req.append("GumonSealShard")
            match = re.match(".*HoruKey.*", str(req))
            if match:
                req.remove("HoruKey")
                for _ in range(5):
                    req.append("SunstoneShard")
        self.requirements.append(req)
        self.difficulties.append(difficulty)
        if self.get_match("(.*KS){4}.*", req):
            self.keys = 4
        elif self.get_match("(.*KS){2}.*", req):
            self.keys = 2
        elif self.get_match(".*MS.*", req):
            self.mapstone = True

    def get_match(self, regex, req):
        return re.match(regex, str(req))

    def get_requirements(self):
        return self.requirements

    def cost(self):
        minReqScore = 7777
        minDiff = 7777
        minReq = []
        for i in range(0, len(self.requirements)):
            score = energy = health = 0
            for abil in self.requirements[i]:
                if abil == "EC":
                    energy += 1
                    if inventory["EC"] < energy:
                        score += costs[abil.strip()]
                elif abil == "HC":
                    health += 1
                    if inventory["HC"] < health:
                        score += costs[abil.strip()]
                else:
                    score += costs[abil.strip()]
            if score < minReqScore:
                minReqScore = score
                minReq = self.requirements[i]
                minDiff = self.difficulties[i]
        return (minReqScore, minReq, minDiff)


class Location:
    factor = 4.0

    def __init__(self, x, y, area, orig, difficulty, zone):
        self.x = int(math.floor(x / self.factor) * self.factor)
        self.y = int(math.floor(y / self.factor) * self.factor)
        self.orig = orig
        self.area = area
        self.difficulty = difficulty
        self.zone = zone

    def get_key(self):
        return self.x * 10000 + self.y

    def to_string(self):
        return "{} {} ({} {})".format(self.area, self.orig, self.x, self.y)


class Door:
    factor = 4.0

    def __init__(self, name, x, y):
        self.x = x
        self.y = y
        self.name = name

    def get_key(self):
        return int(math.floor(self.x / self.factor) * self.factor) * 10000 + int(
            math.floor(self.y / self.factor) * self.factor)


def reach_area(target):
    global itemCount
    if target in sharedMap and playerID > 1:
        for sharedItem in sharedMap[target]:
            if sharedItem[1] == playerID:
                assignQueue.append(sharedItem[0])
                itemCount += 1
            else:
                assign(sharedItem[0])
                if sharedItem[0] not in spoilerGroup:
                    spoilerGroup[sharedItem[0]] = []
                spoilerGroup[sharedItem[0]].append(sharedItem[0] + " from Player " + str(sharedItem[1]) + "\n")
    currentAreas.append(target)
    areasReached[target] = True


def open_free_connections():
    global seedDifficulty
    found = False
    keystoneCount = 0
    mapstoneCount = 0
    # python 3 wont allow concurrent changes
    # list(areasReached.keys()) is a copy of the original list
    for area in list(areasReached.keys()):
        for connection in areas[area].get_connections():
            cost = connection.cost()
            if cost[0] <= 0:
                areas[connection.target].difficulty = cost[2]
                if connection.keys > 0:
                    if area not in doorQueue.keys():
                        doorQueue[area] = connection
                        keystoneCount += connection.keys
                elif connection.mapstone:
                    if connection.target not in areasReached:
                        visitMap = True
                        for map in mapQueue.keys():
                            if map == area or mapQueue[map].target == connection.target:
                                visitMap = False
                        if visitMap:
                            mapQueue[area] = connection
                            mapstoneCount += 1
                else:
                    if connection.target not in areasReached:
                        seedDifficulty += cost[2] * cost[2]
                        reach_area(connection.target)
                    if connection.target in areasRemaining:
                        areasRemaining.remove(connection.target)
                    connectionQueue.append((area, connection))
                    found = True
    return (found, keystoneCount, mapstoneCount)


def get_all_accessible_locations():
    locations = []
    for area in areasReached.keys():
        currentLocations = areas[area].get_locations()
        for location in currentLocations:
            location.difficulty += areas[area].difficulty
        if limitkeys:
            loc = ""
            for location in currentLocations:
                if location.orig in keySpots.keys():
                    loc = location
                    break
            if loc:
                force_assign(keySpots[loc.orig], loc)
                currentLocations.remove(loc)
        locations.extend(currentLocations)
        areas[area].clear_locations()
    if reservedLocations:
        locations.append(reservedLocations.pop(0))
        locations.append(reservedLocations.pop(0))
    if itemCount > 2 and len(locations) >= 2:
        reservedLocations.append(locations.pop(random.randrange(len(locations))))
        reservedLocations.append(locations.pop(random.randrange(len(locations))))
    return locations


def prepare_path(free_space):
    abilities_to_open = OrderedDict()
    totalCost = 0.0
    free_space += len(balanceList)
    # find the sets of abilities we need to get somewhere
    for area in areasReached.keys():
        for connection in areas[area].get_connections():
            if connection.target in areasReached:
                continue
            first_requirement = connection.get_requirements()[0]
            if limitkeys and connection.get_requirements() and ("GinsoKey" in first_requirement or "ForlornKey" in first_requirement or "HoruKey" in first_requirement):
                continue
            for req_set in connection.get_requirements():
                requirements = []
                cost = 0
                energy = 0
                health = 0
                waterVeinShard = 0
                gumonSealShard = 0
                sunstoneShard = 0
                for req in req_set:
                    # for paired randomizer -- if the item isn't yours to assign, skip connection
                    if itemPool[req] == 0:
                        requirements = []
                        break
                    if costs[req] > 0:
                        if req == "EC":
                            energy += 1
                            if energy > inventory["EC"]:
                                requirements.append(req)
                                cost += costs[req]
                        elif req == "HC":
                            health += 1
                            if health > inventory["HC"]:
                                requirements.append(req)
                                cost += costs[req]
                        elif req == "WaterVeinShard":
                            waterVeinShard += 1
                            if waterVeinShard > inventory["WaterVeinShard"]:
                                requirements.append(req)
                                cost += costs[req]
                        elif req == "GumonSealShard":
                            gumonSealShard += 1
                            if gumonSealShard > inventory["GumonSealShard"]:
                                requirements.append(req)
                                cost += costs[req]
                        elif req == "SunstoneShard":
                            sunstoneShard += 1
                            if sunstoneShard > inventory["SunstoneShard"]:
                                requirements.append(req)
                                cost += costs[req]
                        else:
                            requirements.append(req)
                            cost += costs[req]
                # cost *= len(requirements) # decrease the rate of multi-ability paths
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
    target = random.random() * totalCost
    for path in abilities_to_open:
        position += 1.0 / abilities_to_open[path][0]
        if target <= position:
            for req in abilities_to_open[path][1]:
                if itemPool[req] > 0:
                    assignQueue.append(req)
            return abilities_to_open[path][1]


def get_location_from_balance_list():
    target = int(pow(random.random(), 1.0 / balanceLevel) * len(balanceList))
    location = balanceList.pop(target)
    balanceListLeftovers.append(location[0])
    return location[1]


def assign_random(recurseCount=0):
    value = random.random()
    position = 0.0
    for key in itemPool.keys():
        position += itemPool[key] / itemCount
        if value <= position:
            if starved and key in skillsOutput and recurseCount < 3:
                return assign_random(recurseCount=recurseCount + 1)
            return assign(key)


def assign(item):
    itemPool[item] = max(itemPool[item] - 1, 0)
    if item in ("EC", "KS", "HC"):
        if costs[item] > 0:
            costs[item] -= 1
    elif item in ("WaterVeinShard", "GumonSealShard", "SunstoneShard"):
        if costs[item] > 0:
            costs[item] -= 1
    elif item in costs.keys():
        costs[item] = 0
    inventory[item] += 1
    return item


# for use in limitkeys mode
def force_assign(item, location):
    assign(item)
    assign_to_location(item, location)


def assign_to_location(item, location):
    global outputStr
    global eventList
    global spoilerGroup
    global mapstonesAssigned
    global skillCount
    global expRemaining
    global expSlots
    global playerCount
    global playerID
    global sharedList
    global sharedMap
    global balanced
    global balanceList

    assignment = ""
    zone = location.zone
    value = 0

    # if this is the first player of a paired seed, construct the map
    if playerCount > 1 and playerID == 1 and item in sharedList:
        player = random.randint(1, playerCount)
        if location.area not in sharedMap:
            sharedMap[location.area] = []
        sharedMap[location.area].append((item, player))

        if player is not playerID:
            if player not in sharedMap:
                sharedMap[player] = 0
            sharedMap[player] += 1
            if item not in spoilerGroup:
                spoilerGroup[item] = []
            spoilerGroup[item].append(item + " from Player " + str(player) + "\n")
            item = "EX*"
            expSlots += 1

    # if mapstones are progressive, set a special location
    if not nonProgressiveMapstones and location.orig == "MapStone":
        mapstonesAssigned += 1
        assignment += str(20 + mapstonesAssigned * 4) + "|"
        zone = "Mapstone"
        if item in costs.keys():
            if item not in spoilerGroup:
                spoilerGroup[item] = []
            spoilerGroup[item].append(item + " from MapStone " + str(mapstonesAssigned) + "\n")
    else:
        assignment += str(location.get_key()) + "|"
        if item in costs.keys():
            if item not in spoilerGroup:
                spoilerGroup[item] = []
            spoilerGroup[item].append(item + " from " + location.to_string() + "\n")

    if item in skillsOutput:
        assignment += (str(skillsOutput[item][:2]) + "|" + skillsOutput[item][2:])
        if analysis:
            skillAnalysis[item] += skillCount
            skillCount -= 1
    elif item in eventsOutput:
        assignment += (str(eventsOutput[item][:2]) + "|" + eventsOutput[item][2:])
    elif item == "EX*":
        value = get_random_exp_value(expRemaining, expSlots)
        expRemaining -= value
        expSlots -= 1
        assignment += "EX|" + str(value)
    elif item[2:]:
        assignment += (item[:2] + "|" + item[2:])
    else:
        assignment += (item[:2] + "|1")
    assignment += ("|" + zone + "\n")

    if item in eventsOutput:
        eventList.append(assignment)
    elif balanced and item not in costs.keys() and location.orig != "MapStone":
        if value > 0:
            item = "EX" + str(value)
        balanceList.append((item, location, assignment))
    else:
        outputStr += assignment

    if doLocationAnalysis:
        key = location.to_string()
        if location.orig == "MapStone":
            key = "MapStone " + str(mapstonesAssigned)
        if item in locationAnalysisCopy[key]:
            locationAnalysisCopy[key][item] += 1
            locationAnalysisCopy[location.zone][item] += 1


def get_random_exp_value(expRemaining, expSlots):
    min_ = random.randint(2, 9)

    if expSlots <= 1:
        return max(expRemaining, min_)

    return int(max(expRemaining * (inventory["EX*"] + expSlots / 4) * random.uniform(0.0, 2.0) / (
    expSlots * (expSlots + inventory["EX*"])), min_))


def preferred_difficulty_assign(item, locationsToAssign):
    total = 0.0
    for loc in locationsToAssign:
        if pathDifficulty == "easy":
            total += (15 - loc.difficulty) * (15 - loc.difficulty)
        else:
            total += (loc.difficulty * loc.difficulty)
    value = random.random()
    position = 0.0
    for i in range(0, len(locationsToAssign)):
        if pathDifficulty == "easy":
            position += (15 - locationsToAssign[i].difficulty) * (15 - locationsToAssign[i].difficulty) / total
        else:
            position += locationsToAssign[i].difficulty * locationsToAssign[i].difficulty / total
        if value <= position:
            assign_to_location(item, locationsToAssign[i])
            break
    del locationsToAssign[i]


def connect_doors(door1, door2, areas, requirements=["Free"]):
    connection1 = Connection(door1.name, door2.name)
    connection1.add_requirements(requirements, 1)
    areas[door1.name].add_connection(connection1)
    connection2 = Connection(door2.name, door1.name)
    connection2.add_requirements(requirements, 1)
    areas[door2.name].add_connection(connection2)
    return (2 * '{}|EN|{}|{}\n').format(door1.get_key(), door2.x, door2.y, door2.get_key(), door1.x, door1.y)


def randomize_entrances(areas):
    tree = XML.parse("doors.xml")
    root = tree.getroot()

    outerDoors = innerDoors = [[] for _ in range(13)]

    for child in root:
        inner = child.find("Inner")
        innerDoors[int(inner.find("Group").text)].append(
            Door(child.attrib["name"] + "InnerDoor", int(inner.find("X").text), int(inner.find("Y").text)))

        outer = child.find("Outer")
        outerDoors[int(outer.find("Group").text)].append(
            Door(child.attrib["name"] + "OuterDoor", int(outer.find("X").text), int(outer.find("Y").text)))

    random.shuffle(outerDoors[0])
    random.shuffle(innerDoors[12])

    firstDoors = []
    lastDoors = []

    firstDoors.append(outerDoors[0].pop(0))
    firstDoors.append(outerDoors[0].pop(0))

    lastDoors.append(innerDoors[12].pop(0))
    lastDoors.append(innerDoors[12].pop(0))

    doorStr = ""

    activeGroups = [0, 1, 2]
    targets = [3, 4, 5, 6, 7, 8, 9, 10, 12]

    random.shuffle(targets)

    horuEntryGroup = random.randint(4, 10)
    if horuEntryGroup >= 7:
        horuEntryGroup += 1
    if horuEntryGroup == 11:
        horuEntryGroup = 1
        if random.random() > 0.5:
            doorStr += connect_doors(firstDoors[0], innerDoors[1].pop(0), areas)
            doorStr += connect_doors(firstDoors[1], outerDoors[1].pop(0), areas)
        else:
            doorStr += connect_doors(firstDoors[0], outerDoors[1].pop(0), areas)
            doorStr += connect_doors(firstDoors[1], innerDoors[1].pop(0), areas)
    else:
        requirements = ["Free"]
        if firstDoors[1].name == "GinsoDoorOuter":
            requirements = ["GinsoKey"]
        if firstDoors[1].name == "ForlornDoorOuter":
            requirements = ["ForlornKey"]
        doorStr += connect_doors(firstDoors[0], outerDoors[horuEntryGroup].pop(0), areas, requirements)
        doorStr += connect_doors(firstDoors[1], innerDoors[horuEntryGroup - 1].pop(0), areas)
        targets.remove(horuEntryGroup - 1)

    while len(targets) > 0:
        index = random.randrange(len(activeGroups))
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

        doorStr += connect_doors(outerDoors[group].pop(0), innerDoors[target].pop(0), areas)

    lastDoorIndex = 0

    for group in range(13):
        if innerDoors[group]:
            doorStr += connect_doors(innerDoors[group].pop(0), lastDoors[lastDoorIndex], areas)
            lastDoorIndex += 1
        if outerDoors[group]:
            doorStr += connect_doors(outerDoors[group].pop(0), lastDoors[lastDoorIndex], areas)
            lastDoorIndex += 1

    return doorStr


def placeItemsMulti(seed, expPool, hardMode, includePlants, shardsMode, limitkeysMode, cluesMode, noTeleporters,
                    doLocAnalysis, doSkillOrderAnalysis, modes, flags, syncFlags, starvedMode, preferPathDifficulty,
                    setNonProgressiveMapstones, playerCountIn, balanced, entrance):
    global sharedMap

    placements = []
    sharedMap = {}
    playerID = 1
    playerFlags = flags

    if syncFlags:
        playerFlags = flags + syncFlags + "." + str(playerID)

    placement = placeItems(seed, expPool, hardMode, includePlants, shardsMode, limitkeysMode, cluesMode, noTeleporters,
                           doLocAnalysis, doSkillOrderAnalysis, modes, playerFlags, starvedMode, preferPathDifficulty,
                           setNonProgressiveMapstones, playerCountIn, playerID, balanced, entrance)
    if not placement:
        return placeItemsMulti(seed, expPool, hardMode, includePlants, shardsMode, limitkeysMode, cluesMode,
                               noTeleporters, doLocAnalysis, doSkillOrderAnalysis, modes, flags, syncFlags, starvedMode,
                               preferPathDifficulty, setNonProgressiveMapstones, playerCountIn, balanced, entrance)
    placements.append(placement)
    while playerID < playerCountIn:
        playerID += 1
        if syncFlags:
            playerFlags = flags + syncFlags + "." + str(playerID)
        placement = placeItems(seed, expPool, hardMode, includePlants, shardsMode, limitkeysMode, cluesMode,
                               noTeleporters, doLocAnalysis, doSkillOrderAnalysis, modes, playerFlags, starvedMode,
                               preferPathDifficulty, setNonProgressiveMapstones, playerCountIn, playerID, balanced,
                               entrance)
        if not placement:
            return placeItemsMulti(seed, expPool, hardMode, includePlants, shardsMode, limitkeysMode, cluesMode,
                                   noTeleporters, doLocAnalysis, doSkillOrderAnalysis, modes, flags, syncFlags,
                                   starvedMode, preferPathDifficulty, setNonProgressiveMapstones, playerCountIn,
                                   balanced, entrance)
        placements.append(placement)

    return placements


def placeItems(seed, expPool, hardMode, includePlants, shardsMode, limitkeysMode, cluesMode, noTeleporters,
               doLocAnalysis, doSkillOrderAnalysis, modes, flags, starvedMode, preferPathDifficulty,
               setNonProgressiveMapstones, playerCountIn, playerIDIn, balancedIn, entrance, depth=0):
    global costs
    global areas
    global areasReached
    global currentAreas
    global itemCount
    global itemPool
    global assignQueue
    global inventory
    global doorQueue
    global mapQueue
    global connectionQueue
    global outputStr
    global eventList
    global mapstonesAssigned
    global skillCount
    global expRemaining
    global expSlots
    global areasRemaining

    global shards
    global limitkeys
    global clues
    global starved
    global pathDifficulty
    global nonProgressiveMapstones
    global analysis
    global locationAnalysis
    global locationAnalysisCopy
    global doLocationAnalysis

    shards = shardsMode
    limitkeys = limitkeysMode
    clues = cluesMode
    starved = starvedMode
    pathDifficulty = preferPathDifficulty
    nonProgressiveMapstones = setNonProgressiveMapstones
    analysis = doSkillOrderAnalysis
    doLocationAnalysis = doLocAnalysis

    global balanced
    global balanceLevel
    global balanceList
    global balanceListLeftovers

    balanced = balancedIn
    balanceLevel = 0
    balanceList = []
    balanceListLeftovers = []

    global playerCount
    global playerID

    global sharedMap
    global sharedList

    playerCount = playerCountIn
    playerID = playerIDIn

    global skillsOutput
    global eventsOutput

    skillsOutput = {
        "WallJump": "SK3",
        "ChargeFlame": "SK2",
        "Dash": "SK50",
        "Stomp": "SK4",
        "DoubleJump": "SK5",
        "Glide": "SK14",
        "Bash": "SK0",
        "Climb": "SK12",
        "Grenade": "SK51",
        "ChargeJump": "SK8"
    }

    eventsOutput = {
        "GinsoKey": "EV0",
        "Water": "EV1",
        "ForlornKey": "EV2",
        "Wind": "EV3",
        "HoruKey": "EV4",
        "Warmth": "EV5",
        "WaterVeinShard": "RB17",
        "GumonSealShard": "RB19",
        "SunstoneShard": "RB21"
    }

    global seedDifficulty

    seedDifficultyMap = {
        "Dash": 2,
        "Bash": 2,
        "Glide": 3,
        "DoubleJump": 2,
        "ChargeJump": 1
    }
    seedDifficulty = 0

    limitKeysPool = ["SKWallJump", "SKChargeFlame", "SKDash", "SKStomp", "SKDoubleJump", "SKGlide", "SKClimb",
                     "SKGrenade", "SKChargeJump", "EVGinsoKey", "EVForlornKey", "EVHoruKey", "SKBash", "EVWater",
                     "EVWind"]

    difficultyMap = {
        "normal": 1,
        "speed": 2,
        "lure": 2,
        "speed-lure": 3,
        "dboost": 2,
        "dboost-light": 1,
        "dboost-hard": 3,
        "cdash": 2,
        "cdash-farming": 2,
        "dbash": 3,
        "extended": 3,
        "extended-damage": 3,
        "lure-hard": 4,
        "extreme": 4,
        "glitched": 5,
        "timed-level": 5
    }

    outputStr = ""
    eventList = []
    spoilerStr = ""
    groupDepth = 0

    costs = {
        "Free": 0,
        "MS": 0,
        "KS": 2,
        "EC": 6,
        "HC": 12,
        "WallJump": 13,
        "ChargeFlame": 13,
        "DoubleJump": 13,
        "Bash": 41,
        "Stomp": 29,
        "Glide": 17,
        "Climb": 41,
        "ChargeJump": 59,
        "Dash": 13,
        "Grenade": 29,
        "GinsoKey": 12,
        "ForlornKey": 12,
        "HoruKey": 12,
        "Water": 80,
        "Wind": 80,
        "WaterVeinShard": 5,
        "GumonSealShard": 5,
        "SunstoneShard": 5,
        "TPForlorn": 120,
        "TPGrotto": 60,
        "TPSorrow": 90,
        "TPGrove": 60,
        "TPSwamp": 60,
        "TPValley": 90
    }

    # we use OrderedDicts here because the order of a dict depends on the size of the dict and the hash of the keys
    # since python 3.3 the order of a given dict is also dependent on the random hash seed for the current Python invocation
    #     which apparently ignores our random.seed()
    # https://stackoverflow.com/questions/15479928/why-is-the-order-in-dictionaries-and-sets-arbitrary/15479974#15479974
    # Note that as of Python 3.3, a random hash seed is used as well, making hash collisions unpredictable
    # to prevent certain types of denial of service (where an attacker renders a Python server unresponsive
    # by causing mass hash collisions). This means that the order of a given dictionary is then also
    # dependent on the random hash seed for the current Python invocation.

    areas = OrderedDict()

    areasReached = OrderedDict([])
    currentAreas = []
    areasRemaining = []
    connectionQueue = []
    assignQueue = []

    itemCount = 244.0
    expRemaining = expPool
    keystoneCount = 0
    mapstoneCount = 0

    if not hardMode:
        itemPool = OrderedDict([
            ("EX1", 1),
            ("EX*", 91),
            ("KS", 40),
            ("MS", 11),
            ("AC", 33),
            ("EC", 14),
            ("HC", 12),
            ("WallJump", 1),
            ("ChargeFlame", 1),
            ("Dash", 1),
            ("Stomp", 1),
            ("DoubleJump", 1),
            ("Glide", 1),
            ("Bash", 1),
            ("Climb", 1),
            ("Grenade", 1),
            ("ChargeJump", 1),
            ("GinsoKey", 1),
            ("ForlornKey", 1),
            ("HoruKey", 1),
            ("Water", 1),
            ("Wind", 1),
            ("Warmth", 1),
            ("RB0", 3),
            ("RB1", 3),
            ("RB6", 3),
            ("RB8", 1),
            ("RB9", 1),
            ("RB10", 1),
            ("RB11", 1),
            ("RB12", 1),
            ("RB13", 3),
            ("RB15", 3),
            ("WaterVeinShard", 0),
            ("GumonSealShard", 0),
            ("SunstoneShard", 0),
            ("TPForlorn", 1),
            ("TPGrotto", 1),
            ("TPSorrow", 1),
            ("TPGrove", 1),
            ("TPSwamp", 1),
            ("TPValley", 1)
        ])
    else:
        itemPool = OrderedDict([
            ("EX1", 1),
            ("EX*", 167),
            ("KS", 40),
            ("MS", 11),
            ("AC", 0),
            ("EC", 3),
            ("HC", 0),
            ("WallJump", 1),
            ("ChargeFlame", 1),
            ("Dash", 1),
            ("Stomp", 1),
            ("DoubleJump", 1),
            ("Glide", 1),
            ("Bash", 1),
            ("Climb", 1),
            ("Grenade", 1),
            ("ChargeJump", 1),
            ("GinsoKey", 1),
            ("ForlornKey", 1),
            ("HoruKey", 1),
            ("Water", 1),
            ("Wind", 1),
            ("Warmth", 1),
            ("WaterVeinShard", 0),
            ("GumonSealShard", 0),
            ("SunstoneShard", 0),
            ("TPForlorn", 1),
            ("TPGrotto", 1),
            ("TPSorrow", 1),
            ("TPGrove", 1),
            ("TPSwamp", 1),
            ("TPValley", 1)
        ])

    plants = []
    if not includePlants:
        itemCount -= 24
        itemPool["EX*"] -= 24

    if shards:
        itemPool["WaterVeinShard"] = 5
        itemPool["GumonSealShard"] = 5
        itemPool["SunstoneShard"] = 5
        itemPool["GinsoKey"] = 0
        itemPool["ForlornKey"] = 0
        itemPool["HoruKey"] = 0
        itemPool["EX*"] -= 12

    if limitkeys:
        satisfied = False
        while not satisfied:
            ginso = random.randint(0, 12)
            if ginso == 12:
                ginso = 14
            forlorn = random.randint(0, 13)
            horu = random.randint(0, 14)
            if ginso != forlorn and ginso != horu and forlorn != horu and ginso + forlorn < 26:
                satisfied = True
        global keySpots
        keySpots = {limitKeysPool[ginso]: "GinsoKey", limitKeysPool[forlorn]: "ForlornKey",
                    limitKeysPool[horu]: "HoruKey"}
        itemPool["GinsoKey"] = 0
        itemPool["ForlornKey"] = 0
        itemPool["HoruKey"] = 0
        itemCount -= 3

    if noTeleporters:
        itemPool["TPForlorn"] = 0
        itemPool["TPGrotto"] = 0
        itemPool["TPSorrow"] = 0
        itemPool["TPGrove"] = 0
        itemPool["TPSwamp"] = 0
        itemPool["TPValley"] = 0
        itemPool["EX*"] += 6

    inventory = OrderedDict([
        ("EX1", 0),
        ("EX*", 0),
        ("KS", 0),
        ("MS", 0),
        ("AC", 0),
        ("EC", 1),
        ("HC", 3),
        ("WallJump", 0),
        ("ChargeFlame", 0),
        ("Dash", 0),
        ("Stomp", 0),
        ("DoubleJump", 0),
        ("Glide", 0),
        ("Bash", 0),
        ("Climb", 0),
        ("Grenade", 0),
        ("ChargeJump", 0),
        ("GinsoKey", 0),
        ("ForlornKey", 0),
        ("HoruKey", 0),
        ("Water", 0),
        ("Wind", 0),
        ("Warmth", 0),
        ("RB0", 0),
        ("RB1", 0),
        ("RB6", 0),
        ("RB8", 0),
        ("RB9", 0),
        ("RB10", 0),
        ("RB11", 0),
        ("RB12", 0),
        ("RB13", 0),
        ("RB15", 0),
        ("WaterVeinShard", 0),
        ("GumonSealShard", 0),
        ("SunstoneShard", 0),
        ("TPForlorn", 0),
        ("TPGrotto", 0),
        ("TPSorrow", 0),
        ("TPGrove", 0),
        ("TPSwamp", 0),
        ("TPValley", 0)
    ])

    # paired setup for subsequent players
    if playerID > 1:
        for item in sharedList:
            itemPool["EX*"] += itemPool[item]
            itemPool[item] = 0
        itemPool["EX*"] -= sharedMap[playerID]
        itemCount -= sharedMap[playerID]

    tree = XML.parse("areas.xml")
    root = tree.getroot()

    for child in root:
        area = Area(child.attrib["name"])
        areasRemaining.append(child.attrib["name"])

        for location in child.find("Locations"):
            loc = Location(int(location.find("X").text), int(location.find("Y").text), area.name,
                           location.find("Item").text, int(location.find("Difficulty").text),
                           location.find("Zone").text)
            if not includePlants:
                if re.match(".*Plant.*", area.name):
                    plants.append(loc)
                    continue
            area.add_location(loc)
            # location analysis setup
            if doLocationAnalysis:
                key = loc.to_string()
                if key not in locationAnalysis.keys():
                    locationAnalysis[key] = itemsToAnalyze.copy()
                zoneKey = loc.zone
                if zoneKey not in locationAnalysis.keys():
                    locationAnalysis[zoneKey] = itemsToAnalyze.copy()
        for conn in child.find("Connections"):
            connection = Connection(conn.find("Home").attrib["name"], conn.find("Target").attrib["name"])
            entranceConnection = conn.find("Entrance")
            if entrance and entranceConnection is not None:
                continue
            if not includePlants:
                if re.match(".*Plant.*", connection.target):
                    continue
            for req in conn.find("Requirements"):
                if req.attrib["mode"] in modes:
                    connection.add_requirements(req.text.split('+'), difficultyMap[req.attrib["mode"]])
            if connection.get_requirements():
                area.add_connection(connection)
        areas[area.name] = area

    if doLocationAnalysis:
        locationAnalysisCopy = {}
        for location in locationAnalysis:
            locationAnalysisCopy[location] = {}
            for item in locationAnalysis[location]:
                locationAnalysisCopy[location][item] = locationAnalysis[location][item]

    # flags line
    outputStr += (flags + "|" + str(seed) + "\n")

    if entrance:
        outputStr += randomize_entrances(areas)

    outputStr += ("-280256|EC|1|Glades\n")  # first energy cell
    outputStr += ("-1680104|EX|100|Grove\n")  # glitchy 100 orb at spirit tree
    outputStr += ("-12320248|EX|100|Forlorn\n")  # forlorn escape plant
    # the 2nd keystone in misty can get blocked by alt+R, so make it unimportant
    outputStr += ("-10440008|EX|100|Misty\n")

    if not includePlants:
        for location in plants:
            outputStr += (str(location.get_key()) + "|NO|0\n")

    locationsToAssign = []
    connectionQueue = []
    global reservedLocations
    reservedLocations = []

    skillCount = 10
    mapstonesAssigned = 0
    expSlots = itemPool["EX*"]

    global spoilerGroup
    spoilerGroup = {"MS": [], "KS": [], "EC": [], "HC": []}

    doorQueue = OrderedDict()
    mapQueue = OrderedDict()
    spoilerPath = ""

    reach_area("SunkenGladesRunaway")
    while itemCount > 0 or (balanced and balanceListLeftovers):

        balanceLevel += 1
        # open all paths that we can already access
        opening = True
        while opening:
            (opening, keys, mapstones) = open_free_connections()
            keystoneCount += keys
            mapstoneCount += mapstones
            if mapstoneCount == 8:
                mapstoneCount = 9
            if mapstoneCount == 10:
                mapstoneCount = 11
            for connection in connectionQueue:
                areas[connection[0]].remove_connection(connection[1])
            connectionQueue = []

        locationsToAssign = get_all_accessible_locations()
        # if there aren't any doors to open, it's time to get a new skill
        # consider -- work on stronger anti-key-lock logic so that we don't
        # have to give keys out right away (this opens up the potential of
        # using keys in the wrong place, will need to be careful)
        if not doorQueue and not mapQueue:
            spoilerPath = prepare_path(len(locationsToAssign))
            if not assignQueue:
                # we've painted ourselves into a corner, try again
                if not reservedLocations:
                    if playerID == 1:
                        sharedMap = {}
                    if depth > playerCount * playerCount:
                        return
                    return placeItems(seed, expPool, hardMode, includePlants, shardsMode, limitkeysMode, cluesMode,
                                      noTeleporters, doLocationAnalysis, doSkillOrderAnalysis, modes, flags,
                                      starvedMode, preferPathDifficulty, setNonProgressiveMapstones, playerCount,
                                      playerID, balanced, entrance, depth + 1)
                locationsToAssign.append(reservedLocations.pop(0))
                locationsToAssign.append(reservedLocations.pop(0))
                spoilerPath = prepare_path(len(locationsToAssign))
            if balanced:
                for item in assignQueue:
                    if len(balanceList) == 0:
                        break
                    locationsToAssign.append(get_location_from_balance_list())
        # pick what we're going to put in our accessible space
        itemsToAssign = []
        if len(locationsToAssign) < len(assignQueue) + max(keystoneCount - inventory["KS"], 0) + max(
                        mapstoneCount - inventory["MS"], 0):
            # we've painted ourselves into a corner, try again
            if not reservedLocations:
                if playerID == 1:
                    sharedMap = {}
                if depth > playerCount * playerCount:
                    return
                return placeItems(seed, expPool, hardMode, includePlants, shardsMode, limitkeysMode, cluesMode,
                                  noTeleporters, doLocationAnalysis, doSkillOrderAnalysis, modes, flags, starvedMode,
                                  preferPathDifficulty, setNonProgressiveMapstones, playerCount, playerID, balanced,
                                  entrance, depth + 1)
            locationsToAssign.append(reservedLocations.pop(0))
            locationsToAssign.append(reservedLocations.pop(0))
        for i in range(0, len(locationsToAssign)):
            if assignQueue:
                itemsToAssign.append(assign(assignQueue.pop(0)))
            elif inventory["KS"] < keystoneCount:
                itemsToAssign.append(assign("KS"))
            elif inventory["MS"] < mapstoneCount:
                itemsToAssign.append(assign("MS"))
            elif balanced and itemCount == 0:
                itemsToAssign.append(balanceListLeftovers.pop(0))
                itemCount += 1
            else:
                itemsToAssign.append(assign_random())
            itemCount -= 1

        # force assign things if using --prefer-path-difficulty
        if pathDifficulty:
            for item in list(itemsToAssign):
                if item in skillsOutput or item in eventsOutput:
                    preferred_difficulty_assign(item, locationsToAssign)
                    itemsToAssign.remove(item)

        # shuffle the items around and put them somewhere
        random.shuffle(itemsToAssign)
        for i in range(0, len(locationsToAssign)):
            assign_to_location(itemsToAssign[i], locationsToAssign[i])

        currentGroupSpoiler = ""

        if spoilerPath:
            currentGroupSpoiler += ("    Forced pickups: " + str(spoilerPath) + "\n")

        for skill in skillsOutput:
            if skill in spoilerGroup:
                for instance in spoilerGroup[skill]:
                    currentGroupSpoiler += "    " + instance
                if skill in seedDifficultyMap:
                    seedDifficulty += groupDepth * seedDifficultyMap[skill]

        for event in eventsOutput:
            if event in spoilerGroup:
                for instance in spoilerGroup[event]:
                    currentGroupSpoiler += "    " + instance

        for key in spoilerGroup:
            if key[:2] == "TP":
                for instance in spoilerGroup[key]:
                    currentGroupSpoiler += "    " + instance

        for instance in spoilerGroup["MS"]:
            currentGroupSpoiler += "    " + instance

        for instance in spoilerGroup["KS"]:
            currentGroupSpoiler += "    " + instance

        for instance in spoilerGroup["HC"]:
            currentGroupSpoiler += "    " + instance

        for instance in spoilerGroup["EC"]:
            currentGroupSpoiler += "    " + instance

        if currentGroupSpoiler:
            groupDepth += 1
            currentAreas.sort()

            spoilerStr += str(groupDepth) + ": " + str(currentAreas) + " {\n"

            spoilerStr += currentGroupSpoiler

            spoilerStr += "}\n"

        currentAreas = []

        # open all reachable doors (for the next iteration)
        for area in doorQueue.keys():
            if doorQueue[area].target not in areasReached:
                difficulty = doorQueue[area].cost()[2]
                seedDifficulty += difficulty * difficulty
            reach_area(doorQueue[area].target)
            if doorQueue[area].target in areasRemaining:
                areasRemaining.remove(doorQueue[area].target)
            areas[area].remove_connection(doorQueue[area])

        for area in mapQueue.keys():
            if mapQueue[area].target not in areasReached:
                difficulty = mapQueue[area].cost()[2]
                seedDifficulty += difficulty * difficulty
            reach_area(mapQueue[area].target)
            if mapQueue[area].target in areasRemaining:
                areasRemaining.remove(mapQueue[area].target)
            areas[area].remove_connection(mapQueue[area])

        locationsToAssign = []
        spoilerGroup = {"MS": [], "KS": [], "EC": [], "HC": []}

        doorQueue = OrderedDict()
        mapQueue = OrderedDict()
        spoilerPath = ""

    if balanced:
        for item in balanceList:
            outputStr += item[2]

    spoilerStr = flags + "|" + str(seed) + "\n" + "Difficulty Rating: " + str(seedDifficulty) + "\n" + spoilerStr
    random.shuffle(eventList)
    for event in eventList:
        outputStr += event

    if doLocationAnalysis:
        locationAnalysis = locationAnalysisCopy

    return outputStr, spoilerStr


def main():
    global random

    random = Random()

    parser = argparse.ArgumentParser()
    parser.add_argument("--preset", help="Choose a preset group of paths for the generator to use",
                        choices=["casual", "standard", "expert", "master", "hard", "ohko", "0xp", "glitched"])
    parser.add_argument("--custom-logic",
                        help="Customize paths that the generator will use, comma-separated: normal,speed,dbash,extended,extended-damage,lure,speed-lure,lure-hard,dboost,dboost-light,dboost-hard,cdash,cdash-farming,extreme,timed-level,glitched")
    parser.add_argument("--seed", help="Seed number (default 1)", type=int, default=1)
    parser.add_argument("--count", help="Number of seeds to generate (default 1)", type=int, default=1)
    parser.add_argument("--hard", help="Enable hard mode", action="store_true")
    parser.add_argument("--ohko", help="Enable one-hit-ko mode", action="store_true")
    parser.add_argument("--zeroxp", help="Enable 0xp mode", action="store_true")
    parser.add_argument("--nobonus", help="Remove bonus powerups from the item pool", action="store_true")
    parser.add_argument("--noplants", help="Ignore petrified plants when assigning items", action="store_true")
    parser.add_argument("--starved", help="Reduces the rate at which skills will appear when not required to advance",
                        action="store_true")
    parser.add_argument("--shards",
                        help="The Water Vein, Gumon Seal, and Sunstone will be awarded after 3/5 shards are found",
                        action="store_true")
    parser.add_argument("--limitkeys",
                        help="The Water Vein, Gumon Seal, and Sunstone will only appear at skill trees or event sources",
                        action="store_true")
    parser.add_argument("--clues",
                        help="For each 3 trees visited, the location of a random dungeon key will be revealed",
                        action="store_true")
    parser.add_argument("--non-progressive-mapstones",
                        help="Map Stones will retain their behaviour from before v1.2, having their own unique drops",
                        action="store_true")
    parser.add_argument("--force-trees",
                        help="Prevent Ori from entering the final escape room until all skill trees have been visited",
                        action="store_true");
    parser.add_argument("--exp-pool", help="Size of the experience pool (default 10000)", type=int, default=10000)
    parser.add_argument("--prefer-path-difficulty",
                        help="Increase the chances of putting items in more convenient (easy) or less convenient (hard) locations",
                        choices=["easy", "hard"])
    parser.add_argument("--no-teleporters", help="Remove teleporter activation pickups from the pool",
                        action="store_true")
    parser.add_argument("--analysis", help="Report stats on the skill order for all seeds generated",
                        action="store_true")
    parser.add_argument("--loc-analysis", help="Report stats on where skills are placed over multiple seeds",
                        action="store_true")
    parser.add_argument("--players", help="Player count for paired randomizer", type=int, default=1)
    parser.add_argument("--sync-id", help="Team identifier number for paired randomizer", type=int)
    parser.add_argument("--shared-items",
                        help="What will be shared by sync, comma-separated: skills,keys,events,teleporters,upgrades",
                        default="skills,keys")
    parser.add_argument("--share-mode",
                        help="How the server will handle shared pickups, one of: shared,swap,split,none",
                        default="shared")
    parser.add_argument("--balanced", help="Reduce the value of newly discovered locations for progression placements",
                        action="store_true")
    parser.add_argument("--entrance", help="Randomize entrances", action="store_true")

    args = parser.parse_args()

    includePlants = not args.noplants

    presets = {
        "casual": ["normal", "dboost-light"],
        "standard": ["normal", "speed", "lure", "dboost-light"],
        "dboost": ["normal", "speed", "lure", "dboost", "dboost-light"],
        "expert": ["normal", "speed", "lure", "speed-lure", "dboost", "dboost-light", "cdash", "extended",
                   "extended-damage"],
        "master": ["normal", "speed", "lure", "speed-lure", "dboost", "dboost-light", "dboost-hard", "cdash", "dbash",
                   "extended", "extended-damage", "lure-hard", "extreme"],
        "hard": ["normal", "speed", "lure", "dboost-light", "cdash", "dbash", "extended"],
        "ohko": ["normal", "speed", "lure", "cdash", "dbash", "extended"],
        "0xp": ["normal", "speed", "lure", "dboost-light"],
        "glitched": ["normal", "speed", "lure", "speed-lure", "dboost", "dboost-light", "dboost-hard", "cdash", "dbash",
                     "extended", "lure-hard", "timed-level", "glitched", "extended-damage", "extreme"]
    }

    if args.preset:
        mode = args.preset
        modes = presets[args.preset]
    if args.custom_logic:
        mode = "custom"
        modes = args.custom_logic.split(',')

    # initialize pairs
    global sharedMap
    global sharedList

    sharedMap = {}
    sharedList = []

    if args.players > 1:
        sharedItems = args.shared_items.split(",")
        if "skills" in sharedItems:
            for skill in list(skillsOutput):
                sharedList.append(skill)
        if "keys" in sharedItems:
            if args.shards:
                sharedList.append("WaterVeinShard")
                sharedList.append("GumonSealShard")
                sharedList.append("SunstoneShard")
            else:
                sharedList.append("GinsoKey")
                sharedList.append("ForlornKey")
                sharedList.append("HoruKey")
        if "events" in sharedItems:
            sharedList.append("Water")
            sharedList.append("Wind")
            sharedList.append("Warmth")
        if "teleporters" in sharedItems:
            sharedList.append("TPForlorn")
            sharedList.append("TPGrotto")
            sharedList.append("TPSorrow")
            sharedList.append("TPGrove")
            sharedList.append("TPSwamp")
            sharedList.append("TPValley")
        if "upgrades" in sharedItems:
            sharedList.append("RB6")
            sharedList.append("RB8")
            sharedList.append("RB9")
            sharedList.append("RB10")
            sharedList.append("RB11")
            sharedList.append("RB12")
            sharedList.append("RB13")
            sharedList.append("RB15")

    flags = ""
    syncFlags = ""
    flags += mode
    if args.limitkeys:
        flags += ",limitkeys"
    if args.shards:
        flags += ",shards"
    if args.clues:
        flags += ",clues"
    if args.starved:
        flags += ",starved"
    if args.prefer_path_difficulty:
        flags += ",prefer_path_difficulty=" + args.prefer_path_difficulty
    if args.hard:
        flags += ",hard"
    if args.ohko:
        flags += ",OHKO"
    if args.zeroxp:
        flags += ",0XP"
    if args.nobonus:
        flags += ",NoBonus"
    if args.noplants:
        flags += ",NoPlants"
    if args.force_trees:
        flags += ",ForceTrees"
    if args.non_progressive_mapstones:
        flags += ",NonProgressMapStones"
    if args.no_teleporters:
        flags += ",NoTeleporters"
    if args.balanced:
        flags += ",balanced"
    if args.entrance:
        flags += ",entrance"
    if args.players > 1:
        syncFlags += ",shared=" + "+".join(args.shared_items.split(","))
        syncFlags += ",mode=" + args.share_mode
        sync_id = args.sync_id
        if not sync_id:
            sync_id = int(time.time() * 1000 % 1073741824)
        syncFlags += ",sync" + str(sync_id)

    global skillAnalysis
    global itemsToAnalyze
    global locationAnalysis

    skillAnalysis = {
        "WallJump": 0,
        "ChargeFlame": 0,
        "DoubleJump": 0,
        "Bash": 0,
        "Stomp": 0,
        "Glide": 0,
        "Climb": 0,
        "ChargeJump": 0,
        "Dash": 0,
        "Grenade": 0
    }

    itemsToAnalyze = {
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
        "TPValley": 0
    }

    locationAnalysis = {}
    for i in range(1, 10):
        locationAnalysis["MapStone " + str(i)] = itemsToAnalyze.copy()

    for seedOffset in range(0, args.count):

        seed = args.seed + seedOffset
        random.seed(seed)

        placements = placeItemsMulti(seed, args.exp_pool, args.hard, includePlants, args.shards, args.limitkeys,
                                     args.clues, args.no_teleporters, args.loc_analysis, args.analysis, modes, flags,
                                     syncFlags, args.starved, args.prefer_path_difficulty,
                                     args.non_progressive_mapstones, args.players, args.balanced, args.entrance)

        if not args.analysis and not args.loc_analysis:
            for player in range(0, playerCount):
                with open("randomizer_" + mode + str(seed) + "_" + str(player + 1) + ".dat", 'w') as output:
                    output.write(placements[player][0])

                with open("spoiler_" + mode + str(seed) + "_" + str(player + 1) + ".txt", 'w') as spoiler:
                    spoiler.write(placements[player][1])

    if args.analysis:
        for key, item in skillAnalysis.items():
            print(key, item)

    if args.loc_analysis:
        print(
            "location,WallJump,ChargeFlame,DoubleJump,Bash,Stomp,Glide,Climb,ChargeJump,Dash,Grenade,GinsoKey,"
            "ForlornKey,HoruKey,Water,Wind,WaterVeinShard,GumonSealShard,SunstoneShard,TPForlorn,TPGrotto,TPSorrow,"
            "TPGrove,TPSwamp,TPValley")
        for key in locationAnalysis.keys():
            line = key + ","
            for item in list(itemsToAnalyze):
                line += str(locationAnalysis[key][item]) + ","
            print(line)


if __name__ == "__main__":
    main()
