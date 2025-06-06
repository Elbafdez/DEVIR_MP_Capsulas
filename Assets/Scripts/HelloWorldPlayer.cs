using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public float moveSpeed = 5f;
        private Renderer _renderer;

        // Constantes de zona (ajustado al eje X)
        private const float Team1ZoneXMax = -2f;
        private const float Team2ZoneXMin = 2f;

        // Estado actual del jugador
        private enum TeamZone { None, Team1, Team2 }
        private TeamZone currentZone = TeamZone.None;

        // Colores asignables
        private static List<Color> team1Colors = new List<Color> { Color.red, new Color(1f, 0.5f, 0f), Color.magenta };
        private static List<Color> team2Colors = new List<Color> { Color.blue, new Color(0.5f, 0f, 1f), new Color(0.5f, 0.5f, 1f) };

        private static Dictionary<Color, ulong> usedColors = new Dictionary<Color, ulong>();

        private NetworkVariable<Color> playerColor = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            _renderer = GetComponent<Renderer>();
            playerColor.OnValueChanged += OnColorChanged;
            SetColor(playerColor.Value);

            if (IsServer)
            {
                MoveToStart();
            }
        }

        private void OnColorChanged(Color previous, Color current)
        {
            SetColor(current);
        }

        private void Update()
        {
            if (!IsOwner) return;

            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;
            transform.Translate(move);

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (IsServer) MoveToStart();
                else RequestMoveToStartServerRpc();
            }

            if (IsServer)
            {
                CheckZoneChange();
            }
        }

        [Rpc(SendTo.Server)]
        private void RequestMoveToStartServerRpc()
        {
            MoveToStart();
        }

        public void MoveToStart()
        {
            transform.position = GetRandomCentralPosition();
            playerColor.Value = Color.white;
            ReleaseColor();
            currentZone = TeamZone.None;
        }

        private void CheckZoneChange()
        {
            float x = transform.position.x;
            TeamZone newZone = TeamZone.None;

            if (x < Team1ZoneXMax)
                newZone = TeamZone.Team1;
            else if (x > Team2ZoneXMin)
                newZone = TeamZone.Team2;

            if (newZone != currentZone)
            {
                HandleZoneChange(newZone);
            }
        }

        private void HandleZoneChange(TeamZone newZone)
        {
            ReleaseColor();
            switch (newZone)
            {
                case TeamZone.Team1:
                    SetTeamColor(team1Colors);
                    break;
                case TeamZone.Team2:
                    SetTeamColor(team2Colors);
                    break;
                case TeamZone.None:
                    playerColor.Value = Color.white;
                    break;
            }

            currentZone = newZone;
        }

        private void SetTeamColor(List<Color> teamColors)
        {
            foreach (var color in teamColors)
            {
                if (!usedColors.ContainsKey(color))
                {
                    usedColors[color] = OwnerClientId;
                    playerColor.Value = color;
                    return;
                }
            }
        }

        private void ReleaseColor()
        {
            List<Color> toRemove = new List<Color>();
            foreach (var kvp in usedColors)
            {
                if (kvp.Value == OwnerClientId)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var color in toRemove)
            {
                usedColors.Remove(color);
            }
        }

        private void SetColor(Color color)
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            _renderer.material.color = color;
        }

        private static Vector3 GetRandomCentralPosition()
        {
            float x = Random.Range(-2f, 2f);
            float z = Random.Range(-3f, 3f);
            return new Vector3(x, 1f, z);
        }
    }
}