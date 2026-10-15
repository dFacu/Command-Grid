using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMovement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private RectTransform canvasRectTranform;
    private Vector3 originalScale;
    private int currentState = 0;
    private Quaternion originalRotation;
    private Vector3 originalPosition;

    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private Vector2 cardPlay;
    [SerializeField] private Vector3 playPosition;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private GameObject playArrow;
    [SerializeField] private float lerpFactor = 0.1f;
    [SerializeField] private int cardPlayDivider = 4;
    [SerializeField] private float cardPlayMultiplier = 1f;
    [SerializeField] private bool needUpdateCardPlayPosition = false;
    [SerializeField] private int playPositionYDivider = 2;
    [SerializeField] private float playPositionYMultiplier = 1f;
    [SerializeField] private int playPositionXDivider = 4;
    [SerializeField] private float playPositionXMultiplier = 1f;
    [SerializeField] private bool needUpdatePlayPosition = false;

    [SerializeField] private GridControler gridManager;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            canvasRectTranform = canvas.GetComponent<RectTransform>();
        }

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.localPosition;
        originalRotation = rectTransform.localRotation;

        updateCardPlayPostion();
        updatePlayPostion();

        gridManager = FindObjectOfType<GridControler>();
    }

    void Update()
    {
        if (needUpdateCardPlayPosition)
        {
            updateCardPlayPostion();
        }

        if (needUpdatePlayPosition)
        {
            updatePlayPostion();
        }

        switch (currentState)
        {
            case 1:
                HandleHoverState();
                break;
            case 2:
                HandleDragState();
                if (!Input.GetMouseButton(0)) //Check if mouse button is released
                {
                    TransitionToState0();
                }
                break;
            case 3:
                HandlePlayState();
                break;
        }
    }

    private void TransitionToState0()
    {
        currentState = 0;
        
        rectTransform.localScale = originalScale; //Reset Scale
        rectTransform.localRotation = originalRotation; //Reset Rotation
        rectTransform.localPosition = originalPosition; //Reset Position
        glowEffect.SetActive(false); //Disable glow effect
        playArrow.SetActive(false); //Disable playArrow
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == 0)
        {
            originalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;
            originalScale = rectTransform.localScale;

            currentState = 1;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            TransitionToState0();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            currentState = 2;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentState == 2)
        {
            CardUI card = GetComponent<CardUI>();

            if (card.cost <= Manager.Instance.player.currentPoints)
            {
                if (Input.mousePosition.y > cardPlay.y)
                {
                    currentState = 3;
                    playArrow.SetActive(true);
                    rectTransform.localPosition = Vector3.Lerp(rectTransform.position, playPosition, lerpFactor);
                }
            }
        }
    }

    private void HandleHoverState()
    {
        CardUI card = GetComponent<CardUI>();

        if (card.cost <= Manager.Instance.player.currentPoints)
        {
            glowEffect.SetActive(true);
            rectTransform.localScale = originalScale * selectScale;
        }

    }

    private void HandleDragState()
    {
        CardUI card = GetComponent<CardUI>();

        if (card.cost <= Manager.Instance.player.currentPoints)
        {
            //Set the card's rotation to zero
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.position = Vector3.Lerp(rectTransform.position, Input.mousePosition, lerpFactor);
        }
    }

    private void HandlePlayState()
    {
        rectTransform.localPosition = playPosition;
        rectTransform.localRotation = Quaternion.identity;

        if (!Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                TileClick cell = hit.collider.GetComponent<TileClick>();
                CardUI card = GetComponent<CardUI>();

                if (cell != null)
                {
                    if ((card.effectType == EffectType.SpawnUnit))   // Invoca una unidad 
                    {
                        Vector3 targetPos = cell.cellCoords;


                        if (gridManager.AddObjectToGrid(GetComponent<CardUI>().Prefab, targetPos, Manager.Instance.player))
                        {
                            HandManager handManager = FindAnyObjectByType<HandManager>();
                            handManager.cardsInHand.Remove(gameObject);
                            handManager.UpdateHandVisuals();
                            Debug.Log("character");
                            DiscardManager discardManager = FindAnyObjectByType<DiscardManager>();
                            discardManager.AddToDiscard(GetComponent<CardUI>().cardData);
                            Manager.Instance.player.currentPoints -= card.cost;
                            Destroy(gameObject);
                        }
                    }
                    else if (card.effectType == EffectType.Trap)
                    {
                        cell.hasTrap = true;
                        cell.trapDamage = card.value;
                        Renderer cellRender = cell.GetComponent<Renderer>();
                        cellRender.material.color = Color.azure;
                        Debug.Log($"Trampa colocada en {cell.name} con daño {cell.trapDamage}");
                        DiscardManager discardManager = FindAnyObjectByType<DiscardManager>();
                        discardManager.AddToDiscard(GetComponent<CardUI>().cardData);

                        HandManager handManager = FindAnyObjectByType<HandManager>();
                        handManager.cardsInHand.Remove(gameObject);
                        handManager.UpdateHandVisuals();
                        Manager.Instance.player.currentPoints -= card.cost;
                        Destroy(gameObject);
                    }

                }
                else
                {
                    if (card.effectType == EffectType.Damage)
                    {
                        Unit target = hit.collider.GetComponent<Unit>();

                        if (target != null)
                        {
                            // Posición de la unidad seleccionada
                            Vector3 attackerPos = Manager.Instance.player.selectedUnit.transform.position;
                            Vector3 targetPos = target.transform.position;

                            // Calcular distancia
                            float distance = Vector3.Distance(attackerPos, targetPos);

                            // Chequear si está dentro del rango permitido por la carta
                            if (distance <= card.distance)
                            {
                                target.TakeDamage(card.value);
                                Debug.Log($"{card.name} infligió {card.value} de daño a {target.name} (distancia: {distance})");
                                DiscardManager discardManager = FindAnyObjectByType<DiscardManager>();
                                discardManager.AddToDiscard(GetComponent<CardUI>().cardData);

                                HandManager handManager = FindAnyObjectByType<HandManager>();
                                handManager.cardsInHand.Remove(gameObject);
                                handManager.UpdateHandVisuals();
                                Manager.Instance.player.currentPoints -= card.cost;
                                Destroy(gameObject);

                            }
                            else
                            {
                                Debug.Log($"{card.name} no puede atacar: objetivo fuera de rango (distancia: {distance}, rango: {card.distance})");
                            }
                        }
                    }

                }
            }

            TransitionToState0();
        }



        if (Input.mousePosition.y < cardPlay.y)
        {
            currentState = 2;
            playArrow.SetActive(false);
        }
    }

    private void updateCardPlayPostion()
    {
        if (cardPlayDivider != 0 && canvasRectTranform != null)
        {
            float segment = cardPlayMultiplier / cardPlayDivider;

            cardPlay.y = canvasRectTranform.rect.height * segment;
        }
    }

    private void updatePlayPostion()
    {
        if (canvasRectTranform != null && playPositionYDivider != 0 && playPositionXDivider != 0)
        {
            float segmentX = playPositionXMultiplier / playPositionXDivider;
            float segmentY = playPositionYMultiplier / playPositionYDivider;

            playPosition.x = canvasRectTranform.rect.width * segmentX;
            playPosition.y = canvasRectTranform.rect.height * segmentY;
        }
    }
}
