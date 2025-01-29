using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance; //Singleton para acceder facilmente al pool desde cualquier lugar

    public GameObject bulletPrefab; //Prefab de las balas
    public int poolSize = 10; //Tamaño del pool

    private Queue<GameObject> bulletPool = new Queue<GameObject>(); //Cola de balas reutilizables

    private void Awake()
    {
        Instance = this;
        CreatePool();
    }

    void CreatePool()
    {
        for(int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false); //Desactivar la bala al principio
            bulletPool.Enqueue(bullet); //Añadir la bala a la cola
        }
    }

    public GameObject GetBullet()
    {
        if(bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);  //Activar la bala antes de devolverla
            return bullet;
        }
        else
        {
            //Si el pool esta vacio, se puede instanciar una nueva bala o expandir el pool
            GameObject bullet = Instantiate(bulletPrefab);
            return bullet;
        }
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false); //Desactivar la bala
        bulletPool.Enqueue(bullet); //Volver a añadir la bala al pool
    }

}
