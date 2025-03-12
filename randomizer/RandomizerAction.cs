using System;
using System.Linq;
using System.Collections.Generic;

public class RandomizerAction
{
    public static List<string> StringValPickupTypes = new List<string> {"TP", "SH", "NO", "WT", "MU", "HN", "WP", "RP", "WS", "TW", "NB"};

    public RandomizerAction(string action, object value) {
        this.Action = action;
        this.Value = StringValPickupTypes.Contains(action) ? value : int.Parse((string)value);
    }

    public bool IsStringVal() => StringValPickupTypes.Contains(this.Action);

    public string Action;

    public object Value;

    public override string ToString() => $"{this.Action}|{this.Value}";

    public List<RandomizerAction> Decompose() {
        var ret = new List<RandomizerAction>();
        if(this.Action == "MU" || this.Action == "RP") {
            try {
                string[] pieces = ((string)this.Value).Split('/');
                for(int i = 0; i < pieces.Length; i+=2) {
                    ret.Add(new RandomizerAction(pieces[i], pieces[i+1]));
                }
            } catch(Exception e) {
                Randomizer.LogError($"Malformed Multipickup {this.Action}|{this.Value}, treating as {String.Join(",", ret.Select(r=>$"{r}").ToArray())}\nError Msg: {e.Message}");
            }
        } else {
            ret.Add(this);
        }   
        return ret;
    }

    public static RandomizerAction AsMulti(List<RandomizerAction> actions, bool repeatable = false) =>
         new RandomizerAction(repeatable ? "RP" : "MU", String.Join("/", actions.Select(act => $"{act.Action}/{act.Value}").ToArray()));
}
