using Assets.Authory.Scripts.Enum;
using Lidgren.Network;
using Lidgren.Network.Shared;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the client message reading and connection inside the game.
/// </summary>
public class ClientManager : MonoBehaviour
{
    public AuthoryClient AuthoryClient { get; set; }
    public MasterClientManager MasterClientManager { get; set; }

    void Awake()
    {
        MasterClientManager = FindObjectOfType<MasterClientManager>();

        //Force SpawnCollection Awake() method for initialization
        SpawnCollection spawnCollection = FindObjectOfType<SpawnCollection>();
        spawnCollection.gameObject.SetActive(false);
        spawnCollection.gameObject.SetActive(true);

        //Force SkillCollection Awake() method for initialization
        SkillCollection skillList = FindObjectOfType<SkillCollection>();
        skillList.gameObject.SetActive(false);
        skillList.gameObject.SetActive(true);

        AuthoryClient = new AuthoryClient(MasterClientManager.GetMapServerAuthString());
        AuthoryClient.Connect(MasterClientManager.MapServerIP, MasterClientManager.MapServerPort, MasterClientManager.MapServerUID);

        NetIncomingMessage msgIn;
        bool playerInfoArrived = false;
        DateTime start = DateTime.Now.AddSeconds(5);
        while (playerInfoArrived != true)
            if (start < DateTime.Now) return;
            else
                while ((msgIn = AuthoryClient.Client.ReadMessage()) != null)
                {
                    if (msgIn.MessageType == NetIncomingMessageType.Data)
                    {
                        if (((MessageType)msgIn.ReadByte()) == MessageType.PlayerID)
                        {
                            ushort Id = msgIn.ReadUInt16();
                            string Name = msgIn.ReadString();

                            int maxHealth = msgIn.ReadInt32();
                            int health = msgIn.ReadInt32();

                            int maxMana = msgIn.ReadInt32();

                            int mana = msgIn.ReadInt32();

                            byte level = msgIn.ReadByte();
                            ModelType modelType = (ModelType)msgIn.ReadByte();


                            float x = msgIn.ReadFloat();
                            float z = msgIn.ReadFloat();
                            float movementSpeed = msgIn.ReadFloat();

                            ushort END = msgIn.ReadUInt16();
                            ushort STR = msgIn.ReadUInt16();
                            ushort AGI = msgIn.ReadUInt16();
                            ushort INT = msgIn.ReadUInt16();
                            ushort KNW = msgIn.ReadUInt16();
                            ushort LCK = msgIn.ReadUInt16();

                            long experience = msgIn.ReadInt64();
                            long maxExperience = msgIn.ReadInt64();

                            AuthoryClient.Handler.SetMapSize(msgIn.ReadInt32());

                            GameObject entity = AuthoryClient.Handler.GetEntityObjectByModelType(modelType).gameObject;

                            GameObject.Destroy(entity.GetComponent<Entity>());

                            PlayerEntity playerEntity = entity.AddComponent<PlayerEntity>();
                            entity.AddComponent<PlayerMove>();

                            Camera.main.GetComponent<CameraOrbit>().Target = playerEntity.transform;

                            AuthoryData.Instance.SetPlayer(playerEntity, Id, Name);

                            playerEntity.transform.position = new Vector3(x, 0, z);

                            playerEntity.SetMaxHealth(maxHealth);
                            playerEntity.SetHealth((ushort)health);

                            playerEntity.SetMaxMana(maxMana);
                            playerEntity.SetMana((ushort)mana);

                            playerEntity.Level = level;

                            playerEntity.MovementSpeed = movementSpeed;

                            playerEntity.Attributes.Endurance = END;
                            playerEntity.Attributes.Strength = STR;
                            playerEntity.Attributes.Agility = AGI;
                            playerEntity.Attributes.Intelligence = INT;
                            playerEntity.Attributes.Knowledge = KNW;
                            playerEntity.Attributes.Luck = LCK;

                            AuthoryClient.UIController.Player = playerEntity;
                            AuthoryClient.UIController.UpdateMaxExperience(maxExperience, experience);

                            playerInfoArrived = true;
                            break;
                        }
                    }
                }
        AuthorySender.Movement();
    }

    /// <summary>
    /// Calls the Clients read method for reading messages, and drops back to login screen if connection disconnected.
    /// </summary>
    void Update()
    {
        if (AuthoryClient.Client != null && AuthoryClient.Client.ServerConnection == null)
        {
            AuthoryClient.Data.Clear();
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            SceneManager.LoadScene(0);
        }

        AuthoryClient.Read();
    }
}
