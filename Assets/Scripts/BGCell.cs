using UnityEngine;

public class BGCell : MonoBehaviour
{
    [HideInInspector] public bool IsBlocked;
    [HideInInspector] public bool IsFilled;

    [SerializeField] private SpriteRenderer _bgSprite;
    [SerializeField] private Sprite _emptySprite;
    [SerializeField] private Sprite _blockedSprite;
    [SerializeField] private Color _startColor;
    [SerializeField] private Color _correctColor;
    [SerializeField] private Color _incorrectColor;

    public void Init(int blockValue)
    {
        IsBlocked = blockValue == -1;
        if(IsBlocked)
        {
            IsFilled = true;
        }
        _bgSprite.sprite = IsBlocked ? _blockedSprite : _emptySprite;
    }

    public void ResetHighLight()
    {
        _bgSprite.color = _startColor;
    }

    public void UpdateHighlight(bool isCorrect)
    {
        _bgSprite.color = isCorrect ? _correctColor : _incorrectColor;
    }
}
