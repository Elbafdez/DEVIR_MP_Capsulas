using UnityEngine;
using Unity.Netcode;

public class ServerSettingsUI : MonoBehaviour
{
    public HelloWorld.HelloWorldPlayer gameLogic; // Referencia al script principal (para modificar MaxPlayersPerTeam)

    private int maxPlayersPerTeam = 2; // Valor por defecto

    private bool isServer = false;

    private void Start()
    {
        isServer = NetworkManager.Singleton.IsServer;
    }

    private void OnGUI()
    {
        if (!isServer) return; // Solo mostrar UI para servidor

        GUILayout.BeginArea(new Rect(10, 10, 250, 150), "Server Settings", GUI.skin.window);

        GUILayout.Label("Max players per team:");
        maxPlayersPerTeam = Mathf.Clamp(
            int.Parse(GUILayout.TextField(maxPlayersPerTeam.ToString(), 2)),
            1, 10); // Limitar entre 1 y 10

        if (GUILayout.Button("Apply"))
        {
            ApplyMaxPlayers();
        }

        GUILayout.EndArea();
    }

    private void ApplyMaxPlayers()
    {
        if (gameLogic != null)
        {
            HelloWorld.HelloWorldPlayer.MaxPlayersPerTeamStatic = maxPlayersPerTeam;
            Debug.Log($"Max players per team set to {maxPlayersPerTeam}");
        }
        else
        {
            Debug.LogWarning("GameLogic reference not assigned!");
        }
    }
}
