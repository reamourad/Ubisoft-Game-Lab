using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Player;
using Player.Audio;
using Player.NotificationSystem;
using StateManagement.StateManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace StateManagement
{
    public class GameController : NetworkBehaviour
    {
        public enum GameStage
        {
            None = 0,
            Preparing,
            Game,
            Postgame
        }
        
        [SerializeField] private NetworkObject seekerPrefab;
        [FormerlySerializedAs("seekerSpawn")] [SerializeField] public Transform SeekerSpawn;
        
        [SerializeField] private NetworkObject hiderPrefab;
        [FormerlySerializedAs("hiderSpawn")] [SerializeField] public Transform HiderSpawn;

        [Header("Game Variables")] 
        [SerializeField] private int prepTimeSeconds = 30;
        [SerializeField] private int gameTimeSeconds = 480;
        
        [Header("Particles")]
        [SerializeField] private GameObject vacuumedSuckedParticles;
        
        [Header("Audio")]
        [SerializeField] private AudioClip mainSeekerBGM;
        [SerializeField] private AudioClip mainHiderBGM;
        [SerializeField] private AudioClip chaseBGM;
        [SerializeField] private AudioClip gameBeginSound;

        [SerializeField] private float angyDelay = 0.5f;
        [SerializeField] private int maxTimePenalty = 45;
        
        [SerializeField] private AudioClip ambiance;
        
        [SerializeField] private AudioClip houseAngySFX;
        [SerializeField] private GameObject cameraShakeObj;

        [SerializeField] private Animator houseAnimator;
        
        
        private readonly SyncVar<GameStage> currentStage = new SyncVar<GameStage>(GameStage.None);
        public GameStage CurrentGameStage =>currentStage.Value;
        public Action<GameStage> OnStageChanged;
        
        private List<NetworkObject> players;
        private readonly SyncVar<PlayerRole.RoleType> GameWinner = new SyncVar<PlayerRole.RoleType>(PlayerRole.RoleType.None);

       
        
        private static Dictionary<int, PlayerRole.RoleType> createdRoles = new Dictionary<int, PlayerRole.RoleType>();
        public static bool IsReplayingGame { get;set;}

        private int readyClients=0;
        // singleton pattern
        private static GameController instance;
        public static GameController Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<GameController>();
                
                return instance;
            }
        }
        
        private IEnumerator Start()
        {
            //we start from the next frame
            //loading is already heavy
            yield return new WaitForSeconds(0.1f);
            
            GameWinner.OnChange += GameWinnerOnOnChange;
            NoiseManager.OnNoiseGenerated += OnServerNoiseGenerated;

            currentStage.OnChange += TriggerGameStageChangedEvent;
            
            if (IsClientStarted)
            {
                yield return new WaitUntil(() => GameWinner.Value == PlayerRole.RoleType.None);
                if (IsServerStarted)
                {
                    ServerOnClientReady();
                }
                else
                {
                    RPC_InformClientIsReady();
                }

                PlayMainTheme();
                InputReader.Instance.OnPauseEvent += OnPauseToggled;
                OnStageChanged += OnGameStageChanged;
            }
        }

        private void TriggerGameStageChangedEvent(GameStage prev, GameStage next, bool asserver)
        {
            OnStageChanged?.Invoke(next);
        }


        private void OnDestroy()
        {
            OnStageChanged -= OnGameStageChanged;
            InputReader.Instance.OnPauseEvent -= OnPauseToggled;
            GameWinner.OnChange -= GameWinnerOnOnChange;
        }
        
        private void OnGameStageChanged(GameStage stage)
        {
            if (stage == GameStage.Preparing)
            {
                InputReader.Instance.SetToGameplayInputs();
            }
            
            if (stage == GameStage.Game)
            {
                NotificationSystem.Instance.Notify("The hunt begins!");
                AudioManager.Instance.PlaySFX(gameBeginSound);
            }
        }
        
        private void OnPauseToggled()
        {
            PauseMenuController.ShowPauseMenu(!PauseMenuController.IsShowing);
        }
        
        private void PlayMainTheme()
        {
            StartCoroutine(StartPlayingMainTheme());
        }
        
        private IEnumerator StartPlayingMainTheme()
        {
            Debug.Log("GameController:: Playing Main Theme");
            yield return new WaitUntil(() => GameLookupMemory.LocalPlayer != null);
            if (GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Hider)
                AudioManager.Instance.PlayBG(mainHiderBGM);
            else if (GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Seeker)
            {
                AudioManager.Instance.PlayBG(mainSeekerBGM);
                AudioManager.Instance.PlayAmbience(ambiance);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RPC_InformClientIsReady()
        {
            ServerOnClientReady();
        }

        [Server]
        private void ServerOnClientReady()
        {
            readyClients += 1;
            if (readyClients >= 0)
            {
                GameWinner.Value = PlayerRole.RoleType.None;
                SwitchGameStage(GameStage.Preparing);
            }
        }
        
        private void GameWinnerOnOnChange(PlayerRole.RoleType prev, PlayerRole.RoleType next, bool asserver)
        {
            Debug.Log("GameLookupMemory Winner Data Modified!!");
            GameLookupMemory.Winner = next;
        }

        [Server]
        private void ServerSpawnPlayers()
        {
            Debug.Log("GameController:: Spawning Players");
            var networkManager = InstanceFinder.NetworkManager;
            players = new List<NetworkObject>();
            foreach (var connection in InstanceFinder.NetworkManager.ServerManager.Clients)
            {
                var randSel = SelectRandomSide(connection.Key);
                Debug.Log($"GameController:: Spawning Player: {{ connection.Value.ClientId}} ({randSel.prefab.name})");
                
                NetworkObject nob = networkManager.GetPooledInstantiated(randSel.prefab,randSel.spawnPoint.position, randSel.spawnPoint.rotation, true);
                networkManager.ServerManager.Spawn(nob, connection.Value);
                players.Add(nob);
            }
        }
        
        [Server]
        public void ServerInitiateChase()
        {
            RPC_ClientPlayChaseMusic();
        }
        
        [Server]
        public void ServerStopChase()
        {
            RPC_ClientStopChaseMusic();
        }

        [ObserversRpc(ExcludeOwner = false)]
        private void RPC_ClientPlayChaseMusic()
        {
            AudioManager.Instance.PlayBG(chaseBGM);
        }
        
        [ObserversRpc(ExcludeOwner = false)]
        private void RPC_ClientStopChaseMusic()
        {
            if (GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Hider)
                AudioManager.Instance.PlayBG(mainHiderBGM);
            else if (GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Seeker)
            {
                AudioManager.Instance.PlayBG(mainSeekerBGM);
            }
        }
        
        
        //Lets not look here, its quite shitty :p
        private (NetworkObject prefab, Transform spawnPoint) SelectRandomSide(int id)
        {

            if (IsReplayingGame)
            {
                if (!createdRoles.ContainsKey(id))
                {
                    IsReplayingGame = false;
                    createdRoles.Clear();
                    return SelectRandomSideRegular();
                }
                
                //invert roles
                var role = createdRoles[id];
                switch (role)
                {
                    case PlayerRole.RoleType.Seeker:
                        return (hiderPrefab, HiderSpawn);
                    case PlayerRole.RoleType.Hider:
                        return (seekerPrefab, SeekerSpawn);
                    default:
                    {
                        IsReplayingGame = false;
                        return SelectRandomSideRegular();
                    }
                }
            }
            return SelectRandomSideRegular();
        }

        private (NetworkObject prefab, Transform spawnPoint) SelectRandomSideRegular()
        {
            NetworkObject prefab;
            Transform spawnPoint;
            if (players.Count == 0)
            {
                if(Random.Range(0, 100) % 2 == 0)
                {
                    prefab = seekerPrefab;
                    spawnPoint = SeekerSpawn;
                }
                else
                {
                    prefab = hiderPrefab;
                    spawnPoint = HiderSpawn;
                }
            }
            else
            {
                var playerRole = players[0].GetComponent<PlayerRole>().Role;
                if(playerRole == PlayerRole.RoleType.Hider)
                {
                    prefab = seekerPrefab;
                    spawnPoint = SeekerSpawn;
                }
                else
                {
                    prefab = hiderPrefab;
                    spawnPoint = HiderSpawn;
                }
            }
            
            return (prefab, spawnPoint);
        }
        
        private void SwitchGameStage(GameStage stage)
        {
            if(!IsServerStarted)
                return;
            
            if(currentStage.Value == stage)
                return;
            
            Debug.Log($"GameController:: Switching Game Stage {currentStage.Value} --> {stage}");
            switch (stage)
            {
                case GameStage.Preparing:
                    ServerGamePrepareStage();
                    break;
                
                case GameStage.Game:
                    ServerGamePlayStage();
                    break;
                case GameStage.Postgame:
                    ServerGamePostStage();
                    break;
            }
            currentStage.Value = stage;
            RPC_InformClientsOfGameStageChange(currentStage.Value);
            Debug.Log($"GameController:: Switched Game Stage {currentStage.Value} --> {stage}");
        }

        [ObserversRpc]
        void RPC_InformClientsOfGameStageChange(GameStage stage)
        {
            currentStage.Value = stage;
            OnStageChanged?.Invoke(currentStage.Value);
        }
        
        private void ServerGamePrepareStage()
        {
            ServerSpawnPlayers();
            Networking.TimeManager.Instance?.Initialize(prepTimeSeconds, () =>
            {
                SwitchGameStage(GameStage.Game);
            });
            InputReader.Instance.SetToGameplayInputs();
            IsReplayingGame = false;
        }
        
        private void ServerGamePlayStage()
        {
            Networking.TimeManager.Instance.Initialize(gameTimeSeconds, ServerGameTimeRanOut);
        }
        
        private void ServerGamePostStage()
        { 
            if(!IsServerStarted)
                return;
            
            GameStateController.Instance.ServerChangeState(GameStates.GameOver);
        }
        
        /// <summary>
        /// Considers the game to be won by the Hider, Changes state to post game after 2 seconds.
        /// </summary>
        private void ServerGameTimeRanOut()
        {
            if(!IsServerStarted)
                return;
            GameWinner.Value = PlayerRole.RoleType.Hider;
            StartCoroutine(DelayedInvoke(() =>
            {
                SwitchGameStage(GameStage.Postgame);
            }, 2f));
        }
        
        /// <summary>
        /// Considers the game to be won by the seeker, Changes state to post game after 3 seconds.
        /// </summary>
        public void ServerHiderCaptured()
        {
            if (!IsServerStarted)
            {
                Debug.Log("GameController::Invoking On Server!!");
                RPC_InvokeServerHiderCapturedOnServer();
                return;
            }
                
            if(currentStage.Value != GameStage.Game)
                return;

            var hider = players.Find(a => a.GetComponent<PlayerRole>().Role == PlayerRole.RoleType.Hider);
            if (hider != null)
            {
                RPC_SpawnDustParticles(hider.transform.position + hider.GetComponent<Rigidbody>().centerOfMass);
                hider.Despawn();
            }

            Networking.TimeManager.Instance.StopActiveTimer();
            GameWinner.Value = PlayerRole.RoleType.Seeker;
            StartCoroutine(DelayedInvoke(() =>
            {
                SwitchGameStage(GameStage.Postgame);
            }, 0.1f));
        }

        [ObserversRpc]
        private void RPC_SpawnDustParticles(Vector3 position)
        {
            var go = Instantiate(vacuumedSuckedParticles, position, Quaternion.identity);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RPC_InvokeServerHiderCapturedOnServer()
        {
            ServerHiderCaptured();
        }
        
        private IEnumerator DelayedInvoke(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        
        public NetworkObject GetPlayer(int clientId)
        {
            return players.Find(a => a.LocalConnection.ClientId == clientId);
        }

        [Server]
        private void OnServerNoiseGenerated(Vector3 position, float strength, float dissipation)
        {
            if(!IsServerStarted)
                return;
            
            if(CurrentGameStage != GameStage.Game)
                return;
            
            StartCoroutine(DelayedInvoke(() =>
            {
                ServerDoTimePenalty(strength);
            }, angyDelay));
            RPC_InvokeHouseAngy(angyDelay);
        }

        [Server]
        private void ServerDoTimePenalty(float strength)
        {
            Networking.TimeManager.Instance.ApplyPenalty((int)(maxTimePenalty * strength));
        }
        
        [ObserversRpc]
        private void RPC_InvokeHouseAngy(float delay)
        {
            StartCoroutine(ClientHouseAngy(delay));
        }

        [Client]
        private IEnumerator ClientHouseAngy(float delay)
        {
            houseAnimator.SetTrigger("SirShakesALot");
            yield return new WaitForSeconds(delay);
            AudioManager.Instance.PlayMonsterSFX(houseAngySFX);
            var go = Instantiate(cameraShakeObj);

            if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            {
                Gamepad.current.SetMotorSpeeds(0.5f, 0.75f);
            }
            
            yield return new WaitForSeconds(houseAngySFX.length);
            
            if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            {
                Gamepad.current.ResetHaptics();
            }
            
            Destroy(go, houseAngySFX.length);
        }

        public void AggitateHouse()
        {
            houseAnimator.SetBool("Agitate", true);
            StartCoroutine(DelayedInvoke(() =>
            {
                houseAnimator.SetBool("Agitate", false);
            }, 2f));
        }
    }
}