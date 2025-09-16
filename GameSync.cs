﻿using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;

namespace SilklessCoop
{
    internal class GameSync : MonoBehaviour
    {

        private static float stof(string s)
        {
            float f1;
            try { f1 = float.Parse(s.Replace(",", ".")); } catch (Exception) { f1 = float.MaxValue; }
            float f2;
            try { f2 = float.Parse(s.Replace(".", ",")); } catch (Exception) { f2 = float.MaxValue; }

            if (Mathf.Abs(f1) < Mathf.Abs(f2)) return f1;
            else return f2;
        }

        public ManualLogSource Logger;
        public ModConfig Config;

        // sprite sync - self
        private GameObject _hornetObject = null;
        private tk2dSprite _hornetSprite = null;
        private Rigidbody2D _hornetRigidbody = null;

        // sprite sync - others
        private Dictionary<string, GameObject> _playerObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, tk2dSprite> _playerSprites = new Dictionary<string, tk2dSprite>();
        private Dictionary<string, SimpleInterpolator> _playerInterpolators = new Dictionary<string, SimpleInterpolator>();

        // player count
        private GameObject _pauseMenu = null;
        private int _playerCount = 0;
        private List<GameObject> _countPins = new List<GameObject>();

        // map sync - self
        private GameObject _mainQuests = null;
        private GameObject _map = null;
        private GameObject _compass = null;

        // map sync - others
        private Dictionary<string, GameObject> _playerCompasses = new Dictionary<string, GameObject>();
        private Dictionary<string, tk2dSprite> _playerCompassSprites = new Dictionary<string, tk2dSprite>();

        private bool _setup = false;
        public int geoEventAmount = 0;
        public int shardEventAmount = 0;
        public int damage = 0;
        public bool kill = false;

        private void Update()
        {
            if (!_hornetObject) _hornetObject = GameObject.Find("Hero_Hornet");
            if (!_hornetObject) _hornetObject = GameObject.Find("Hero_Hornet(Clone)");
            if (!_hornetObject) { _setup = false; return; }

            if (!_hornetSprite) _hornetSprite = _hornetObject.GetComponent<tk2dSprite>();
            if (!_hornetSprite) { _setup = false; return; }

            if (!_hornetRigidbody) _hornetRigidbody = _hornetObject.GetComponent<Rigidbody2D>();
            if (!_hornetRigidbody) { _setup = false; return; }

            if (!_map) _map = GameObject.Find("Game_Map_Hornet");
            if (!_map) _map = GameObject.Find("Game_Map_Hornet(Clone)");
            if (!_map) { _setup = false; return; }

            if (!_compass) _compass = _map.transform.Find("Compass Icon")?.gameObject;
            if (!_compass) { _setup = false; return; }

            if (!_pauseMenu) _pauseMenu = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "NewPauseMenuScreen");
            if (!_pauseMenu) { _setup = false; return; }

            if (!_mainQuests) _mainQuests = _map.transform.Find("Main Quest Pins")?.gameObject;
            if (!_mainQuests) { _setup = false; return; }


            if (Config.SyncCompasses)
            {
                foreach (GameObject g in _playerCompasses.Values)
                    if (g != null) g.SetActive(_mainQuests.activeSelf);
            }

            foreach (GameObject g in _countPins)
                if (g != null) g.SetActive(_mainQuests.activeSelf);

            if (!_setup)
            {
                _setup = true;

                //LogChildrenTree(_hornetObject.transform.root.gameObject, 0);

                Logger.LogInfo("GameObject setup complete.");
                //Logger.LogInfo($"Rosaries: {_hornetObject.GetType()}");

                Assembly assembly = Assembly.GetExecutingAssembly();
                // Or for a specific assembly by name:
                // Assembly assembly = Assembly.Load("YourAssemblyFileName"); 
                var exports =
                assembly.GetExportedTypes()
                        .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                                .Select(member => new
                                                {
                                                    type.FullName,
                                                    Member = member.ToString()
                                                }))
                        .ToList();

                var typesWithProperty = assembly.GetExportedTypes()
                                             .Where(type => type.GetProperty("rosaries") != null)
                                             .ToList();
                GameManager manager = GameObject.Find("_GameManager").GetComponent<GameManager>();

                //Logger.LogInfo($"Geo: {manager.playerData.geo}");
                //Logger.LogInfo($"PlayerPref: {typesWithProperty}");


            }
        }

        private void LogChildrenTree(GameObject parent, int level)
        {
            string[] ids = { "-", "+", "*" };
            string id = ids[level % 3];

            string tab = "";
            for (int i = 0; i < level; i++)
            {
                tab += "  ";
            }

            Logger.LogInfo($"{tab}{id} {parent.name} : {parent.tag}");
            foreach (Transform g in parent.transform)
            {
                LogChildrenTree(g.gameObject, level + 1);
            }
        }

        public string GetUpdateContent()
        {
            if (!_setup) return null;

            Transform xf = _hornetObject.transform;
            Vector2 vel = _hornetRigidbody.linearVelocity;
            UpdateData data = new()
            {
                x = xf.position.x,
                y = xf.position.y,
                z = xf.position.z,

                vx = vel.x,
                vy = vel.y,

                sprite = new SpriteData
                {
                    id = _hornetSprite.spriteId,
                    scaleX = xf.localScale.x,
                    scaleY = xf.localScale.y
                }
            };

            if (Config.SyncCompasses)
            {
                data.compass = new CompassData
                {
                    active = _compass.activeSelf,
                    x = _compass.transform.localPosition.x,
                    y = _compass.transform.localPosition.x,
                };
            }

            if (geoEventAmount != 0)
            {
                Logger.LogInfo($"Update Geo Event with {geoEventAmount} geos");
                data.geoEvent = new GeoEvent { amount = geoEventAmount, };
                geoEventAmount = 0;
            }

            if (shardEventAmount != 0)
            {
                data.shardEvent = new GeoEvent { amount = shardEventAmount, };
                shardEventAmount = 0;
            }

            if (damage > 0)
            {
                data.damage = damage;
                damage = 0;
            }
            if (kill)
            {
                data.deathEvent = true;
                kill = false;
            }

            return JsonConvert.SerializeObject(data);



            /*string scene = SceneManager.GetActiveScene().name;
            float posX = _hornetObject.transform.position.x;
            float posY = _hornetObject.transform.position.y;
            float posZ = _hornetObject.transform.position.z;
            int spriteId = _hornetSprite.spriteId;
            float scaleX = _hornetObject.transform.localScale.x;
            float vX = _hornetRigidbody.linearVelocity.x;
            float vY = _hornetRigidbody.linearVelocity.y;

            int compassActive = 0;
            float compassX = 0;
            float compassY = 0;

            if (Config.SyncCompasses)
            {
                compassActive = _compass.activeSelf ? 1 : 0;
                compassX = _compass.transform.localPosition.x;
                compassY = _compass.transform.localPosition.y;
            }

            string baseData = $"{scene}:{posX}:{posY}:{posZ}:{spriteId}:{scaleX}:{vX}:{vY}";
            string compassData = Config.SyncCompasses ? $":{compassActive}:{compassX}:{compassY}" : "";
            string data = $"{baseData}{compassData}";*/


            //return data;
        }

        public void ApplyUpdate(string data)
        {
            try
            {
                if (!_setup) return;

                //if (Config.PrintDebugOutput) Logger.LogInfo($"Applying update {data}...");

                UpdateUI();

                string[] parts = data.Split("::");
                string id = parts[0];
                string[] metadataParts = parts[1].Split(":");
                string contentParts = parts[2];

                _playerCount = int.Parse(metadataParts[0]);

                UpdateData content = JsonConvert.DeserializeObject<UpdateData>(contentParts);
                //UpdateData = JsonConv.Deserialize<UpdateData>(contentParts);

                /*string scene = contentParts[0];
                float posX = stof(contentParts[1]);
                float posY = stof(contentParts[2]);
                float posZ = stof(contentParts[3]);
                int spriteId = int.Parse(contentParts[4]);
                float scaleX = stof(contentParts[5]);
                float vX = stof(contentParts[6]);
                float vY = stof(contentParts[7]);*/

                bool compassActive = false;
                float compassX = 0;
                float compassY = 0;

                if (Config.SyncCompasses && content.compass != null)
                {
                    compassActive = content.compass.Value.active;
                    compassX = content.compass.Value.x;
                    compassY = content.compass.Value.y;
                }

                if (content.geoEvent != null)
                {
                    CurrencyManager.AddGeoQuietly(content.geoEvent.Value.amount);
                    CurrencyManager.AddGeoToCounter(content.geoEvent.Value.amount);
                }
                if (content.shardEvent != null)
                {
                    GameManager manager = GameObject.Find("_GameManager").GetComponent<GameManager>();
                    CurrencyManager.TakeShards(-content.shardEvent.Value.amount);
                }
                var hero = _hornetObject.GetComponent<HeroController>();
                if (content.deathEvent != null)
                {
                    hero.playerData.health = 0;
                }
                if (content.damage != null)
                {
                    hero.playerData.health = 1;
                    if (hero.playerData.health == 0)
                    {
                        kill = true;
                    }
                }

                bool sameScene = true;//scene == SceneManager.GetActiveScene().name;

                if (!_playerObjects.ContainsKey(id))
                {
                    _playerObjects.Add(id, null);
                    _playerSprites.Add(id, null);
                    _playerInterpolators.Add(id, null);
                }

                if (!_playerCompasses.ContainsKey(id))
                {
                    _playerCompasses.Add(id, null);
                    if (!_playerCompassSprites.ContainsKey(id)) _playerCompassSprites.Add(id, null);
                }

                if (!sameScene)
                {
                    // clear dupes if player leaves scene
                    if (_playerObjects.ContainsKey(id))
                        if (_playerObjects[id] != null)
                            Destroy(_playerObjects[id]);
                }
                else
                {
                    if (_playerObjects[id] != null)
                    {
                        // update player
                        _playerObjects[id].transform.position = new Vector3(content.x, content.y, content.z + 0.001f);
                        _playerObjects[id].transform.localScale = new Vector3(content.sprite.scaleX, content.sprite.scaleY, 1);
                        _playerSprites[id].spriteId = content.sprite.id;
                        _playerInterpolators[id].velocity = new Vector3(content.vx, content.vy, 0);
                    }
                    else
                    {
                        // create player
                        if (Config.PrintDebugOutput) Logger.LogInfo($"Creating new player object for player {id}...");

                        GameObject newObject = new GameObject();
                        newObject.SetName("SilklessCooperator");
                        newObject.transform.position = new Vector3(content.x, content.y, content.z + 0.001f);
                        newObject.transform.localScale = new Vector3(content.sprite.scaleX, content.sprite.scaleY, 1);

                        tk2dSprite newSprite = tk2dSprite.AddComponent(newObject, _hornetSprite.Collection, 0);
                        newSprite.color = new Color(1, 1, 1, Config.PlayerOpacity);

                        SimpleInterpolator newInterpolator = newObject.AddComponent<SimpleInterpolator>();
                        newInterpolator.velocity = new Vector3(content.vx, content.vy, 0);

                        _playerObjects[id] = newObject;
                        _playerSprites[id] = newSprite;
                        _playerInterpolators[id] = newInterpolator;

                        if (Config.PrintDebugOutput) Logger.LogInfo($"Successfully created new player object for player {id}.");
                    }
                }

                if (Config.SyncCompasses)
                {
                    if (compassActive)
                    {
                        if (_playerCompasses[id] != null)
                        {
                            // update compass
                            _playerCompasses[id].transform.localPosition = new Vector3(compassX, compassY, _compass.transform.localPosition.z + 0.001f);
                            _playerCompassSprites[id].color = new Color(1, 1, 1, Config.ActiveCompassOpacity);
                        }
                        else
                        {
                            // create compass
                            if (Config.PrintDebugOutput) Logger.LogInfo($"Creating new compass for player {id}...");

                            GameObject newObject = Instantiate(_compass, _map.transform);
                            newObject.SetName("SilklessCompass");
                            newObject.transform.localPosition = new Vector3(compassX, compassY, _compass.transform.localPosition.z + 0.001f);
                            tk2dSprite newSprite = newObject.GetComponent<tk2dSprite>();
                            newSprite.color = new Color(1, 1, 1, Config.ActiveCompassOpacity);

                            _playerCompasses[id] = newObject;
                            _playerCompassSprites[id] = newSprite;

                            if (Config.PrintDebugOutput) Logger.LogInfo($"Successfully created new compass for player {id}.");
                        }
                    }
                    else
                    {
                        if (_playerCompasses[id] != null)
                            _playerCompassSprites[id].color = new Color(1, 1, 1, Config.InactiveCompassOpacity);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while applying update: {e}");
            }
        }

        private void UpdateUI()
        {
            try
            {
                while (_countPins.Count < _playerCount)
                {
                    if (Config.PrintDebugOutput) Logger.LogInfo($"Creating player count pin {_countPins.Count + 1}...");

                    GameObject newPin = Instantiate(_compass, _map.transform);
                    newPin.SetName("SilklessPlayerCountPin");
                    _countPins.Add(newPin);

                    if (Config.PrintDebugOutput) Logger.LogInfo($"Successfully created player count pin {_countPins.Count}.");
                }

                while (_countPins.Count > _playerCount)
                {
                    if (Config.PrintDebugOutput) Logger.LogInfo($"Removing player count pin {_countPins.Count}...");

                    Destroy(_countPins[_countPins.Count - 1]);
                    _countPins.RemoveAt(_countPins.Count - 1);

                    if (Config.PrintDebugOutput) Logger.LogInfo($"Successfully removed player count pin {_countPins.Count + 1}.");
                }

                for (int i = 0; i < _countPins.Count; i++)
                    _countPins[i].transform.position = new Vector3(-14.8f + i * 0.9f, -8.2f, -5f);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while updating ui: {e}");
            }
        }

        public void Reset()
        {
            foreach (GameObject g in _playerObjects.Values)
                if (g != null) Destroy(g);
            _playerObjects.Clear();
            _playerSprites.Clear();
            _playerInterpolators.Clear();

            foreach (GameObject g in _countPins)
                if (g != null) Destroy(g);
            _countPins.Clear();

            foreach (GameObject g in _playerCompasses.Values)
                if (g != null) Destroy(g);
            _playerCompasses.Clear();
        }
    }
}
