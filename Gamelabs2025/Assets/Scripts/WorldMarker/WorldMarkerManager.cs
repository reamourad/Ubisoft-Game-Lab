using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.WorldMarker
{
    public class WorldMarkerManager : MonoBehaviour
    {
        [SerializeField] private GameObject worldMarkerReference;
        
        
        private Dictionary<string,WorldMarker> worldMarkers = new Dictionary<string, WorldMarker>(); 
        
        private static WorldMarkerManager instance;
        public static WorldMarkerManager Instance
        {
            get
            {
                if (instance == null)
                    instance = CreateMarkerCanvas();
                return instance;
            }
        }
        
        private static WorldMarkerManager CreateMarkerCanvas()
        {
            var go = Instantiate(Resources.Load("WorldMarker")) as GameObject;
            return go.GetComponent<WorldMarkerManager>();
        }

        private void Update()
        {
            foreach (var marker in worldMarkers)
            {
                if (marker.Value != null)
                    marker.Value.UpdatePositions();
            }
        }

        public string AddWorldMarker(Transform markerTransform, Sprite worldMarkerIcon, bool animate)
        {
            var go = Instantiate(worldMarkerReference, transform);
            go.SetActive(true);
            var marker = go.GetComponent<WorldMarker>();
            marker.Set(markerTransform, worldMarkerIcon, animate);
            var id = Guid.NewGuid().ToString();
            worldMarkers.Add(id, marker);
            return id;
        }
        
        public void DestroyMarker(string id)
        {
            if (!worldMarkers.ContainsKey(id))
            {
                Debug.Log($"Destroy KEY NOT FOUND {id}");
                return;
            }

            Debug.Log($"Destroy {id}");
            if(worldMarkers[id] != null)
                Destroy(worldMarkers[id].gameObject);
            worldMarkers.Remove(id);
        }

        public void ClearWorldMarkers()
        {
            foreach (var marker in worldMarkers)
            {
                if(marker.Value != null)
                    Destroy(marker.Value.gameObject);
            }
            
            worldMarkers.Clear();
        }
    }
}