import lxml.etree as XML

meta = []

with open("areas.ori", 'r') as f:
	meta = f.readlines()

new_areas = XML.Element("Areas")

area_name = None
area = None
conn = None
has_reqs = False
all_req = None

loc_homes = {}
homes = {}

allowed_things = [
	"WallJump",
	"ChargeFlame",
	"DoubleJump",
	"Bash",
	"Stomp",
	"Glide",
	"Climb",
	"ChargeJump",
	"Grenade",
	"Dash",
	"GinsoKey",
	"ForlornKey",
	"HoruKey",
	"Water",
	"Wind",
	"TPGrove",
	"TPSwamp",
	"TPGrotto",
	"TPValley",
	"TPForlorn",
	"TPSorrow",
	"TPGinso",
	"TPHoru",
	"Health",
	"Energy",
	"Ability",
	"Keystone",
	"Mapstone",
	"Free",
    "Open"
]

allowed_paths = [
	"casual-core",
	"casual-dboost",
	"standard-core",
	"standard-lure",
	"standard-dboost",
	"standard-abilities",
	"expert-core",
	"expert-lure",
	"expert-dboost",
	"expert-abilities",
	"master-core",
	"master-lure",
	"master-dboost",
	"master-abilities",
	"dbash",
	"gjump",
	"glitched",
	"timed-level",
	"insane",
	"ALL"
]

def OpenConnection(target):
	global area, area_name, conn
	conn = XML.SubElement(area, "Connection")
	XML.SubElement(conn, "Home", name=area_name)
	XML.SubElement(conn, "Target", name=target)
	conn = XML.SubElement(conn, "Requirements")

def BuildRequirements(reqs):
	req_text = ""

	for thing in reqs:
		if req_text:
			req_text += '+'

		pickup = thing
		count = 1

		if '=' in thing:
			pickup, count = thing.split('=')

		assert pickup in allowed_things, pickup

		count = int(count)
		for i in range(count):
			if i > 0:
				req_text += '+'
			if pickup == 'Health':
				pickup = 'HC'
			elif pickup == 'Energy':
				pickup = 'EC'
			elif pickup == 'Keystone':
				pickup = 'KS'
			elif pickup == 'Ability':
				pickup = 'AC'
			elif pickup == 'Mapstone':
				pickup = 'MS'
			req_text += pickup

	return req_text

def AddRequirement(tokens):
	global conn, has_reqs, all_req
	path_type = tokens[0]

	assert path_type in allowed_paths, path_type

	if path_type == "ALL":
		all_req = BuildRequirements(tokens[1:])
		return

	req_node = XML.SubElement(conn, "Requirement", mode=path_type)
	has_reqs = True

	req_text = BuildRequirements(tokens[1:])

	if all_req:
		req_text = all_req + '+' + req_text

	req_node.text = req_text

def CloseConnection():
	global area_name, conn, has_reqs, all_req

	if conn is None:
		return

	if not has_reqs:
		assert not all_req, area_name
		req_node = XML.SubElement(conn, "Requirement", mode="casual-core")
		req_node.text = "Free"

	conn = None
	has_reqs = False
	all_req = None

# collect home names for validation purposes later
for line in meta:
	tokens = line.split()

	if len(tokens) == 0:
		continue
	if tokens[0][:2] == '--':
		continue

	if tokens[0] == "home:":
		homes[tokens[1]] = "NOT LINKED"

# now actually parse the file
for line in meta:
	tokens = line.split()

	if len(tokens) == 0:
		continue
	if tokens[0][:2] == '--':
		continue

	if tokens[0] == "loc:":
		assert not area
		assert not conn

		loc_name = tokens[1]
		loc_x = tokens[2]
		loc_y = tokens[3]
		loc_item = tokens[4]
		loc_diff = tokens[5]
		loc_zone = tokens[6]

		new_area = XML.SubElement(new_areas, "Area", name=loc_name)
		new_locs = XML.SubElement(new_area, "Locations")
		new_loc = XML.SubElement(new_locs, "Location")

		XML.SubElement(new_loc, "X").text = loc_x
		XML.SubElement(new_loc, "Y").text = loc_y
		XML.SubElement(new_loc, "Item").text = loc_item
		XML.SubElement(new_loc, "Difficulty").text = loc_diff
		XML.SubElement(new_loc, "Zone").text = loc_zone

		loc_homes[loc_name] = "NOT LINKED"

		XML.SubElement(new_area, "Connections")
	elif tokens[0] == "home:":
		assert len(tokens) == 2
		area_name = tokens[1]

		area_el = XML.SubElement(new_areas, "Area", name=area_name)
		XML.SubElement(area_el, "Locations")
		area = XML.SubElement(area_el, "Connections")
		CloseConnection()
	elif tokens[0] == "pickup:":
		assert area_name
		assert area is not None
		loc = tokens[1]

		assert loc_homes[loc] == "NOT LINKED", loc
		loc_homes[loc] = area_name

		CloseConnection()
		OpenConnection(loc)
	elif tokens[0] == "conn:":
		assert area_name
		assert area is not None
		target = tokens[1]

		assert target in homes, target
		assert target not in loc_homes, target
		homes[target] = "LINKED"

		CloseConnection()
		OpenConnection(target)
	else:
		assert area_name
		assert area is not None
		assert conn is not None
		AddRequirement(tokens)

for loc, area in loc_homes.items():
	assert area != "NOT LINKED", loc

for home, linked in homes.items():
	if home != "SunkenGladesRunaway":
		assert linked == "LINKED", home

new_tree = XML.ElementTree(new_areas)
new_tree.write("areas.xml", pretty_print=True)