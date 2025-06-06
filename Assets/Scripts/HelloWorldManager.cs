using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    /// <summary>
    /// Add this component to the same GameObject as
    /// the NetworkManager component.
    /// </summary>
    public class HelloWorldManager : MonoBehaviour
    {
        private NetworkManager m_NetworkManager;

        private void Awake()
        {
            m_NetworkManager = GetComponent<NetworkManager>();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
            if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
        
                if (GUILayout.Button("Mover a inicio"))
                {
                    if (m_NetworkManager.IsServer)
                    {
                        // Mover todos los jugadores
                        foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
                        {
                            var player = m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid)
                                .GetComponent<HelloWorldPlayer>();
                            player.MoveToStart();
                        }
                    }
                    else
                    {
                        var player = m_NetworkManager.SpawnManager.GetLocalPlayerObject()
                            .GetComponent<HelloWorldPlayer>();
                        player.MoveToStart(); // El cliente pide moverse solo él
                    }
                }
            }
        
            GUILayout.EndArea();
        }

        private void StartButtons()
        {
            if (GUILayout.Button("Host")) m_NetworkManager.StartHost();
            if (GUILayout.Button("Client")) m_NetworkManager.StartClient();
            if (GUILayout.Button("Server")) m_NetworkManager.StartServer();
        }

        private void StatusLabels()
        {
            var mode = m_NetworkManager.IsHost ?
                "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        private void MoverAInicioBoton()
        {
            if (GUILayout.Button("Mover a inicio") || Input.GetKeyDown(KeyCode.M))
            {
                if (m_NetworkManager.IsServer && !m_NetworkManager.IsClient)
                {
                    // Ejecutar para todos los jugadores
                    foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
                    {
                        var player = m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid)
                            .GetComponent<HelloWorldPlayer>();
                        player.MoveToStart();
                    }
                }
                else
                {
                    // Solo para el jugador local
                    var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.MoveToStart();
                }
            }
        }
    }
}