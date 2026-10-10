using System;
using System.IO;
using System.Linq;
using Necrom.Core.Domain;
using UnityEditor;
using UnityEngine;
// Explicit, isolated manager verification. Never loads/saves live PlayerPrefs or runtime roster.
[InitializeOnLoad]
public static class NecroGachaMileageProbe
{
    static NecroGachaMileageProbe(){EditorApplication.update+=Tick;}
    static void Tick()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        var root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
        var job=Path.Combine(root,"Library/gacha-mileage-job.txt");
        if(!File.Exists(job))return;
        try{File.Delete(job);}catch(IOException){return;}
        var output=Path.Combine(root,"docs/evidence/Gacha-Mileage-20261011");
        Directory.CreateDirectory(output);
        try
        {
            var data=new MonsterCatalogData{monsters=Enum.GetNames(typeof(MonsterRarity)).Select((r,i)=>
                new MonsterEntry{id="QA_M"+i,rarity=r,gachaWeight=1,traits=new[]{"Abyss","Supporter"}}).ToArray()};
            var g=new MonsterGachaManager(data,new PermanentMonsterRoster(),new System.Random(7),GachaGuaranteeMode.SelectionMileage);
            g.RestoreMileageState(new GachaMileageState{successfulDraws=198});
            if(g.DrawBatch((_,__)=>false,DrawCurrency.Diamond,100,10)!=null || g.MileageProgress!=198)throw new Exception("Failed spend mutated mileage");
            int payments=0;
            var draws=g.DrawBatch((_,__)=>{payments++;return true;},DrawCurrency.Diamond,100,10);
            if(draws.Count!=10||payments!=1||g.AvailableSelectionTickets!=1||g.MileageProgress!=8)throw new Exception("Ten draw boundary failed");
            var json=JsonUtility.ToJson(g.CaptureMileageState(),true);
            var h=new MonsterGachaManager(data,new PermanentMonsterRoster(),new System.Random(7),GachaGuaranteeMode.SelectionMileage);
            h.RestoreMileageState(JsonUtility.FromJson<GachaMileageState>(json));
            var result=h.RedeemMileage(new MileageSelection(TftOrigin.Abyss),"QA_M4");
            if(result==null||result.Monster.id!="QA_M4"||!h.Roster.Owns("QA_M4")||h.AvailableSelectionTickets!=0||
                h.RedeemMileage(new MileageSelection(TftClass.Supporter),"QA_M4")!=null)throw new Exception("Selected grant/duplicate consumption failed");
            File.WriteAllText(Path.Combine(output,"snapshot.json"),JsonUtility.ToJson(h.CaptureMileageState(),true));
            File.WriteAllText(Path.Combine(output,"unity-validation.txt"),DateTimeOffset.Now.ToString("o")+"\nUnity "+Application.unityVersion+
                "\nNative manager PASS: failed spend unchanged; 198+10=208 -> ticket1/progress8; one payment; JsonUtility state roundtrip; exact Abyss Legendary grant; duplicate redemption refused\nIsolated TEST catalog/roster. UI/PlayerPrefs/asset Import/combat unchanged; gameplay/device/build NOT RUN.\n");
            Debug.Log("[GACHA-MILEAGE] PASS");
        }catch(Exception e){File.WriteAllText(Path.Combine(output,"unity-validation.txt"),"FAIL "+e);Debug.LogException(e);}
    }
}
