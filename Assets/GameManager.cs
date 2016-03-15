using UnityEngine;
using System.Collections;
using SimpleJSON;

//Carson
public class GameManager : MonoBehaviour {
    public enum LobbyData { GameEnd = 1, Disconnected = 2 }

    void Start()
    {
        NetworkingManager.Subscribe(gameEnd, DataType.Lobby, (int)LobbyData.GameEnd);
    }

    public static void PlayerTookDamage(int playerID, float damage, BaseClass.PlayerBaseStat ClassStat)
    {
        if (GameData.MyPlayer.PlayerID == playerID)
        {
            HUD_Manager.instance.UpdatePlayerHealth(-(damage / ClassStat.MaxHp));
            if (ClassStat.CurrentHp <= 0)
                PlayerDied();
        }

        if (playerID == GameData.AllyKingID)
        {
            HUD_Manager.instance.UpdateAllyKingHealth(-(damage / ClassStat.MaxHp));
            if (ClassStat.CurrentHp <= 0)
                GameLost();

        }

        if (playerID == GameData.EnemyKingID)
        {
            HUD_Manager.instance.UpdateEnemyKingHealth(-(damage / ClassStat.MaxHp));
            if (ClassStat.CurrentHp <= 0)
                GameWon();
        }
    }

    private static void PlayerDied()
    {
        Debug.Log("You have died");

        //GameObject.Find("Main Camera").GetComponent<FollowCamera>().target = GameObject.Find
    }

    private void gameEnd(JSONClass packet)
    {
        if  (packet["TeamID"] == GameData.MyPlayer.TeamID)
        {
            GameWon();
        } else
        {
            GameLost();
        }
    }

    private static void GameWon()
    {
        Debug.Log("You have won");
    }

    private static void GameLost()
    {
        Debug.Log("You have lost");
    }
    
}
