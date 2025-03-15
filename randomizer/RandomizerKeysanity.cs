using Game;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomizerKeysanity {

    public bool IsActive;
    private Dictionary<MoonGuid, int> _doorKeyMap;
    private Dictionary<int, string> _hintMap;
    private RandomizerInventory _inventory;

    public RandomizerKeysanity(RandomizerInventory inventory) {
        _doorKeyMap = new Dictionary<MoonGuid, int>() {
            { new MoonGuid(-1232154268, 1164352171, -836255810, -1590216903), 300 },
            { new MoonGuid(594661726, 1329767267, -1704907880, -27301018), 301 },
            { new MoonGuid(-1932550571, 1250172391, -1917455943, -1939528727), 302 },
            { new MoonGuid(-11481148, 1140648064, 412034978, -613375286), 303 },
            { new MoonGuid(1795834119, 1281161926, -2078449998, 1300906360), 304 },            
            { new MoonGuid(378303650, 1322384872, 242472089, -1590736948), 305 },
            { new MoonGuid(1873996457, 1258667073, -902258499, -2125303602), 306 },            
            { new MoonGuid(1509405999, 1211109047, -376011127, -1110353735), 307 },
            { new MoonGuid(-234272614, 1227199594, 1013218476, 1564745183), 308 },
            { new MoonGuid(1974125039, 1165165912, 119656890, 137757679), 309 },
            { new MoonGuid(1472759010, 1273700124, 616551597, 280271352), 310 },
            { new MoonGuid(-1909990366, 1163800373, 1858164881, 1500718794), 311 },
        };

        _hintMap = new Dictionary<int, string>() {
            { 300, "Glades Pool Keystone"},
            { 301, "Lower Spirit Caverns Keystone"},
            { 302, "Grotto Keystone"},
            { 303, "Swamp Keystone"},
            { 304, "Upper Spirit Caverns Keystone"},
            { 305, "Lower Ginso Keystone"},
            { 306, "Upper Ginso Keystone"},            
            { 307, "Misty Keystone"},
            { 308, "Forlorn Keystone"},
            { 309, "Lower Sorrow Keystone"},
            { 310, "Mid Sorrow Keystone"},
            { 311, "Upper Sorrow Keystone"},
        };


        _inventory = inventory;
    }

    public void ApplyKeystoneCount(MoonGuid guid, int numberUsed) { 
        if (!IsActive) {
            return;
        }

        if (_doorKeyMap.TryGetValue(guid, out var value)) {
            Characters.Sein.Inventory.Keystones = _inventory.GetRandomizerItem(value) - numberUsed;
            return;
        }
        Characters.Sein.Inventory.Keystones = 0;
    }

    public void ResetKeystoneCount() {
        if (!IsActive) {
            return;
        }

        Characters.Sein.Inventory.Keystones = 0;
    }

    public void ShowPickupHint(int id) {
        if (_hintMap.TryGetValue(id, out var baseHint)) {
            RandomizerSwitch.PickupMessage($"{baseHint} ({_inventory.GetRandomizerItem(id)} / {(id < 304 ? 2 : 4)})");
        }

        return;
    }

}