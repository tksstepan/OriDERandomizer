#!/usr/bin/env python

import re
import math
import xml.etree.ElementTree as XML
import argparse
import time
from collections import OrderedDict

#A custom implementation of a Mersenne Twister
#(since javascript hates everything)
#https://en.wikipedia.org/wiki/Mersenne_Twister
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

        self.index = self.index + 1

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
        if args.shards:
            match = re.match(".*GinsoKey.*", str(req))
            if match:
                req.remove("GinsoKey")
                req.append("WaterVeinShard")
                req.append("WaterVeinShard")
                req.append("WaterVeinShard")
                req.append("WaterVeinShard")
                req.append("WaterVeinShard")
            match = re.match(".*ForlornKey.*", str(req))
            if match:
                req.remove("ForlornKey")
                req.append("GumonSealShard")
                req.append("GumonSealShard")
                req.append("GumonSealShard")
                req.append("GumonSealShard")
                req.append("GumonSealShard")
            match = re.match(".*HoruKey.*", str(req))
            if match:
                req.remove("HoruKey")
                req.append("SunstoneShard")
                req.append("SunstoneShard")
                req.append("SunstoneShard")
                req.append("SunstoneShard")
                req.append("SunstoneShard")
        self.requirements.append(req)
        self.difficulties.append(difficulty)
        match = re.match(".*KS.*KS.*KS.*KS.*", str(req))
        if match:
            self.keys = 4
            return
        match = re.match(".*KS.*KS.*", str(req))
        if match:
            self.keys = 2
            return
        match = re.match(".*MS.*", str(req))
        if match:
            self.mapstone = True
            return

    def get_requirements(self):
        return self.requirements

    def cost(self):
        minReqScore = 7777
        minDiff = 7777
        minReq = []
        for i in range(0,len(self.requirements)):
            score = 0
            energy = 0
            health = 0
            ability = 0
            for abil in self.requirements[i]:
                if abil == "EC":
                    energy += 1
                    if inventory["EC"] < energy:
                        score += costs[abil.strip()]
                elif abil == "HC":
                    health += 1
                    if inventory["HC"] < health:
                        score += costs[abil.strip()]
                elif abil == "AC":
                    ability += 1
                    if inventory["AC"] < health:
                        score += costs[abil.strip()]
                elif abil == "MS":
                    if inventory["MS"] < mapstonesSeen:
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
        self.x = int(math.floor((x)/self.factor) * self.factor)
        self.y = int(math.floor((y)/self.factor) * self.factor)
        self.orig = orig
        self.area = area
        self.difficulty = difficulty
        self.zone = zone

    def get_key(self):
        return self.x*10000 + self.y

    def to_string(self):
        return self.area + " " + self.orig + " (" + str(self.x) + " " + str(self.y) + ")"

class Door:

    factor = 4.0

    def __init__(self, name, x, y):
        self.x = x
        self.y = y
        self.name = name

    def get_key(self):
        return int(math.floor(self.x/self.factor) * self.factor)*10000 + int(math.floor(self.y/self.factor) * self.factor)

class Generator:

    def reach_area(self, target):
        if self.playerID > 1 and target in self.sharedMap:
            for sharedItem in self.sharedMap[target]:
                if sharedItem[1] == self.playerID:
                    self.assignQueue.append(sharedItem[0])
                    self.itemCount += 1
                else:
                    self.assign(sharedItem[0])
                    if sharedItem[0] not in self.spoilerGroup:
                        self.spoilerGroup[sharedItem[0]] = []
                    self.spoilerGroup[sharedItem[0]].append(sharedItem[0] + " from Player " + str(sharedItem[1]) + "\n")
        self.currentAreas.append(target)
        self.areasReached[target] = True

    def open_free_connections(self):
        global mapstonesSeen
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
                    elif connection.mapstone and not args.free_mapstones:
                        if not reached:
                            visitMap = True
                            for map in self.mapQueue.keys():
                                if map == area or self.mapQueue[map].target == connection.target:
                                    visitMap = False
                            if visitMap:
                                self.mapQueue[area] = connection
                                mapstoneCount += 1
                    else:
                        if not reached:
                            self.seedDifficulty += cost[2] * cost[2]
                            self.reach_area(connection.target)
                            # only reached if args.free_mapstones is true
                            if connection.mapstone:
                                mapstonesSeen += 1
                                if mapstonesSeen >= 9:
                                    mapstonesSeen = 11
                                if mapstonesSeen == 8:
                                    mapstonesSeen = 9
                        if connection.target in self.areasRemaining:
                            self.areasRemaining.remove(connection.target)
                        self.connectionQueue.append((area, connection))
                        found = True
        return (found, keystoneCount, mapstoneCount)


    def get_all_accessible_locations(self):
        locations = []
        for area in self.areasReached.keys():
            currentLocations = self.areas[area].get_locations()
            for location in currentLocations:
                location.difficulty += self.areas[area].difficulty
            if args.limitkeys:
                loc = ""
                for location in currentLocations:
                    if location.orig in self.keySpots.keys():
                        loc = location
                        break
                if loc:
                    self.force_assign(self.keySpots[loc.orig], loc)
                    currentLocations.remove(loc)
            locations.extend(currentLocations)
            self.areas[area].clear_locations()
        if self.reservedLocations:
            locations.append(self.reservedLocations.pop(0))
            locations.append(self.reservedLocations.pop(0))
        if self.itemCount > 2 and len(locations) >= 2:
            self.reservedLocations.append(locations.pop(random.randrange(len(locations))))
            self.reservedLocations.append(locations.pop(random.randrange(len(locations))))
        return locations


    def prepare_path(self, free_space):
        abilities_to_open = OrderedDict()
        totalCost = 0.0
        free_space += len(self.balanceList)
        # find the sets of abilities we need to get somewhere
        for area in self.areasReached.keys():
            for connection in self.areas[area].get_connections():
                if connection.target in self.areasReached:
                    continue
                if args.limitkeys and connection.get_requirements() and ("GinsoKey" in connection.get_requirements()[0] or "ForlornKey" in connection.get_requirements()[0] or "HoruKey" in connection.get_requirements()[0]):
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
                        if costs[req] > 0:
                            # for paired randomizer -- if the item isn't yours to assign, skip connection
                            if self.itemPool[req] == 0:
                                requirements = []
                                break
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
                                if args.easy and self.itemPool[req] > 1:
                                    requirements.append(req)
                    # decrease the rate of multi-item paths
                    cost *= max(1, len(requirements) - 1)
                    if len(requirements) <= free_space:
                        for req in requirements:
                            if req not in abilities_to_open:
                                abilities_to_open[req] = (cost, requirements)
                            elif abilities_to_open[req][0] > cost:
                                abilities_to_open[req] = (cost, requirements)
        # pick a random path weighted by cost
        for path in abilities_to_open:
            totalCost += 1.0/abilities_to_open[path][0]
        position = 0
        target = random.random() * totalCost
        path_selected = None
        for path in abilities_to_open:
            position += 1.0/abilities_to_open[path][0]
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
        target = int(pow(random.random(), 1.0 / self.balanceLevel) * len(self.balanceList))
        location = self.balanceList.pop(target)
        self.balanceListLeftovers.append(location[0])
        return location[1]

    def assign_random(self, recurseCount = 0):
        value = random.random()
        position = 0.0
        for key in self.itemPool.keys():
            position += self.itemPool[key]/self.itemCount
            if value <= position:
                if args.starved and key in self.skillsOutput and recurseCount < 3:
                    return self.assign_random(recurseCount = recurseCount + 1)
                return self.assign(key)

    def assign(self, item):
        self.itemPool[item] = max(self.itemPool[item]-1,0)
        if item == "KS":
            if costs[item] > 0:
                costs[item] -= 2
        # MS not included here since it doesn't really make sense to decrease their cost as you get more
        elif item in ["EC", "HC", "AC"]:
            if costs[item] > 0:
                costs[item] -= 1
        elif item in ["WaterVeinShard", "GumonSealShard", "SunstoneShard"]:
            if costs[item] > 0:
                costs[item] -= 1
        elif item in costs.keys() and self.itemPool[item] == 0:
            costs[item] = 0
        inventory[item] += 1
        return item

    # for use in limitkeys mode
    def force_assign(self, item, location):

        self.assign(item)
        self.assign_to_location(item, location)

    # for use in world tour mode
    def relic_assign(self, location):

        self.force_assign("Relic", location)
        self.areas[location.area].remove_location(location)

    def choose_relic_for_zone(self, zone):

        from relics import relics
        random.shuffle(relics[zone])
        return relics[zone][0]

    def assign_to_location(self, item, location):

        assignment = ""
        zone = location.zone
        value = 0

        # if this is the first player of a paired seed, construct the map
        if args.players > 1 and self.playerID == 1 and item in self.sharedList:
            player = random.randint(1,args.players)
            if location.area not in self.sharedMap:
                self.sharedMap[location.area] = []
            self.sharedMap[location.area].append((item, player))

            if player is not self.playerID:
                if player not in self.sharedMap:
                    self.sharedMap[player] = 0
                self.sharedMap[player] += 1
                if item not in self.spoilerGroup:
                    self.spoilerGroup[item] = []
                self.spoilerGroup[item].append(item + " from Player " + str(player) + "\n")
                item = "EX*"
                self.expSlots += 1

        # if mapstones are progressive, set a special location
        if not args.non_progressive_mapstones and location.orig == "MapStone":
            self.mapstonesAssigned += 1
            assignment += (str(20 + self.mapstonesAssigned * 4) + "|")
            zone = "Mapstone"
            if item in costs.keys():
                if item not in self.spoilerGroup:
                    self.spoilerGroup[item] = []
                self.spoilerGroup[item].append(item + " from MapStone " + str(self.mapstonesAssigned) + "\n")
        else:
            assignment += (str(location.get_key()) + "|")
            if item in costs.keys():
                if item not in self.spoilerGroup:
                    self.spoilerGroup[item] = []
                self.spoilerGroup[item].append(item + " from " + location.to_string() + "\n")

        if item in self.skillsOutput:
            assignment += (str(self.skillsOutput[item][:2]) + "|" + self.skillsOutput[item][2:])
            if args.analysis:
                skillAnalysis[item] += self.skillCount
                self.skillCount -= 1
        elif item in self.eventsOutput:
            assignment += (str(self.eventsOutput[item][:2]) + "|" + self.eventsOutput[item][2:])
        elif item == "Relic":
            relic = self.choose_relic_for_zone(zone)
            assignment += "WT|#" + relic[0] + "#\\n" + relic[1]
        elif item == "EX*":
            value = self.get_random_exp_value()
            self.expRemaining -= value
            self.expSlots -= 1
            assignment += "EX|" + str(value)
        elif item[2:]:
            assignment += (item[:2] + "|" + item[2:])
        else:
            assignment += (item[:2] + "|1")
        assignment += ("|" + zone + "\n")

        if item in self.eventsOutput:
            self.eventList.append(assignment)
        elif args.balanced and item not in costs.keys() and location.orig != "MapStone":
            if value > 0:
                item = "EX" + str(value)
            self.balanceList.append((item, location, assignment))
        else:    
            self.outputStr += assignment

        if args.loc_analysis:
            key = location.to_string()
            if location.orig == "MapStone":
                key = "MapStone " + str(self.mapstonesAssigned)
            if item in locationAnalysisCopy[key]:
                locationAnalysisCopy[key][item] += 1
                locationAnalysisCopy[location.zone][item] += 1

    def get_random_exp_value(self):

        min = random.randint(2,9)

        if self.expSlots <= 1:
            return max(self.expRemaining,min)

        return int(max(self.expRemaining * (inventory["EX*"] + self.expSlots / 4) * random.uniform(0.0,2.0) / (self.expSlots * (self.expSlots + inventory["EX*"])), min))

    def preferred_difficulty_assign(self, item, locationsToAssign):
        total = 0.0
        for loc in locationsToAssign:
            if pathDifficulty == "easy":
                total += (20 - loc.difficulty) * (20 - loc.difficulty)
            else:
                total += (loc.difficulty * loc.difficulty)
        value = random.random()
        position = 0.0
        for i in range(0,len(locationsToAssign)):
            if pathDifficulty == "easy":
                position += (20 - locationsToAssign[i].difficulty) * (20 - locationsToAssign[i].difficulty)/total
            else:
                position += locationsToAssign[i].difficulty * locationsToAssign[i].difficulty/total
            if value <= position:
                self.assign_to_location(item, locationsToAssign[i])
                break
        del locationsToAssign[i]

    def connect_doors(self, door1, door2, requirements=["Free"]):
        connection1 = Connection(door1.name, door2.name)
        connection1.add_requirements(requirements, 1)
        self.areas[door1.name].add_connection(connection1)
        connection2 = Connection(door2.name, door1.name)
        connection2.add_requirements(requirements, 1)
        self.areas[door2.name].add_connection(connection2)
        return str(door1.get_key()) + "|EN|" + str(door2.x) + "|" + str(door2.y) + "\n" + str(door2.get_key()) + "|EN|" + str(door1.x) + "|" + str(door1.y) + "\n";

    def randomize_entrances(self):

        tree = XML.parse("doors.xml")
        root = tree.getroot()

        outerDoors = [[],[],[],[],[],[],[],[],[],[],[],[],[]]
        innerDoors = [[],[],[],[],[],[],[],[],[],[],[],[],[]]

        for child in root:
            inner = child.find("Inner")
            innerDoors[int(inner.find("Group").text)].append(Door(child.attrib["name"] + "InnerDoor", int(inner.find("X").text), int(inner.find("Y").text)))

            outer = child.find("Outer")
            outerDoors[int(outer.find("Group").text)].append(Door(child.attrib["name"] + "OuterDoor", int(outer.find("X").text), int(outer.find("Y").text)))

        random.shuffle(outerDoors[0])
        random.shuffle(innerDoors[12])

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

        random.shuffle(targets)

        horuEntryGroup = random.randint(4,9)
        if horuEntryGroup >= 7:
            horuEntryGroup += 2
        if horuEntryGroup == 11:
            horuEntryGroup = 1
            if random.random() > 0.5:
                doorStr += self.connect_doors(firstDoors[0], innerDoors[1].pop(0))
                outerDoors[0].append(firstDoors[1])
            else:
                doorStr += self.connect_doors(firstDoors[0], outerDoors[1].pop(0))
                outerDoors[0].append(firstDoors[1])
                outerDoors[0].append(innerDoors[1].pop(0))
        else:
            requirements = ["Free"]
            if firstDoors[1].name == "GinsoDoorOuter":
                requirements = ["GinsoKey"]
            if firstDoors[1].name == "ForlornDoorOuter":
                requirements = ["ForlornKey"]
            doorStr += self.connect_doors(firstDoors[0], outerDoors[horuEntryGroup].pop(0), requirements)
            doorStr += self.connect_doors(firstDoors[1], innerDoors[horuEntryGroup-1].pop(0))
            targets.remove(horuEntryGroup-1)

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
                activeGroups.append(target+1)

            if (target == 6 and 10 not in targets) or (target == 10 and 6 not in targets):
                activeGroups.append(12)

            doorStr += self.connect_doors(outerDoors[group].pop(0), innerDoors[target].pop(0))

        lastDoorIndex = 0

        for group in range(13):
            if innerDoors[group]:
                doorStr += self.connect_doors(innerDoors[group].pop(0), lastDoors[lastDoorIndex])
                lastDoorIndex += 1
            if outerDoors[group]:
                doorStr += self.connect_doors(outerDoors[group].pop(0), lastDoors[lastDoorIndex])
                lastDoorIndex += 1

        return doorStr


    def placeItemsMulti(self, seed, args, modes, flags, syncFlags):

        placements = []

        # initialize pairs
        self.sharedMap = {}
        self.sharedList = []

        if args.players > 1:
            sharedItems = args.shared_items.split(",")
            if "skills" in sharedItems:
                self.sharedList.append("WallJump")
                self.sharedList.append("ChargeFlame")
                self.sharedList.append("Dash")
                self.sharedList.append("Stomp")
                self.sharedList.append("DoubleJump")
                self.sharedList.append("Glide")
                self.sharedList.append("Bash")
                self.sharedList.append("Climb")
                self.sharedList.append("Grenade")
                self.sharedList.append("ChargeJump")
            if "keys" in sharedItems:
                if args.shards:
                    self.sharedList.append("WaterVeinShard")
                    self.sharedList.append("GumonSealShard")
                    self.sharedList.append("SunstoneShard")
                else:
                    self.sharedList.append("GinsoKey")
                    self.sharedList.append("ForlornKey")
                    self.sharedList.append("HoruKey")
            if "events" in sharedItems:
                self.sharedList.append("Water")
                self.sharedList.append("Wind")
                self.sharedList.append("Warmth")
            if "teleporters" in sharedItems:
                self.sharedList.append("TPForlorn")
                self.sharedList.append("TPGrotto")
                self.sharedList.append("TPSorrow")
                self.sharedList.append("TPGrove")
                self.sharedList.append("TPSwamp")
                self.sharedList.append("TPValley")
                self.sharedList.append("TPGinso")
                self.sharedList.append("TPHoru")
            if "upgrades" in sharedItems:
                self.sharedList.append("RB6")
                self.sharedList.append("RB8")
                self.sharedList.append("RB9")
                self.sharedList.append("RB10")
                self.sharedList.append("RB11")
                self.sharedList.append("RB12")
                self.sharedList.append("RB13")
                self.sharedList.append("RB15")

        self.playerID = 1
        playerFlags = flags

        if syncFlags:
            playerFlags = flags + syncFlags + "." + str(self.playerID)

        placement = self.placeItems(seed, args, modes, flags)
        if not placement:
            return self.placeItemsMulti(seed, args, modes, flags, syncFlags)
        placements.append(placement)
        while self.playerID < args.players:
            self.playerID += 1
            if syncFlags:
                playerFlags = flags + syncFlags + "." + str(self.playerID)
            placement = self.placeItems(seed, args, modes, flags)
            if not placement:
                return self.placeItemsMulti(seed, args, modes, flags, syncFlags)
            placements.append(placement)

        return placements

    def placeItems(self, seed, args, modes, flags, depth=0):

        global costs
        global inventory

        global locationAnalysis
        global locationAnalysisCopy

        self.balanceLevel = 0
        self.balanceList = []
        self.balanceListLeftovers = []

        self.skillsOutput = {
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

        self.eventsOutput = {
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

        seedDifficultyMap = {
            "Dash": 2,
            "Bash": 2,
            "Glide": 3,
            "DoubleJump": 2,
            "ChargeJump": 1
        }
        self.seedDifficulty = 0

        limitKeysPool = ["SKWallJump", "SKChargeFlame", "SKDash", "SKStomp", "SKDoubleJump", "SKGlide", "SKClimb", "SKGrenade", "SKChargeJump", "EVGinsoKey", "EVForlornKey", "EVHoruKey", "SKBash", "EVWater", "EVWind"]

        difficultyMap = {
            "casual-core": 1,
            "casual-dboost": 1,
            "standard-core": 2,
            "standard-lure": 2,
            "standard-dboost": 2,
            "standard-abilities": 2,
            "expert-core": 3,
            "expert-lure": 3,
            "expert-dboost": 3,
            "expert-abilities": 2,
            "dbash": 3,
            "master-core": 4,
            "master-lure": 4,
            "master-dboost": 4,
            "master-abilities": 3,
            "gjump": 4,
            "glitched": 5,
            "timed-level": 5,
            "insane": 5
        }

        self.outputStr = ""
        self.eventList = []
        spoilerStr = ""
        groupDepth = 0

        costs = {
            "Free": 0,
            "MS": 0,
            "KS": 4,
            "AC": 12,
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
            "TPValley": 90,
            "TPGinso": 150,
            "TPHoru": 180,
            "Open": 1,
            "Relic": 1
        }

        if args.free_mapstones:
            costs["MS"] = 11

        # we use OrderedDicts here because the order of a dict depends on the size of the dict and the hash of the keys
        # since python 3.3 the order of a given dict is also dependent on the random hash seed for the current Python invocation
        #     which apparently ignores our random.seed()
        # https://stackoverflow.com/questions/15479928/why-is-the-order-in-dictionaries-and-sets-arbitrary/15479974#15479974
        # Note that as of Python 3.3, a random hash seed is used as well, making hash collisions unpredictable
        # to prevent certain types of denial of service (where an attacker renders a Python server unresponsive
        # by causing mass hash collisions). This means that the order of a given dictionary is then also
        # dependent on the random hash seed for the current Python invocation.

        self.areas = OrderedDict()

        self.areasReached = OrderedDict([])
        self.currentAreas = []
        self.areasRemaining = []
        self.connectionQueue = []
        self.assignQueue = []

        self.itemCount = 253.0
        self.expRemaining = args.exp_pool
        keystoneCount = 0
        mapstoneCount = 0

        if not args.hard:
            self.itemPool = OrderedDict([
                ("EX1", 1),
                ("EX*", 101),
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
                ("RB8", 0),
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
                ("TPValley", 1),
                ("TPGinso", 0),
                ("TPHoru", 0),
                ("Open", 0),
                ("Relic", 0)
            ])
        else:
            self.itemPool = OrderedDict([
                ("EX1", 1),
                ("EX*", 176),
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
                ("TPValley", 1),
                ("TPGinso", 0),
                ("TPHoru", 0),
                ("Open", 0),
                ("Relic", 0)
            ])

        if args.easy:
            self.itemPool["EX*"] -= 9
            self.itemPool["DoubleJump"] += 1
            self.itemPool["Bash"] += 1
            self.itemPool["Stomp"] += 1
            self.itemPool["Glide"] += 1
            self.itemPool["ChargeJump"] += 1
            self.itemPool["Dash"] += 1
            self.itemPool["Grenade"] += 1
            self.itemPool["Water"] += 1
            self.itemPool["Wind"] += 1

        plants = []
        if args.noplants:
            self.itemCount -= 24
            self.itemPool["EX*"] -= 24

        if args.shards:
            self.itemPool["WaterVeinShard"] = 5
            self.itemPool["GumonSealShard"] = 5
            self.itemPool["SunstoneShard"] = 5
            self.itemPool["GinsoKey"] = 0
            self.itemPool["ForlornKey"] = 0
            self.itemPool["HoruKey"] = 0
            self.itemPool["EX*"] -= 12

        if args.limitkeys:
            satisfied = False
            while not satisfied:
                ginso = random.randint(0,12)
                if ginso == 12:
                    ginso = 14
                forlorn = random.randint(0,13)
                horu = random.randint(0,14)
                if ginso != forlorn and ginso != horu and forlorn != horu and ginso+forlorn < 26:
                    satisfied = True
            self.keySpots = {limitKeysPool[ginso]:"GinsoKey", limitKeysPool[forlorn]:"ForlornKey", limitKeysPool[horu]:"HoruKey"}
            self.itemPool["GinsoKey"] = 0
            self.itemPool["ForlornKey"] = 0
            self.itemPool["HoruKey"] = 0
            self.itemCount -= 3

        if args.open:
            self.itemPool["TPGinso"] = 1
            self.itemPool["TPHoru"] = 1
            self.itemPool["KS"] -= 2

        if args.no_teleporters:
            self.itemPool["TPForlorn"] = 0
            self.itemPool["TPGrotto"] = 0
            self.itemPool["TPSorrow"] = 0
            self.itemPool["TPGrove"] = 0
            self.itemPool["TPSwamp"] = 0
            self.itemPool["TPValley"] = 0
            self.itemPool["TPGinso"] = 0
            self.itemPool["TPHoru"] = 0
            self.itemPool["EX*"] += 6
            if args.open:
                self.itemPool["EX*"] += 2

        if args.world_tour:
            self.itemPool["EX*"] -= 11
            self.itemPool["Relic"] += 11

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
            ("TPValley", 0),
            ("TPGinso", 0),
            ("TPHoru", 0),
            ("Open", 0),
            ("Relic", 0)
        ])

        if args.open:
            inventory["Open"] = 1
            costs["Open"] = 0

        # paired setup for subsequent players
        if self.playerID > 1:
            for item in self.sharedList:
                self.itemPool["EX*"] += self.itemPool[item]
                self.itemPool[item] = 0
            self.itemPool["EX*"] -= self.sharedMap[self.playerID]
            self.itemCount -= self.sharedMap[self.playerID]

        tree = XML.parse("areas.xml")
        root = tree.getroot()

        for child in root:
            area = Area(child.attrib["name"])
            self.areasRemaining.append(child.attrib["name"])

            for location in child.find("Locations"):
                loc = Location(int(location.find("X").text), int(location.find("Y").text), area.name, location.find("Item").text, int(location.find("Difficulty").text), location.find("Zone").text)
                if args.noplants:
                    if re.match(".*Plant.*", area.name):
                        plants.append(loc)
                        continue
                area.add_location(loc)
                # location analysis setup
                if args.loc_analysis:
                    key = loc.to_string()
                    if key not in locationAnalysis.keys():
                        locationAnalysis[key] = itemsToAnalyze.copy()
                        locationAnalysis[key]["Zone"] = loc.zone
                    zoneKey = loc.zone
                    if zoneKey not in locationAnalysis.keys():
                        locationAnalysis[zoneKey] = itemsToAnalyze.copy()
                        locationAnalysis[zoneKey]["Zone"] = loc.zone
            for conn in child.find("Connections"):
                connection = Connection(conn.find("Home").attrib["name"], conn.find("Target").attrib["name"])
                entranceConnection = conn.find("Entrance")
                if args.entrance and entranceConnection is not None:
                    continue
                if args.noplants:
                    if re.match(".*Plant.*", connection.target):
                        continue
                for req in conn.find("Requirements"):
                    if req.attrib["mode"] in modes:
                        connection.add_requirements(req.text.split('+'), difficultyMap[req.attrib["mode"]])
                if connection.get_requirements():
                    area.add_connection(connection)
            self.areas[area.name] = area

        if args.loc_analysis:
            locationAnalysisCopy = {}
            for location in locationAnalysis:
                locationAnalysisCopy[location] = {}
                for item in locationAnalysis[location]:
                    locationAnalysisCopy[location][item] = locationAnalysis[location][item]
        
        # flags line
        self.outputStr += (flags + "|" + str(seed) + "\n")

        if args.entrance:
            self.outputStr += self.randomize_entrances()

        if args.world_tour:
            # accumulate a list of locations per zone
            # exclude limitkeys locations if applicable
            locations_by_zone = OrderedDict([
                ("Glades", []),
                ("Grove", []),
                ("Grotto", []),
                ("Blackroot", []),
                ("Swamp", []),
                ("Ginso", []),
                ("Valley", []),
                ("Misty", []),
                ("Forlorn", []),
                ("Sorrow", []),
                ("Horu", [])
            ])

            for area in self.areas.values():
                for location in area.locations:
                    locations_by_zone[location.zone].append(location)

            self.relic_spoiler = {}
            self.spoilerGroup = self.relic_spoiler

            for locations in locations_by_zone.values():
                random.shuffle(locations)

                relic_loc = None

                while not relic_loc and len(locations):
                    next_loc = locations.pop()
                    # Can't put a relic on a map turn-in
                    if next_loc.orig == "MapStone":
                        continue
                    # Can't put a relic on a reserved limitkeys location
                    if args.limitkeys and next_loc.orig in self.keySpots:
                        continue
                    relic_loc = next_loc

                self.relic_assign(relic_loc)
                self.itemCount -= 1

        self.outputStr += ("-280256|EC|1|Glades\n")  # first energy cell
        self.outputStr += ("-1680104|EX|100|Grove\n")  # glitchy 100 orb at spirit tree
        self.outputStr += ("-12320248|SK|51|Forlorn\n")  # forlorn escape plant
        # the 2nd keystone in misty can get blocked by alt+R, so make it unimportant
        self.outputStr += ("-10440008|EX|100|Misty\n")

        if args.noplants:
            for location in plants:
                self.outputStr += (str(location.get_key()) + "|NO|0\n")

        locationsToAssign = []
        self.connectionQueue = []
        self.reservedLocations = []

        global mapstonesSeen
        mapstonesSeen = 1

        self.skillCount = 10
        self.mapstonesAssigned = 0
        self.expSlots = self.itemPool["EX*"]

        self.spoilerGroup = {"MS": [], "KS": [], "EC": [], "HC": []}

        self.doorQueue = OrderedDict()
        self.mapQueue = OrderedDict()
        spoilerPath = ""

        self.reach_area("SunkenGladesRunaway")
        if args.open:
            self.reach_area("GladesMain")
            for connection in list(self.areas["SunkenGladesRunaway"].connections):
                if connection.target == "GladesMain":
                    self.areas["SunkenGladesRunaway"].remove_connection(connection)

        while self.itemCount > 0 or (args.balanced and self.balanceListLeftovers):

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

            locationsToAssign = self.get_all_accessible_locations()
            # if there aren't any doors to open, it's time to get a new skill
            # consider -- work on stronger anti-key-lock logic so that we don't
            # have to give keys out right away (this opens up the potential of
            # using keys in the wrong place, will need to be careful)
            if not self.doorQueue and not self.mapQueue:
                spoilerPath = self.prepare_path(len(locationsToAssign))
                if not self.assignQueue:
                    # we've painted ourselves into a corner, try again
                    if not self.reservedLocations:
                        if self.playerID == 1:
                            self.sharedMap = {}
                        if depth > args.players * args.players:
                            return
                        return self.placeItems(seed, args, modes, flags, depth+1)
                    locationsToAssign.append(self.reservedLocations.pop(0))
                    locationsToAssign.append(self.reservedLocations.pop(0))
                    spoilerPath = self.prepare_path(len(locationsToAssign))
                if args.balanced:
                    for item in self.assignQueue:
                        if len(self.balanceList) == 0:
                            break
                        locationsToAssign.append(self.get_location_from_balance_list())
            # pick what we're going to put in our accessible space
            itemsToAssign = []
            if len(locationsToAssign) < len(self.assignQueue) + max(keystoneCount - inventory["KS"], 0) + max(mapstoneCount - inventory["MS"], 0):
                # we've painted ourselves into a corner, try again
                if not self.reservedLocations:
                    if self.playerID == 1:
                        self.sharedMap = {}
                    if depth > args.players * args.players:
                        return
                    return self.placeItems(seed, args, modes, flags, depth+1)
                locationsToAssign.append(self.reservedLocations.pop(0))
                locationsToAssign.append(self.reservedLocations.pop(0))
            for i in range(0, len(locationsToAssign)):
                if self.assignQueue:
                    itemsToAssign.append(self.assign(self.assignQueue.pop(0)))
                elif inventory["KS"] < keystoneCount:
                    itemsToAssign.append(self.assign("KS"))
                elif inventory["MS"] < mapstoneCount:
                    itemsToAssign.append(self.assign("MS"))
                elif inventory["HC"] * args.force_cells < (252 - self.itemCount) and self.itemPool["HC"] > 0:
                    itemsToAssign.append(self.assign("HC"))
                elif inventory["EC"] * args.force_cells < (252 - self.itemCount) and self.itemPool["EC"] > 0:
                    itemsToAssign.append(self.assign("EC"))
                elif args.balanced and self.itemCount == 0:
                    itemsToAssign.append(self.balanceListLeftovers.pop(0))
                    self.itemCount += 1
                else:
                    itemsToAssign.append(self.assign_random())
                self.itemCount -= 1

            # force assign things if using --prefer-path-difficulty
            if args.prefer_path_difficulty:
                for item in list(itemsToAssign):
                    if item in self.skillsOutput or item in self.eventsOutput:
                        self.preferred_difficulty_assign(item, locationsToAssign)
                        itemsToAssign.remove(item)

            # shuffle the items around and put them somewhere
            random.shuffle(itemsToAssign)
            for i in range(0, len(locationsToAssign)):
                self.assign_to_location(itemsToAssign[i], locationsToAssign[i])

            currentGroupSpoiler = ""

            if spoilerPath:
                currentGroupSpoiler += ("    Forced pickups: " + str(spoilerPath) + "\n")

            for skill in self.skillsOutput:
                if skill in self.spoilerGroup:
                    for instance in self.spoilerGroup[skill]:
                        currentGroupSpoiler += "    " + instance
                    if skill in seedDifficultyMap:
                        self.seedDifficulty += groupDepth * seedDifficultyMap[skill]

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
            self.spoilerGroup = {"MS": [], "KS": [], "EC": [], "HC": []}

            self.doorQueue = OrderedDict()
            self.mapQueue = OrderedDict()
            spoilerPath = ""

        if args.world_tour:
            spoilerStr += "Relics: {\n"

            for instance in self.relic_spoiler["Relic"]:
                spoilerStr += "    " + instance

            spoilerStr += "}\n"

        if args.balanced:
            for item in self.balanceList:
                self.outputStr += item[2]

        spoilerStr = flags + "|" + str(seed) + "\n" + "Difficulty Rating: " + str(self.seedDifficulty) + "\n" + spoilerStr
        random.shuffle(self.eventList)
        for event in self.eventList:
            self.outputStr += event

        if args.loc_analysis:
            locationAnalysis = locationAnalysisCopy


        return (self.outputStr, spoilerStr)

def main():

    global random
    global args

    random = Random()

    parser = argparse.ArgumentParser()
    parser.add_argument("--preset", help="Choose a preset group of paths for the generator to use", choices=["casual", "standard", "expert", "master", "hard", "ohko", "0xp", "glitched"], default="standard")
    parser.add_argument("--custom-logic", help="Customize paths that the generator will use, comma-separated: normal,speed,dbash,extended,extended-damage,lure,speed-lure,lure-hard,dboost,dboost-light,dboost-hard,gjump,cdash,cdash-farming,extreme,timed-level,glitched")
    parser.add_argument("--seed", help="Seed number (default 1)", type=int, default=1)
    parser.add_argument("--count", help="Number of seeds to generate (default 1)", type=int, default=1)
    parser.add_argument("--hard", help="Enable hard mode", action="store_true")
    parser.add_argument("--ohko", help="Enable one-hit-ko mode", action="store_true")
    parser.add_argument("--zeroxp", help="Enable 0xp mode", action="store_true")
    parser.add_argument("--nobonus", help="Remove bonus powerups from the item pool", action="store_true")
    parser.add_argument("--noplants", help="Ignore petrified plants when assigning items", action="store_true")
    parser.add_argument("--starved", help="Reduces the rate at which skills will appear when not required to advance", action="store_true")
    parser.add_argument("--shards", help="The Water Vein, Gumon Seal, and Sunstone will be awarded after 3/5 shards are found", action="store_true")
    parser.add_argument("--limitkeys", help="The Water Vein, Gumon Seal, and Sunstone will only appear at skill trees or event sources", action="store_true")
    parser.add_argument("--clues", help="For each 3 trees visited, the location of a random dungeon key will be revealed", action="store_true")
    parser.add_argument("--non-progressive-mapstones", help="Map Stones will retain their behaviour from before v1.2, having their own unique drops", action="store_true")
    parser.add_argument("--force-trees", help="Prevent Ori from entering the final escape room until all skill trees have been visited", action="store_true");
    parser.add_argument("--exp-pool", help="Size of the experience pool (default 10000)", type=int, default=10000)
    parser.add_argument("--prefer-path-difficulty", help="Increase the chances of putting items in more convenient (easy) or less convenient (hard) locations", choices=["easy", "hard"])
    parser.add_argument("--no-teleporters", help="Remove teleporter activation pickups from the pool", action="store_true")
    parser.add_argument("--analysis", help="Report stats on the skill order for all seeds generated", action="store_true")
    parser.add_argument("--loc-analysis", help="Report stats on where skills are placed over multiple seeds", action="store_true")
    parser.add_argument("--players", help="Player count for paired randomizer", type=int, default=1)
    parser.add_argument("--sync-id", help="Team identifier number for paired randomizer", type=int)
    parser.add_argument("--shared-items", help="What will be shared by sync, comma-separated: skills,keys,events,teleporters,upgrades", default="skills,keys")
    parser.add_argument("--share-mode", help="How the server will handle shared pickups, one of: shared,swap,split,none", default="shared")
    parser.add_argument("--balanced", help="Reduce the value of newly discovered locations for progression placements", action="store_true")
    parser.add_argument("--entrance", help="Randomize entrances", action="store_true")
    parser.add_argument("--open", help="Activate open mode", action="store_true")
    parser.add_argument("--force-cells", help="Force health and energy cells to appear every N pickups, if they don't randomly", type=int, default=256)
    parser.add_argument("--easy", help="Add an extra copy of double jump, bash, stomp, glide, charge jump, dash, grenade, water, and wind", action="store_true")
    parser.add_argument("--free-mapstones", help="Don't require a mapstone to be placed when a map monument becomes accessible", action="store_true")
    parser.add_argument("--world-tour", help="Prevent Ori from entering the final escape until collecting one relic from each of the zones in the world", action="store_true")

    args = parser.parse_args()

    presets = {
        "casual": ["casual-core", "casual-dboost"],
        "standard": ["casual-core", "casual-dboost", "standard-core", "standard-lure", "standard-dboost", "standard-abilities"],
        "expert": ["casual-core", "casual-dboost", "standard-core", "standard-lure", "standard-dboost", "standard-abilities", "expert-core", "expert-lure", "expert-dboost", "expert-abilities", "dbash"],
        "master": ["casual-core", "casual-dboost", "standard-core", "standard-lure", "standard-dboost", "standard-abilities", "expert-core", "expert-lure", "expert-dboost", "expert-abilities", "master-core", "master-lure", "master-dboost", "master-abilities", "dbash", "gjump"],
        "ohko": ["casual-core", "standard-core", "standard-lure", "standard-abilities", "expert-core", "expert-abilities", "dbash"],
        "0xp": ["casual-core", "casual-dboost", "standard-core", "standard-lure"],
        "glitched": ["casual-core", "casual-dboost", "standard-core", "standard-lure", "standard-dboost", "standard-abilities", "expert-core", "expert-lure", "expert-dboost", "expert-abilities", "master-core", "master-lure", "master-dboost", "master-abilities", "dbash", "gjump", "glitched", "timed-level"]
    }

    # default to standard logic
    if args.custom_logic:
        mode = "custom"
        modes = args.custom_logic.split(',')
    else:
        mode = args.preset
        modes = presets[args.preset]

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
    if args.open:
        flags += ",open"
    if args.force_cells != 256:
        flags += ",cells" + str(args.force_cells)
    if args.easy:
        flags += ",easy"
    if not args.free_mapstones:
        flags += ",LockedMapStones"
    if args.world_tour:
        flags += ",WorldTour"
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
        "TPValley": 0,
        "TPGinso": 0,
        "TPHoru": 0
    }

    locationAnalysis = {}
    for i in range(1,10):
        locationAnalysis["MapStone " + str(i)] = itemsToAnalyze.copy()
        locationAnalysis["MapStone " + str(i)]["Zone"] = "MapStone"

    for seedOffset in range(0, args.count):
        
        seed = args.seed + seedOffset
        random.seed(seed)

        generator = Generator()

        placements = generator.placeItemsMulti(seed, args, modes, flags, syncFlags)

        if args.analysis or args.loc_analysis:
            print(seed)

        if not args.analysis and not args.loc_analysis:
            for player in range(0,args.players):
                output = open("randomizer_" + mode + str(seed) + "_" + str(player+1) + ".dat", 'w')
                output.write(placements[player][0])
                output.close()

                spoiler = open("spoiler_" + mode + str(seed) + "_" + str(player+1) + ".txt", 'w')
                spoiler.write(placements[player][1])
                spoiler.close()

    if args.analysis:
        print(skillAnalysis["WallJump"])
        print(skillAnalysis["ChargeFlame"])
        print(skillAnalysis["DoubleJump"])
        print(skillAnalysis["Bash"])
        print(skillAnalysis["Stomp"])
        print(skillAnalysis["Glide"])
        print(skillAnalysis["Climb"])
        print(skillAnalysis["ChargeJump"])
        print(skillAnalysis["Dash"])
        print(skillAnalysis["Grenade"])

    if args.loc_analysis:
        output = open("analysis.csv", 'w')
        output.write("Location,Zone,WallJump,ChargeFlame,DoubleJump,Bash,Stomp,Glide,Climb,ChargeJump,Dash,Grenade,GinsoKey,ForlornKey,HoruKey,Water,Wind,WaterVeinShard,GumonSealShard,SunstoneShard,TPGrove,TPGrotto,TPSwamp,TPValley,TPSorrow,TPGinso,TPForlorn,TPHoru\n")
        for key in locationAnalysis.keys():
            line = key + ","
            line += str(locationAnalysis[key]["Zone"]) + ","
            line += str(locationAnalysis[key]["WallJump"]) + ","
            line += str(locationAnalysis[key]["ChargeFlame"]) + ","
            line += str(locationAnalysis[key]["DoubleJump"]) + ","
            line += str(locationAnalysis[key]["Bash"]) + ","
            line += str(locationAnalysis[key]["Stomp"]) + ","
            line += str(locationAnalysis[key]["Glide"]) + ","
            line += str(locationAnalysis[key]["Climb"]) + ","
            line += str(locationAnalysis[key]["ChargeJump"]) + ","
            line += str(locationAnalysis[key]["Dash"]) + ","
            line += str(locationAnalysis[key]["Grenade"]) + ","
            line += str(locationAnalysis[key]["GinsoKey"]) + ","
            line += str(locationAnalysis[key]["ForlornKey"]) + ","
            line += str(locationAnalysis[key]["HoruKey"]) + ","
            line += str(locationAnalysis[key]["Water"]) + ","
            line += str(locationAnalysis[key]["Wind"]) + ","
            line += str(locationAnalysis[key]["WaterVeinShard"]) + ","
            line += str(locationAnalysis[key]["GumonSealShard"]) + ","
            line += str(locationAnalysis[key]["SunstoneShard"]) + ","
            line += str(locationAnalysis[key]["TPGrove"]) + ","
            line += str(locationAnalysis[key]["TPGrotto"]) + ","
            line += str(locationAnalysis[key]["TPSwamp"]) + ","
            line += str(locationAnalysis[key]["TPValley"]) + ","
            line += str(locationAnalysis[key]["TPSorrow"]) + ","
            line += str(locationAnalysis[key]["TPGinso"]) + ","
            line += str(locationAnalysis[key]["TPForlorn"]) + ","
            line += str(locationAnalysis[key]["TPHoru"])

            output.write(line + "\n")

if __name__ == "__main__":
    main()
