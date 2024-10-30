using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NpcBehaviorTree : MonoBehaviour
{
    private Transform Target;
    private Animator Anime;

    private bool isPlay = false;
    private float timer = 0;
    private const string label = "Hello";    

    void Start()
    {
        Target = gameObject.GetComponent<Transform>();
        Anime = gameObject.GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {        
        if(other.gameObject.tag == "Player")
        {            
            if(isPlay)
            {
                return;
            }

            isPlay = true;
            Anime.Play(label);  
            
            while(isPlay)
            {
                timer += Time.deltaTime;
                if(timer >= 1)
                {
                    isPlay = false;
                    break;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Vector3 dir = other.transform.position - Target.position;
            dir.y = 0;
            Quaternion rotation = Quaternion.LookRotation(dir);
            Target.rotation = Quaternion.Slerp(Target.rotation, rotation, Time.deltaTime * 10f);            
        }
    }
}
