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

        // Máximo jugadores por equipo
        private const int MaxPlayersPerTeam = 2;

        // Estado actual del jugador
        private enum TeamZone { None, Team1, Team2 }
        private TeamZone currentZone = TeamZone.None;

        // Colores asignables
        private static List<Color> team1Colors = new List<Color> { Color.red, new Color(1f, 0.5f, 0f), Color.magenta };
        private static List<Color> team2Colors = new List<Color> { Color.blue, new Color(0.5f, 0f, 1f), new Color(0.5f, 0.5f, 1f) };

        private static Dictionary<Color, ulong> usedColors = new Dictionary<Color, ulong>();

        // Jugadores actuales por equipo
        private static HashSet<ulong> team1Players = new HashSet<ulong>();
        private static HashSet<ulong> team2Players = new HashSet<ulong>();

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

        private void SetColor(Color color)
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            _renderer.material.color = color;
        }

        private void Update()
        {
            if (!IsOwner) return;

            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;

            Vector3 targetPos = transform.position + move;

            if (IsMoveAllowed(targetPos))
            {
                transform.position = targetPos;

                RequestZoneCheckServerRpc(transform.position);
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                MoveToStart();
            }
        }

        private bool IsMoveAllowed(Vector3 targetPos)
        {
            float x = targetPos.x;

            if (x < Team1ZoneXMax)
            {
                // Zona equipo 1
                if (team1Players.Count >= MaxPlayersPerTeam && !team1Players.Contains(OwnerClientId))
                {
                    return false;
                }
            }
            else if (x > Team2ZoneXMin)
            {
                // Zona equipo 2
                if (team2Players.Count >= MaxPlayersPerTeam && !team2Players.Contains(OwnerClientId))
                {
                    return false;
                }
            }
            return true;
        }

        [Rpc(SendTo.Server)]
        private void RequestMoveToStartServerRpc()
        {
            MoveToStart();
        }

        [Rpc(SendTo.Server)]
        private void RequestZoneCheckServerRpc(Vector3 position)
        {
            CheckZoneChange(position);
        }

        public void MoveToStart()
        {
            transform.position = GetRandomCentralPosition();
            playerColor.Value = Color.white;
            ReleaseColorAndTeam();
            currentZone = TeamZone.None;
        }

        private void CheckZoneChange(Vector3 pos)
        {
            float x = pos.x;
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
            if (newZone == currentZone) return;

            ReleaseColorAndTeam();

            switch (newZone)
            {
                case TeamZone.Team1:
                    team1Players.Add(OwnerClientId);
                    SetTeamColor(team1Colors);
                    break;
                case TeamZone.Team2:
                    team2Players.Add(OwnerClientId);
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
            // Si no hay color disponible, deja blanco o algún color por defecto
            playerColor.Value = Color.white;
        }

        private void ReleaseColorAndTeam()
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

            team1Players.Remove(OwnerClientId);
            team2Players.Remove(OwnerClientId);
        }

        private static Vector3 GetRandomCentralPosition()
        {
            float x = Random.Range(-2f, 2f);
            float z = Random.Range(-3f, 3f);
            return new Vector3(x, 1f, z);
        }
    }
}