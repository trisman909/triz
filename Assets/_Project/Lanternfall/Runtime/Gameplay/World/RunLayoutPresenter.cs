using System.Collections.Generic;
using Lanternfall.Core.Run;
using UnityEngine;

namespace Lanternfall.Gameplay.World
{
    /// <summary>
    /// Runtime presentation of a deterministic route graph. Authored room
    /// templates will replace the greybox view without changing graph rules.
    /// </summary>
    public sealed class RunLayoutPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject roomTemplate;
        [SerializeField] private ulong seed = 20260701;
        [SerializeField, Range(6, 20)] private int mainPathLength = 12;
        [SerializeField] private float depthSpacing = 10f;
        [SerializeField] private float branchSpacing = 8f;

        private readonly List<GameObject> _spawned = new List<GameObject>(32);
        private MaterialPropertyBlock _properties;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public ulong Seed => seed;
        public IReadOnlyList<GameObject> SpawnedRooms => _spawned;

        private void Start() => Generate(seed);

        public void Configure(GameObject template, ulong runSeed, int pathLength)
        {
            roomTemplate = template;
            seed = runSeed;
            mainPathLength = pathLength;
        }

        public void Generate(ulong runSeed)
        {
            Clear();
            if (roomTemplate == null) return;
            seed = runSeed;
            _properties = _properties ?? new MaterialPropertyBlock();
            IReadOnlyList<RoomNode> graph =
                new RoomGraphGenerator().Generate(seed, mainPathLength);

            for (int index = 0; index < graph.Count; index++)
            {
                RoomNode node = graph[index];
                int lane = node.Id < mainPathLength ? 0 : node.Id - mainPathLength + 1;
                Vector3 position = new Vector3(
                    node.Depth * depthSpacing,
                    0f,
                    lane * branchSpacing);
                GameObject room = Instantiate(roomTemplate, position, Quaternion.identity, transform);
                room.name = $"{node.Id:00} {node.Kind}";
                room.SetActive(true);
                Renderer renderer = room.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.GetPropertyBlock(_properties);
                    _properties.SetColor(BaseColor, KindColor(node.Kind));
                    renderer.SetPropertyBlock(_properties);
                }
                _spawned.Add(room);
            }

            for (int source = 0; source < graph.Count; source++)
            {
                foreach (int target in graph[source].Connections)
                    CreateConnection(_spawned[source].transform.position, _spawned[target].transform.position);
            }
        }

        private void Clear()
        {
            for (int index = 0; index < _spawned.Count; index++)
                if (_spawned[index] != null) Destroy(_spawned[index]);
            _spawned.Clear();
        }

        private void CreateConnection(Vector3 start, Vector3 end)
        {
            GameObject connection = GameObject.CreatePrimitive(PrimitiveType.Cube);
            connection.name = "Route";
            connection.transform.SetParent(transform);
            connection.transform.position = (start + end) * 0.5f + Vector3.up * 0.05f;
            Vector3 delta = end - start;
            connection.transform.rotation = Quaternion.LookRotation(delta);
            connection.transform.localScale = new Vector3(1.4f, 0.15f, delta.magnitude);
            Collider collider = connection.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            _spawned.Add(connection);
        }

        private static Color KindColor(RoomKind kind)
        {
            switch (kind)
            {
                case RoomKind.Start: return new Color(0.25f, 0.75f, 0.85f);
                case RoomKind.Elite: return new Color(0.85f, 0.25f, 0.14f);
                case RoomKind.Treasure: return new Color(0.9f, 0.68f, 0.15f);
                case RoomKind.Shop: return new Color(0.35f, 0.85f, 0.45f);
                case RoomKind.Shrine: return new Color(0.7f, 0.4f, 0.9f);
                case RoomKind.Puzzle: return new Color(0.2f, 0.65f, 0.8f);
                case RoomKind.Secret: return new Color(0.18f, 0.14f, 0.26f);
                case RoomKind.Challenge: return new Color(1f, 0.42f, 0.08f);
                case RoomKind.MiniBoss: return new Color(0.75f, 0.12f, 0.2f);
                case RoomKind.Boss: return new Color(0.55f, 0.02f, 0.06f);
                case RoomKind.Event: return new Color(0.25f, 0.8f, 0.7f);
                case RoomKind.Healing: return new Color(0.25f, 0.9f, 0.55f);
                default: return new Color(0.32f, 0.34f, 0.38f);
            }
        }
    }
}

