import sys
from collections import OrderedDict

def _parsefatal(line, msg):
    if _VERBOSE:
        print("[FATAL] line %d - %s" % (line, msg))

def _parseerror(line, msg):
    if _VERBOSE:
        print("[ERROR] line %d - %s" % (line, msg))

def _parsewarn(line, msg):
    if _VERBOSE:
        print("[WARN] line %d - %s" % (line, msg))

_VERBOSE = False

_PATHSETS = [
    "casual-core",
    "casual-dboost",
    "standard-core",
    "standard-dboost",
    "standard-lure",
    "standard-abilities",
    "expert-core",
    "expert-dboost",
    "expert-lure",
    "expert-abilities",
    "dbash",
    "master-core",
    "master-dboost",
    "master-lure",
    "master-abilities",
    "gjump",
    "glitched",
    "timed-level",
    "insane"
]

_REQS = [
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
    "TPGinso",
    "TPValley",
    "TPForlorn",
    "TPSorrow",
    "TPHoru",
    "Health",
    "Energy",
    "Ability",
    "Keystone",
    "Mapstone",
    "Free",
    "Open",
    "OpenWorld"
]

def ori_load_file(fn, verbose=False):
    with open(fn, 'r') as f:
        lines = f.readlines()

    return ori_load(lines, verbose)

def ori_load_url(url, verbose=False):
    try:
        # use urlfetch if we have it to avoid webgen warning spam
        from google.appengine.api import urlfetch
        result = urlfetch.fetch(url)
        lines = result.content.split("\n")
    except ImportError:
        # cli_gen uses urllib2 instead
        import urllib2
        response = urllib2.urlopen(url)
        lines = response.read().split("\n")

    return ori_load(lines, verbose)

def ori_load(lines, verbose=False):
    global _VERBOSE
    _VERBOSE = verbose

    contents = OrderedDict([
        ('locs', OrderedDict()),
        ('homes', OrderedDict())
    ])
    context_home = None
    context_conn = None
    i = 0

    while i < len(lines):
        # Tokenize the line by whitespace
        tokens = lines[i].split()
        i += 1

        # Skip empty lines and full comment lines
        if len(tokens) == 0:
            continue
        if tokens[0][:2] == "--":
            continue

        # Drop any tokens after a comment marker
        for j in range(len(tokens)):
            if tokens[j][:2] == "--":
                tokens = tokens[:j]
                break

        # Find a type marker and perform contextual parsing
        if tokens[0][-1:] == ":":
            type_marker = tokens[0][:-1]

            if type_marker == "loc":
                context_home = None
                context_conn = None

                if len(tokens) < 7:
                    _parseerror(i, "ignoring loc definition with too few fields (name, X, Y, original item, difficulty, zone)")
                    continue

                name = tokens[1]
                if name in contents["homes"]:
                    _parsefatal(i, "cannot use the same name `%s` for both a pickup location and a home!" % name)
                    return None

                if name in contents["locs"]:
                    _parseerror(i, "ignoring duplicate loc definition for `%s`" % name)
                    continue

                if len(tokens) > 7:
                    _parsewarn(i, "ignoring extra fields in loc definition for `%s`" % name)

                x = tokens[2]
                y = tokens[3]
                item = tokens[4]
                difficulty = tokens[5]
                zone = tokens[6]

                contents["locs"][name] = OrderedDict([
                    ('x', x),
                    ('y', y),
                    ('item', item),
                    ('difficulty', difficulty),
                    ('zone', zone)
                ])
            elif type_marker == "home":
                context_home = None
                context_conn = None

                if len(tokens) < 2:
                    _parseerror(i, "ignoring home definition with too few fields (name)")
                    continue

                name = tokens[1]
                if name in contents["locs"]:
                    _parsefatal(i, "cannot use the same name `%s` for both a pickup location and a home!" % name)
                    return None

                if name in contents["homes"]:
                    _parseerror(i, "ignoring duplicate home definition for `%s`" % name)
                    continue
                else:
                    contents["homes"][name] = OrderedDict([
                        ('conns', OrderedDict())
                    ])

                if len(tokens) > 2:
                    _parsewarn(i, "ignoring extra fields in home definition for `%s`" % name)

                context_home = name
            elif type_marker == "pickup" or type_marker == "conn":
                if not context_home:
                    _parseerror(i, "ignoring %s connection with no active home" % type_marker)
                    continue

                context_conn = None

                if len(tokens) < 2:
                    _parseerror(i, "ignoring %s connection in home `%s` with too few fields (name)" % (type_marker, context_home))
                    continue

                name = tokens[1]
                if name in contents["homes"][context_home]["conns"]:
                    _parsewarn(i, "combining duplicate %s connection for `%s` in same home `%s`" % (type_marker, name, context_home))
                else:
                    contents["homes"][context_home]["conns"][name] = OrderedDict([
                        ("type", type_marker),
                        ("paths", [])
                    ])

                if len(tokens) > 2:
                    _parsewarn(i, "ignoring extra fields in %s connection for `%s` in home `%s`" % (type_marker, name, context_home))

                context_conn = name
        else:
            # If there's no type marker, it's a logic path
            valid = True

            if tokens[0] not in _PATHSETS:
                _parseerror(i, "ignoring logic path with unknown pathset %s" % tokens[0])
                valid = False

            if valid and not context_home:
                _parseerror(i, "ignoring logic path with no active home")
                valid = False

            if valid and not context_conn:
                _parseerror(i, "ignoring logic path with no active connection")
                valid = False

            has_health = False
            has_ability = False
            has_mapstone = False

            if valid:
                for j in range(1, len(tokens)):
                    req = tokens[j]
                    if "=" in req:
                        (req, count) = req.split("=")

                        if int(count) == 0:
                            _parseerror(i, "ignoring logic path with invalid count requirement %s" % tokens[j])
                            valid = False
                            break
                    if req not in _REQS:
                        _parseerror(i, "ignoring logic path with unknown requirement %s" % tokens[j])
                        valid = False
                        break

                    if req == "Health":
                        has_health = True
                    if req == "Ability":
                        has_ability = True
                    if req == "Mapstone":
                        has_mapstone = True

            if not valid:
                contents["homes"][context_home]["conns"][name]["paths"].append(tuple(["invalid"] + tokens))
                continue

            if tokens[0] != "casual-dboost" and "dboost" in tokens[0] and not has_health:
                _parsewarn(i, "`%s` logic path (from: `%s` to: `%s`) may be missing a health value" % (tokens[0], context_home, context_conn))
            elif "dboost" not in tokens[0] and has_health:
                _parsewarn(i, "`%s` logic path (from: `%s` to: `%s`) has a health requirement" % (tokens[0], context_home, context_conn))

            if "abilities" in tokens[0] and not has_ability:
                _parsewarn(i, "`%s` logic path (from: `%s` to: `%s`) may be missing an ability point count" % (tokens[0], context_home, context_conn))
            elif "abilities" not in tokens[0] and (tokens[0] == "casual-dboost" or "dboost" not in tokens[0]) and has_ability:
                _parsewarn(i, "`%s` logic path (from: `%s` to: `%s`) has an ability point count" % (tokens[0], context_home, context_conn))

            if context_conn[-3:] == "Map" and not has_mapstone:
                _parsewarn(i, "`%s` logic path (from: `%s` to: `%s`) is missing a mapstone requirement" % (tokens[0], context_home, context_conn))
            elif context_conn[-3:] != "Map" and has_mapstone:
                _parsewarn(i, "`%s` logic path (from: `%s` to: `%s`) incorrectly has a mapstone requirement" % (tokens[0], context_home, context_conn))

            contents["homes"][context_home]["conns"][name]["paths"].append(tuple(tokens))

    connected = {
        "SunkenGladesRunaway": True
    }

    for area in contents["homes"].keys():
        for target in contents["homes"][area]["conns"].keys():
            connected[target] = True
            conn_type = contents["homes"][area]["conns"][target]["type"]
            if conn_type == "pickup" and target in contents["homes"]:
                _parsewarn(0, "home `%s` is connected from home `%s` with type `pickup` (should be `conn`)" % (target, area))
            elif conn_type == "conn" and target in contents["locs"]:
                _parsewarn(0, "pickup location `%s` is connected from `%s` with type `conn` (should be `pickup`)" % (target, area))

    for loc in contents["locs"].keys():
        if loc not in connected:
            _parsewarn(0, "pickup location `%s` is not connected from any home" % loc)

    for home in contents["homes"].keys():
        if home not in connected:
            _parsewarn(0, "home `%s` is not connected from any home!" % home)

    return contents

if __name__ == "__main__":
    import sys
    fn = sys.argv[1]
    ori_load_file(fn, True)
