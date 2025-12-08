using DG.Tweening;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int ColorId = -1;
    [SerializeField] private FaceEmoji faceEmoji;
    [SerializeField] private GameObject bodyEnemy;
    [SerializeField] private GameObject blastParticle;

    public bool hasAim = false;
    private int aimedByCanonID = -1;
    public bool hasActiveBullet { get; private set; } = false;
    private int bulletOwnerID = -1;

    public int col = -1;
    public int row = -1;
    private bool isBeingDestroyed = false;

    public void SetEmoji()
    {
        faceEmoji.Play("EnemyIdle");
    }

    public void SetEmojiVisibility(bool visible)
    {
        if (faceEmoji != null)
        {
            faceEmoji.gameObject.SetActive(visible);
        }
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }
    public void SetInfo(int c, int r)
    {
        col = c;
        row = r;
    }
    public void SetColor(int colorId)
    {
        ColorId = colorId;
        var r = bodyEnemy.GetComponent<Renderer>();
        if (r != null) r.material = GameConfig.Instance.GetColorEnemy(ColorId);
    }


    public void HasAim(bool aim)
    {
        hasAim = aim;
        if (!aim)
        {
            aimedByCanonID = -1;
        }
    }

    public bool TryClaimAim(int canonInstanceID)
    {
        if (isBeingDestroyed)
        {
            return false;
        }

        if (hasActiveBullet)
        {
            return false;
        }

        if (hasAim && aimedByCanonID != -1 && aimedByCanonID != canonInstanceID)
        {
            return false;
        }

        hasAim = true;
        aimedByCanonID = canonInstanceID;

        return true;
    }

    public bool LockForBullet(int canonInstanceID, int bulletInstanceID)
    {
        if (!hasAim || aimedByCanonID != canonInstanceID)
        {
            return false;
        }

        hasActiveBullet = true;
        bulletOwnerID = bulletInstanceID;

        return true;
    }

    public bool CanBulletDestroy(int bulletInstanceID)
    {
        return hasActiveBullet && bulletOwnerID == bulletInstanceID;
    }

    public void ResetAim()
    {
        hasAim = false;
        aimedByCanonID = -1;
    }

    public void ResetAllLocks()
    {
        hasAim = false;
        aimedByCanonID = -1;
        hasActiveBullet = false;
        bulletOwnerID = -1;
        isBeingDestroyed = false;
    }

    public void ResetBulletLock()
    {
        hasActiveBullet = false;
        bulletOwnerID = -1;
    }

    ParticleSystem ps;
    public void OnHit()
    {
        isBeingDestroyed = true;
        hasActiveBullet = false;
        bulletOwnerID = -1;

        faceEmoji.Play("EnemyHit");
        AudioController.Instance.EnemyHit();
        Sequence hitSequence = DOTween.Sequence();
        Vector3 originalPosition = transform.localPosition;
        float jumpHeight = Random.Range(0.5f, 0.9f);
        hitSequence.Append(transform.DOLocalMoveY(originalPosition.y + jumpHeight, 0.08f).SetEase(Ease.OutQuad));
        float randomScale = Random.Range(1.01f, 1.02f);
        var scaleUpTween = transform.DOScale(randomScale, 0.02f).SetEase(Ease.OutQuad);
        hitSequence.Join(scaleUpTween);
        scaleUpTween.OnComplete(() =>
        {
            GameObject objParticle = PoolManager.Instance.GetFromPool(BoardController.Instance.blastParticle);
            ps = objParticle.GetComponent<ParticleSystem>();
            ps.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
            objParticle.SetActive(true);

            var r = bodyEnemy.GetComponent<Renderer>();
            Color colorParticle = GameConfig.Instance.GetColorBlood(ColorId).color;
            ps.GetComponent<ColorParticle>().setColorParticle(colorParticle);
            ps.Play();
        });
        float tiltX = Random.Range(10, 30f);
        float tiltY = Random.Range(-20f, 20f);
        Vector3 targetRotation = new Vector3(tiltX, tiltY, transform.localEulerAngles.z);
        hitSequence.Join(transform.DOLocalRotate(targetRotation, 0.05f).SetEase(Ease.Linear));
        hitSequence.Append(transform.DOScale(0f, 0.02f).SetEase(Ease.InBack).SetDelay(0.05f));
        hitSequence.OnComplete(() => { gameObject.SetActive(false); });
    }
}
