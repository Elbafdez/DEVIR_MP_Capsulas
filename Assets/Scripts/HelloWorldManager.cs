using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
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

                // Método que controla botón y tecla M
                MoverAInicioBoton();
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
            var mode = m_NetworkManager.IsHost ? "Host" : m_NetworkManager.IsServer ? "Server" : "Client";
            GUILayout.Label("Transport: " + m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        private void MoverAInicioBoton()
        {
            if (GUILayout.Button("Mover a inicio") || Input.GetKeyDown(KeyCode.M))
            {
                if (m_NetworkManager.IsServer)
                {
                    // El servidor pide a cada jugador que se mueva
                    foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
                    {
                        var player = m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid)
                            .GetComponent<HelloWorldPlayer>();
                        player.RequestMoveToStart();  // Llamada pública que invoca ServerRpc
                    }
                }
                else
                {
                    // Cliente pide moverse solo él
                    var localPlayer = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
                    var player = localPlayer.GetComponent<HelloWorldPlayer>();
                    player.RequestMoveToStartServerRpc();
                }
            }
        }
    }
}
