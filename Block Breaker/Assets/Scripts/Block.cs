using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    // Config parameters
    [SerializeField] AudioClip BreakSound;
    [SerializeField] GameObject SparkleVFX; // input given as a prefab game-obj
    [SerializeField] int maxHits = 1; // here coded for variable max hits and block stages
    // so that, for not every hit the block changes its stage texture
    [SerializeField] Sprite[] BlockStages; // 0 to len -> high to low

    // Cached reference Components
    Level level;
    GameSession session;
    SpriteRenderer BlockSprite;
    // State Variables
    int HitsLeft;

    // Start is called before the first frame update
    void Start()
    {
        level = FindObjectOfType<Level>();
        session = FindObjectOfType<GameSession>();
        BlockSprite = GetComponent<SpriteRenderer>();

        HitsLeft = maxHits;
        ChangeSpriteStage(0);
        
        // counting only breakable blocks
        if (tag == "Breakable")
        {
            level.IncrCountBlocks();
            gameObject.name = "BB-" + level.getCount();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (tag == "Breakable")
        {
            HitsLeft--;
            if(HitsLeft <= 0)
                DestroyBlock();
            else
                for (int i = 1; i < BlockStages.Length; i++)
                {
                    if (HitsLeft <= maxHits*i/BlockStages.Length)
                    {
                        ChangeSpriteStage(BlockStages.Length - i);
                        break;
                    }
                }
        }
    }

    private void ChangeSpriteStage(int i)
    {
        if (BlockStages[i] != null)
            BlockSprite.sprite = BlockStages[i];
        else
            Debug.LogError("Block-Stage[" + i + "] of " + gameObject.name + " is missing.");
    }

    private void DestroyBlock()
    {
        // destroyed sound effect
        AudioSource.PlayClipAtPoint(BreakSound, transform.position);

        // decrease no.of break-able blocks in currrent level
        level.DecrCountBlocks();

        // add points per block
        session.AddBlockPoint();

        // show particle vfx
        TriggerSparklesVFX();

        // finally destroy block
        Destroy(gameObject);
    }

    private void TriggerSparklesVFX()
    {
        GameObject sparkles = Instantiate(SparkleVFX, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(sparkles,1f); //destroying created instance delayed after the animation is over
    }
}
