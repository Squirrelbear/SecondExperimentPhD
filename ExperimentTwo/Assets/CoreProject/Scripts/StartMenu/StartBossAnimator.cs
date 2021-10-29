using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartBossAnimator : MonoBehaviour {

    public List<AnimationClip> anims;
    public float maxTimeBetweenAnims = 10;
    public AnimationClip curAnim;

    private float counter;
    //private Random rand;
    private Animation anim;

	// Use this for initialization
	void Start () {
	    //rand = new Random();
        anim = gameObject.GetComponentInChildren<Animation>();
	}
	
	// Update is called once per frame
	void Update () {
        counter += Time.deltaTime;
        if(!anim.isPlaying || counter > maxTimeBetweenAnims)
        {
            counter %= maxTimeBetweenAnims;
            if (anims != null && anim != null)
            {
                curAnim = anims[Random.Range(0, anims.Count)];
                anim.Play(curAnim.name);
            }
        }
	}
}
