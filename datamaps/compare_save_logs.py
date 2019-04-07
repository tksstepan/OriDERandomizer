
lines1 = open('randomizer.log').readlines()
lines2 = open('randomizer2.log').readlines()

map = {}

scene = ""

i = 0
while i < len(lines2):
    if lines2[i].strip() == "SCENE":
        scene = lines2[i+1]
    else:
        map[lines2[i]] = lines2[i+1]
    i += 2

i = 0
while i < len(lines1):
    if lines1[i].strip() == "SCENE":
        scene = lines1[i+1]
    else:
        if (lines1[i] not in map or map[lines1[i]] != lines1[i+1]) and lines1[i].strip != "":
            print lines1[i].strip()
            print lines1[i+1].strip()
    i += 2
    