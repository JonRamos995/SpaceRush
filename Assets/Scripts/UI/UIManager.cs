using UnityEngine;

namespace SpaceRush.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Sub-Controllers")]
        public HUDController hudController;
        public LocationListController locationListController;
        public PlanetDashboardController dashboardController;
        public LogWindowController logWindowController;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Optional: Any central initialization of UI components
            // Currently each controller handles its own Update/Event subscription
        }
    }
}
