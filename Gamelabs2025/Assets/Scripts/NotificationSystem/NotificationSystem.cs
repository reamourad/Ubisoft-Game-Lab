using UnityEngine;
using Utils;

namespace Player.NotificationSystem
{
    public class NotificationSystem : SingletonBehaviour<NotificationSystem>
    {
        [SerializeField] private Transform notificationParent;
        [SerializeField] private GameObject notificationRef;

        public void Notify(string message)
        {
            var go = Instantiate(notificationRef, notificationParent);
            go.SetActive(true);
            go.GetComponent<NotificationObject>().Setup(message);
        }
    }
}