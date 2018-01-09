using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelType{
	Exit = 0,
	Rescue = 1,
	Survive = 2,
	Wave = 3,
	Timed = 4,
	Boss = 6,
	InfiniteWave = 7
}

[System.Serializable]
public class LevelSetType{
	[SerializeField]
	public string seedString;
	[SerializeField]
	public LevelType type;
	public int level;
}

public class Level : MonoBehaviour
{

    public int level, seed, islandCount;
    public string seedString;
    public LevelType type;
    public int special;
    public float sizeMin, sizeMax;

    public List<string> badWords = new List<string> {"anal","anus","arse","ass"
        ,"ballsack","balls","bastard","bitch","biatch","bloody","blowjob"
        ,"bollock","bollok","boner","boob","bugger","bum","butt","buttplug"
        ,"clitoris","cock","coon","crap","cunt","damn","dick","dildo","dyke","fag"
        ,"feck","fellate","fellatio","felching","fuck","fudgepacker"
        ,"flange","Goddamn","hell","homo","jerk","jizz","knobend","labia","lmao","lmfao"
        ,"muff","nigger","nigga","omg","penis","piss","poop","prick","pube","pussy","queer"
        ,"scrotum","sex","shit","sh1t","slut","smegma","spunk","tit","tosser","turd","twat"
        ,"vagina","wank","whore","wtf"};

    public void Init(LevelSetType setType)
    {
        seedString = setType.seedString;
        type = setType.type;
        level = setType.level;
        for (int i = 0; i < seedString.Length; i++)
        {
            seed += char.ConvertToUtf32(seedString, i);
        }
        Random.InitState(seed);
        switch (type)
        {
            case LevelType.Exit:
                islandCount = level + 10;
                break;
            case LevelType.Rescue:
                islandCount = level + 5;
                break;
            case LevelType.Survive:
                islandCount = Mathf.FloorToInt(Random.value * 4 + 4);
                break;
            case LevelType.Wave:
                islandCount = Mathf.FloorToInt(Random.value * 4 + 4);
                break;
            case LevelType.Timed:
                islandCount = level + 5;
			break;
		case LevelType.Boss:
			islandCount = Mathf.FloorToInt(Random.value * 4 + 4);
			break;
		case LevelType.InfiniteWave:
			islandCount = 1;
			sizeMax = 100;
			sizeMin = 100;
			break;
            default:
                break;
        }
    }

    public void SetState()
    {
        for (int i = 0; i < seedString.Length; i++)
        {
            seed += char.ConvertToUtf32(seedString, i);
        }
        Random.InitState(seed);
        switch (type)
        {
            case LevelType.Exit:
                break;
            case LevelType.Rescue:
                special = Mathf.FloorToInt(level / 10 + 3);
                break;
            case LevelType.Survive:
                special = 10 * level;
                break;
            case LevelType.Wave:
                special = Mathf.FloorToInt(level / 10 + 3);
                break;
            case LevelType.Timed:
                special = 120;
                break;
            case LevelType.Boss:
                break;
            default:
                break;
        }
        PlayerPrefs.SetInt("playingLevel", level);
        PlayerPrefs.SetInt("playingLevelType", (int)type);
        PlayerPrefs.SetInt("playingLevelSeed", seed);
        PlayerPrefs.SetInt("playingLevelIslandCount", islandCount);
        PlayerPrefs.SetFloat("playingLevelSizeMin", sizeMin);
        PlayerPrefs.SetFloat("playingLevelSizeMax", sizeMax);
        PlayerPrefs.SetInt("playingLevelSpecialValue", special);
        int hasBadWord = 0;
        foreach (string str in badWords)
        {
            hasBadWord += seedString.ToLower().Contains(str) ? 1 : 0;
        }
        PlayerPrefs.SetInt("playingLevelBadWord", hasBadWord);
    }
}
