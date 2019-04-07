from math import floor
from collections import defaultdict, namedtuple
from seedbuilder.oriparse import ori_load_url
import logging as log

coord_correction_map = {
    679620: 719620,
    -4560020: -4600020,
    -520160: -560160,
    8599908: 8599904,
    2959744: 2919744,
}

PickLoc = namedtuple("PickLoc", ["coords", "name", "zone", "area", "x", "y"])

extra_PBT = [
    PickLoc(24, 'Mapstone 1', 'Mapstone', 'MS1', 0, 24),
    PickLoc(28, 'Mapstone 2', 'Mapstone', 'MS2', 0, 28),
    PickLoc(32, 'Mapstone 3', 'Mapstone', 'MS3', 0, 32),
    PickLoc(36, 'Mapstone 4', 'Mapstone', 'MS4', 0, 36),
    PickLoc(40, 'Mapstone 5', 'Mapstone', 'MS5', 0, 40),
    PickLoc(44, 'Mapstone 6', 'Mapstone', 'MS6', 0, 44),
    PickLoc(48, 'Mapstone 7', 'Mapstone', 'MS7', 0, 48),
    PickLoc(52, 'Mapstone 8', 'Mapstone', 'MS8', 0, 52),
    PickLoc(56, 'Mapstone 9', 'Mapstone', 'MS9', 0, 56),
    PickLoc(-280256, "EC", "Glades", "SunkenGladesFirstEC", -28, -256),
    PickLoc(-1680104, "EX100", "Grove", "UnsafeSpiritTree100Ex", -168, -104),
    PickLoc(-2399488, "EVWarmth", "Horu", "FinalEscape", -240, 512),
    PickLoc(-12320248, "Plant", "Forlorn", "ForlornEscapePlant", -1232, -248),
    PickLoc(2, "SPAWN", "Glades", "FirstPickup", 189, -210),
]

def enums_from_strlist(enum, strlist):
    enums = []
    for elem in strlist:
        maybe_enum = enum.mk(elem)
        if maybe_enum:
            enums.append(maybe_enum)
    return enums


def int_to_bits(n, min_len=2):
    raw = [1 if digit == '1' else 0 for digit in bin(n)[2:]]
    if len(raw) < min_len:
        raw = [0] * (min_len - len(raw)) + raw
    return raw

def bits_to_int(n):
    return int("".join([str(b) for b in n]), 2)

def rm_none(itr):
    return [elem for elem in itr if elem is not None]


log_2 = {1: 0, 2: 1, 4: 2, 8: 3, 16: 4, 32: 5, 64: 6, 128: 7, 256: 8, 512: 9, 1024: 10, 2048: 11, 4096: 12, 8192: 13, 16384: 14, 32768: 15, 65536: 16}

all_locs = set([2, 2999808, 5280264, -4159572, 4479832, 4559492, 919772, -3360288, 24, -8400124, 28, 32, 1599920, -6479528, 36, 40, 3359580, 2759624, 44, 4959628, 4919600, 3279920, -12320248, 1479880,
                52, 56, 3160244, 960128, 799804, -6159632, -800192, 5119584, 5719620, -6279608, -3160308, 5320824, 4479568, 9119928, -319852, 1719892, -480168, 919908, 1519708, -6079672, 2999904,
                -6799732, -11040068, 5360732, 559720, 4039612, 4439632, 1480360, -2919980, -120208, -2480280, 4319860, -7040392, -1800088, -4680068, 4599508, 2919744, 3319936, 1720000, 120164,
                -4600188, 5320328, 6999916, 3399820, 1920384, -400240, -6959592, 4319892, 2239640, 2719900, -160096, 3559792, 1759964, -5160280, 6359836, 5080496, 5359824, 1959768, 5039560, 4560564,
                -10440008, 2519668, -2240084, -10760004, -4879680, 799776, -5640092, -6080316, 6279880, 4239780, -5119796, 7599824, 5919864, -4160080, 4999892, 3359784, 4479704, -1800156, -6280316,
                -5719844, -8600356, -2160176, 5399780, -6119704, 5639752, 3439744, 7959788, 5080304, 5320488, -10120036, -7960144, -1680140, -8920328, 1839836, 2520192, 1799708, 5399808, -8720256,
                639888, 719620, 6639952, 3919624, -4600020, 5200140, 39756, 2480400, 959960, 6839792, -1680104, -8880252, 5320660, 3279644, -6719712, 48, 599844, -3600088, 8839900, 4199724, 3039472,
                -4559584, -1560272, 1600136, 4759860, 5280500, 2559800, 3119768, 6159900, 5879616, -10759968, 5280296, 3919688, -2080116, 5119900, 3199820, 2079568, -5400236, -4199936, -8240012,
                -5479592, -3200164, 8599904, -5039728, 7839588, -5159576, 4079964, -1840196, 7679852, 5400100, -7680144, -6720040, -5919556, 1880164, -3559936, -6319752, 5280404, 39804, 6399872,
                -280256, -9799980, 1280164, -1560188, -2200184, 6080608, -1919808, 4639628, 7639816, -6800032, 5160336, 3879576, 4199828, 3959588, 5119556, 5400276, -1840228, 5160864, 1040112,
                4680612, -11880100, -4440152, -3520100, 7199904, -2200148, 7559600, -10839992, 5040476, -8160268, 4319676, 5160384, 5239456, -2400212, 2599880, 3519820, -9120036, 3639880, -6119656,
                3039696, 1240020, -5159700, -4359680, -5400104, -5959772, 5439640, -8440352, 3639888, -2480208, 399844, -560160, 4359656, -4799416, 8719856, -6039640, -5479948, 5519856, 6199596,
                -4600256, -2840236, 5799932, -600244, 5360432, -1639664, -199724, -919624, -959848,  1720288,  2160192,  2640380,  3040304, -2399488, -5599400, -7200024, -7320236,  4999752, 5480952])


def get_bit(bits_int, bit):
    return int_to_bits(bits_int, log_2[bit] + 1)[-(1 + log_2[bit])]

def get_taste(bits_int, bit):
    bits = int_to_bits(bits_int, log_2[bit] + 2)[-(2 + log_2[bit]):][:2]
    return 2 * bits[0] + bits[1]

def add_single(bits_int, bit, remove=False):
    if bit < 0:
        return bits_int
    if bits_int >= bit:
        if remove:
            return bits_int - bit
        if get_bit(bits_int, bit) == 1:
            return bits_int
    return bits_int + bit

def inc_stackable(bits_int, bit, remove=False):
    if bit < 0:
        return bits_int
    if remove:
        if get_taste(bits_int, bit) > 0:
            return bits_int - bit
        return bits_int
    if get_taste(bits_int, bit) > 2:
        return bits_int
    return bits_int + bit


def get(x, y):
    return x * 10000 + y

def sign(x):
    return 1 if x >= 0 else -1

def rnd(x):
    return int(floor((x) / 4.0) * 4.0)

def unpack(coord):
    y = coord % (sign(coord) * 10000)
    if y > 2000:
        y -= 10000
    elif y < -2000:
        y += 10000
    if y < 0:
        coord -= y
    x = rnd(coord / 10000)
    return x, y

def is_int(s):
    try:
        int(s)
        return True
    except ValueError:
        return False


def picks_by_type(extras=False):
    locs = ori_load_url('http://raw.githubusercontent.com/sigmasin/OriDERandomizer/3.0/seed_gen/areas.ori')["locs"]

    picks_by_type = defaultdict(lambda: [])
    all_locs_unpacked = {unpack(loc): loc for loc in all_locs}
    for area, loc_info in locs.iteritems():
        x = loc_info["x"]
        y = loc_info["y"]
        item = loc_info["item"]
        zone = loc_info["zone"]
        crd = get(rnd(int(x)), rnd(int(y)))
        if crd not in all_locs and item != "MapStone":
            secondary_match = all_locs_unpacked.get((rnd(int(x)), rnd(int(y))))
            if secondary_match:
                crd = secondary_match
            else:
                print "No secondary match found here!", crd, item, zone, area, x, y
        line = PickLoc(crd, item, zone, area, x, y)
        picks_by_type[item[0:2]].append(line)
    if extras:
        for extra in extra_PBT:
            p_type = extra.name[0:2]
            if p_type == "Ma":
                picks_by_type["MP"].append(extra)
            elif p_type in picks_by_type.keys():
                picks_by_type[p_type].append(extra)
    return picks_by_type

def picks_by_coord(extras=False):
    pbt = picks_by_type(extras)
    pbc = {}
    for pickgroup in pbt.values():
        for pick in pickgroup:
            pbc[pick.coords] = pick
    return pbc

def picks_by_type_generator():
    lines = "{\n"
    pbt = picks_by_type(extras=True)
    for key in sorted(pbt.keys()):
        lines += '"%s": [\n' % key
        for item in sorted(pbt[key], key=lambda x: str(x.coords)):
            lines += """\t{"loc": %s, "name": "%s", "zone": "%s", "area": "%s", "x": %s, "y": %s}, \n""" % item
        lines = lines[:-3] + '\n], '
    lines = lines[:-2] + "\n}"
    return lines