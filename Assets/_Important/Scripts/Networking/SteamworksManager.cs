using UnityEngine;
using System.Collections;
using Steamworks;

public class SteamworksManager : MonoBehaviour {
	public static SteamworksManager instance;
	private Vector2 m_ScrollPos;
	private int m_NumGamesStat;
	private float m_FeetTraveledStat;
	private bool m_AchievedWinOneGame;
	private SteamLeaderboard_t m_SteamLeaderboard;
	private SteamLeaderboardEntries_t m_SteamLeaderboardEntries;
	private Texture2D m_Icon;

	protected Callback<UserStatsReceived_t> m_UserStatsReceived;
	protected Callback<UserStatsStored_t> m_UserStatsStored;
	protected Callback<UserAchievementStored_t> m_UserAchievementStored;
	protected Callback<UserStatsUnloaded_t> m_UserStatsUnloaded;
	protected Callback<UserAchievementIconFetched_t> m_UserAchievementIconFetched;

	private CallResult<UserStatsReceived_t> OnUserStatsReceivedCallResult;
	private CallResult<LeaderboardFindResult_t> OnLeaderboardFindResultCallResult;
	private CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloadedCallResult;
	private CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploadedCallResult;
	private CallResult<NumberOfCurrentPlayers_t> OnNumberOfCurrentPlayersCallResult;
	private CallResult<GlobalAchievementPercentagesReady_t> OnGlobalAchievementPercentagesReadyCallResult;
	private CallResult<LeaderboardUGCSet_t> OnLeaderboardUGCSetCallResult;
	private CallResult<GlobalStatsReceived_t> OnGlobalStatsReceivedCallResult;

    //resources for easy calling
    SteamLeaderboard_t infiniteLeaderboard;


    public void OnEnable() {
		instance = this;
        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
		m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnUserAchievementStored);
		m_UserStatsUnloaded = Callback<UserStatsUnloaded_t>.Create(OnUserStatsUnloaded);
		m_UserAchievementIconFetched = Callback<UserAchievementIconFetched_t>.Create(OnUserAchievementIconFetched);

		OnUserStatsReceivedCallResult = CallResult<UserStatsReceived_t>.Create(OnUserStatsReceived);
		OnLeaderboardFindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
		OnLeaderboardScoresDownloadedCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
		OnLeaderboardScoreUploadedCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
		OnNumberOfCurrentPlayersCallResult = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
		OnGlobalAchievementPercentagesReadyCallResult = CallResult<GlobalAchievementPercentagesReady_t>.Create(OnGlobalAchievementPercentagesReady);
		OnLeaderboardUGCSetCallResult = CallResult<LeaderboardUGCSet_t>.Create(OnLeaderboardUGCSet);
		OnGlobalStatsReceivedCallResult = CallResult<GlobalStatsReceived_t>.Create(OnGlobalStatsReceived);

        if (SteamManager.Initialized)
        {
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);
        }

        //setup the needed resources
        SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard("Infinite Wave");
        OnLeaderboardFindResultCallResult.Set(hSteamAPICall, OnLeaderboardFindResult);
        infiniteLeaderboard = new SteamLeaderboard_t(2405130);
        UpdateInfiniteWaveScore(1, 1);
    }

	void OnUserStatsReceived(UserStatsReceived_t pCallback) {
		Debug.Log("[" + UserStatsReceived_t.k_iCallback + " - UserStatsReceived] - " + pCallback.m_nGameID + " -- " + pCallback.m_eResult + " -- " + pCallback.m_steamIDUser);

		// The Callback version is for the local player RequestCurrentStats(), and the CallResult version is for other players with RequestUserStats()
	}

	void OnUserStatsReceived(UserStatsReceived_t pCallback, bool bIOFailure) {
		Debug.Log("[" + UserStatsReceived_t.k_iCallback + " - UserStatsReceived] - " + pCallback.m_nGameID + " -- " + pCallback.m_eResult + " -- " + pCallback.m_steamIDUser);

		// The Callback version is for the local player RequestCurrentStats(), and the CallResult version is for other players with RequestUserStats()
	}

	void OnUserStatsStored(UserStatsStored_t pCallback) {
		Debug.Log("[" + UserStatsStored_t.k_iCallback + " - UserStatsStored] - " + pCallback.m_nGameID + " -- " + pCallback.m_eResult);
	}

	void OnUserAchievementStored(UserAchievementStored_t pCallback) {
		Debug.Log("[" + UserAchievementStored_t.k_iCallback + " - UserAchievementStored] - " + pCallback.m_nGameID + " -- " + pCallback.m_bGroupAchievement + " -- " + pCallback.m_rgchAchievementName + " -- " + pCallback.m_nCurProgress + " -- " + pCallback.m_nMaxProgress);
	}

	void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure) {
		Debug.Log("[" + LeaderboardFindResult_t.k_iCallback + " - LeaderboardFindResult] - " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_bLeaderboardFound);
		if (pCallback.m_bLeaderboardFound != 0) {
			m_SteamLeaderboard = pCallback.m_hSteamLeaderboard;
		}
	}

	void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure) {
        if(pCallback.m_hSteamLeaderboard.m_SteamLeaderboard == infiniteLeaderboard.m_SteamLeaderboard)
        {
            LevelSelect.instance.PopulateInfiniteWaveLeaderboard(pCallback);
        }
	}

	void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure) {
		Debug.Log("[" + LeaderboardScoreUploaded_t.k_iCallback + " - LeaderboardScoreUploaded] - " + pCallback.m_bSuccess + " -- " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_nScore + " -- " + pCallback.m_bScoreChanged + " -- " + pCallback.m_nGlobalRankNew + " -- " + pCallback.m_nGlobalRankPrevious);
	}

	void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure) {
		Debug.Log("[" + NumberOfCurrentPlayers_t.k_iCallback + " - NumberOfCurrentPlayers] - " + pCallback.m_bSuccess + " -- " + pCallback.m_cPlayers);
	}

	void OnUserStatsUnloaded(UserStatsUnloaded_t pCallback) {
		Debug.Log("[" + UserStatsUnloaded_t.k_iCallback + " - UserStatsUnloaded] - " + pCallback.m_steamIDUser);
	}

	void OnUserAchievementIconFetched(UserAchievementIconFetched_t pCallback) {
		Debug.Log("[" + UserAchievementIconFetched_t.k_iCallback + " - UserAchievementIconFetched] - " + pCallback.m_nGameID + " -- " + pCallback.m_rgchAchievementName + " -- " + pCallback.m_bAchieved + " -- " + pCallback.m_nIconHandle);
	}

	void OnGlobalAchievementPercentagesReady(GlobalAchievementPercentagesReady_t pCallback, bool bIOFailure) {
		Debug.Log("[" + GlobalAchievementPercentagesReady_t.k_iCallback + " - GlobalAchievementPercentagesReady] - " + pCallback.m_nGameID + " -- " + pCallback.m_eResult);
	}

	void OnLeaderboardUGCSet(LeaderboardUGCSet_t pCallback, bool bIOFailure) {
		Debug.Log("[" + LeaderboardUGCSet_t.k_iCallback + " - LeaderboardUGCSet] - " + pCallback.m_eResult + " -- " + pCallback.m_hSteamLeaderboard);
	}

	void OnGlobalStatsReceived(GlobalStatsReceived_t pCallback, bool bIOFailure) {
		Debug.Log("[" + GlobalStatsReceived_t.k_iCallback + " - GlobalStatsReceived] - " + pCallback.m_nGameID + " -- " + pCallback.m_eResult);
	}

	public void GetInfiniteWaveLeaderboard(){

    }

    public void UpdateInfiniteWaveScore(int score, int wave)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steamworks not initialized");
        }
        else
        {
            SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(infiniteLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, new int[] { wave }, 1);
            OnLeaderboardScoreUploadedCallResult.Set(hSteamAPICall, OnLeaderboardScoreUploaded);
        }
    }

    public void GetInfiniteWaveScore()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steamworks not initialized");
        }
        else
        {
            SteamAPICall_t hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(infiniteLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -3, 3);
            OnLeaderboardScoresDownloadedCallResult.Set(hSteamAPICall, OnLeaderboardScoresDownloaded);
        }
    }
}