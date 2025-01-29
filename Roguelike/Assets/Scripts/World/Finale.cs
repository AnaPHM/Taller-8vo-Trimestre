using UnityEngine;
using System.Linq;

public class Finale : MonoBehaviour
{
    public GameObject[] enemies;
    public Transition_Manager manager;

    
    void Update()
    {
        
        if (enemies.All(enemy => enemy == null))
            manager.Victory();

    }
}